using API.Extensions;
using API.Models;
using API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConversationsController : ControllerBase {
    private readonly IConversationRepository _conversationRepository;
    private readonly IAgentActionRepository _actionRepository;
    private readonly ILogger<ConversationsController> _logger;

    public ConversationsController(
        IConversationRepository conversationRepository,
        IAgentActionRepository actionRepository,
        ILogger<ConversationsController> logger) {
        _conversationRepository = conversationRepository;
        _actionRepository = actionRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetConversations() {
        var user = this.GetCurrentUser();
        if (user == null) {
            return this.UnauthorizedUser();
        }

        _logger.LogInformation("Fetching conversations for user: {Email}", user.Email);

        var conversations = await _conversationRepository.GetByUserIdAsync(user.Id);

        return Ok(conversations.Select(c => new {
            c.Id,
            c.Title,
            c.CreatedAt,
            c.UpdatedAt,
            c.ActionCount
        }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetConversation(int id) {
        var user = this.GetCurrentUser();
        if (user == null) {
            return this.UnauthorizedUser();
        }

        _logger.LogInformation("Fetching conversation {Id} for user: {Email}", id, user.Email);

        var conversation = await _conversationRepository.GetByIdAsync(id);

        if (conversation == null) {
            return NotFound(new { error = "Conversation not found" });
        }

        if (conversation.UserId != user.Id) {
            _logger.LogWarning("User {Email} attempted to access conversation {Id} owned by user {OwnerId}",
                user.Email, id, conversation.UserId);
            return Forbid();
        }

        var actions = await _actionRepository.GetByConversationIdAsync(id);

        return Ok(new {
            conversation.Id,
            conversation.Title,
            conversation.CreatedAt,
            conversation.UpdatedAt,
            Actions = actions.Select(a => new {
                a.Id,
                a.ActionType,
                a.Status,
                a.InputPrompt,
                a.Parameters,
                a.Result,
                a.ErrorMessage,
                a.CreatedAt,
                a.CompletedAt
            })
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request) {
        var user = this.GetCurrentUser();
        if (user == null) {
            return this.UnauthorizedUser();
        }

        _logger.LogInformation("Creating conversation for user: {Email}", user.Email);

        var conversation = new Conversation {
            UserId = user.Id,
            Title = request.Title,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        conversation = await _conversationRepository.CreateAsync(conversation);

        return CreatedAtAction(nameof(GetConversation), new { id = conversation.Id }, new {
            conversation.Id,
            conversation.Title,
            conversation.CreatedAt,
            conversation.UpdatedAt
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteConversation(int id) {
        var user = this.GetCurrentUser();
        if (user == null) {
            return this.UnauthorizedUser();
        }

        _logger.LogInformation("Deleting conversation {Id} for user: {Email}", id, user.Email);

        var conversation = await _conversationRepository.GetByIdAsync(id);

        if (conversation == null) {
            return NotFound(new { error = "Conversation not found" });
        }

        if (conversation.UserId != user.Id) {
            _logger.LogWarning("User {Email} attempted to delete conversation {Id} owned by user {OwnerId}",
                user.Email, id, conversation.UserId);
            return Forbid();
        }

        await _conversationRepository.DeleteAsync(id);

        return NoContent();
    }
}

public record CreateConversationRequest(string Title);