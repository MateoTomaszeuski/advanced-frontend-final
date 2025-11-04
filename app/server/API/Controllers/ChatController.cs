using Microsoft.AspNetCore.Mvc;
using API.Services;
using API.DTOs.Chat;
using API.Repositories;
using System.Text.Json;
using API.Models.AI;
using API.Services.Helpers;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAIService _aiService;
    private readonly IAgentService _agentService;
    private readonly IConversationRepository _conversationRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IUserService userService,
        IAIService aiService,
        IAgentService agentService,
        IConversationRepository conversationRepository,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ChatController> logger)
    {
        _userService = userService;
        _aiService = aiService;
        _agentService = agentService;
        _conversationRepository = conversationRepository;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequestDto request)
    {
        var email = _httpContextAccessor.HttpContext?.User.FindFirst("email")?.Value;
        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized(new { error = "User not authenticated" });
        }

        var user = await _userService.GetOrCreateUserAsync(email);
        
        try
        {
            Models.Conversation? conversation;

            if (request.ConversationId.HasValue)
            {
                conversation = await _conversationRepository.GetByIdAsync(request.ConversationId.Value);
                
                if (conversation == null || conversation.UserId != user.Id)
                {
                    return NotFound(new { error = "Conversation not found or access denied" });
                }
            }
            else
            {
                conversation = new Models.Conversation
                {
                    UserId = user.Id,
                    Title = "New Conversation",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                conversation = await _conversationRepository.CreateAsync(conversation);
            }

            var aiMessages = new List<AIMessage>
            {
                new AIMessage("system", AITools.GetSystemPrompt())
            };

            aiMessages.AddRange(request.Messages.Select(m => new AIMessage(m.Role, m.Content)));

            var tools = AITools.GetSpotifyTools();
            var aiResponse = await _aiService.GetChatCompletionAsync(aiMessages, tools);

            if (aiResponse.Error != null)
            {
                return Ok(new
                {
                    response = "I'm sorry, I encountered an error processing your request.",
                    error = aiResponse.Error,
                    conversationId = conversation.Id
                });
            }

            if (aiResponse.ToolCalls != null && aiResponse.ToolCalls.Count > 0)
            {
                var toolResults = new List<object>();
                var toolExecutionHelper = new ToolExecutionHelper(_agentService, _logger);

                foreach (var toolCall in aiResponse.ToolCalls)
                {
                    object result = toolCall.Name switch
                    {
                        "create_smart_playlist" => await toolExecutionHelper.ExecuteCreateSmartPlaylistAsync(user, conversation.Id, toolCall.Arguments),
                        "discover_new_music" => await toolExecutionHelper.ExecuteDiscoverNewMusicAsync(user, conversation.Id, toolCall.Arguments),
                        _ => new { success = false, message = $"Unknown tool: {toolCall.Name}" }
                    };

                    toolResults.Add(new { id = toolCall.Id, name = toolCall.Name, result });
                }

                var messagesWithTools = new List<AIMessage>(aiMessages)
                {
                    new AIMessage(
                        "assistant",
                        aiResponse.Response,
                        aiResponse.ToolCalls.Select(tc => new AIToolCall(
                            tc.Id,
                            "function",
                            new AIFunction(tc.Name, JsonSerializer.Serialize(tc.Arguments))
                        )).ToList()
                    )
                };

                foreach (var toolResult in toolResults)
                {
                    var resultJson = JsonSerializer.Serialize(toolResult);
                    var toolCallId = JsonSerializer.Serialize(toolResult).Contains("id") ? 
                        JsonDocument.Parse(JsonSerializer.Serialize(toolResult)).RootElement.GetProperty("id").GetString() : "";
                    messagesWithTools.Add(new AIMessage("tool", resultJson, ToolCallId: toolCallId));
                }

                var finalResponse = await _aiService.GetChatCompletionAsync(messagesWithTools, tools);
                
                return Ok(new
                {
                    response = finalResponse.Response,
                    conversationId = conversation.Id,
                    toolCalls = toolResults
                });
            }

            return Ok(new { response = aiResponse.Response, conversationId = conversation.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in chat endpoint");
            return StatusCode(500, new { error = "An error occurred processing your request" });
        }
    }
}
