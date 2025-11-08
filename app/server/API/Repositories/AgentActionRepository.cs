using API.Data;
using API.Models;
using Dapper;
using System.Text.Json;

namespace API.Repositories;

public interface IAgentActionRepository
{
    Task<AgentAction?> GetByIdAsync(int id);
    Task<IEnumerable<AgentAction>> GetByConversationIdAsync(int conversationId);
    Task<AgentAction> CreateAsync(AgentAction action);
    Task UpdateAsync(AgentAction action);
    Task DeleteAsync(int id);
    Task<IEnumerable<AgentAction>> GetRecentPlaylistCreationsAsync(int userId, int limit);
}

public class AgentActionRepository : IAgentActionRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public AgentActionRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<AgentAction?> GetByIdAsync(int id)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        
        const string sql = @"
            SELECT id as Id, conversation_id as ConversationId,
                   action_type as ActionType, status as Status,
                   input_prompt as InputPrompt, parameters as Parameters,
                   result as Result, error_message as ErrorMessage,
                   created_at as CreatedAt, completed_at as CompletedAt
            FROM agent_actions
            WHERE id = @Id";

        var action = await connection.QueryFirstOrDefaultAsync<AgentActionDto>(sql, new { Id = id });
        return action?.ToAgentAction();
    }

    public async Task<IEnumerable<AgentAction>> GetByConversationIdAsync(int conversationId)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        
        const string sql = @"
            SELECT id as Id, conversation_id as ConversationId,
                   action_type as ActionType, status as Status,
                   input_prompt as InputPrompt, parameters as Parameters,
                   result as Result, error_message as ErrorMessage,
                   created_at as CreatedAt, completed_at as CompletedAt
            FROM agent_actions
            WHERE conversation_id = @ConversationId
            ORDER BY created_at DESC";

        var actions = await connection.QueryAsync<AgentActionDto>(sql, new { ConversationId = conversationId });
        return actions.Select(a => a.ToAgentAction());
    }

    public async Task<AgentAction> CreateAsync(AgentAction action)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        
        const string sql = @"
            INSERT INTO agent_actions 
                (conversation_id, action_type, status, input_prompt, parameters, result, error_message, created_at, completed_at)
            VALUES 
                (@ConversationId, @ActionType, @Status, @InputPrompt, @Parameters::jsonb, @Result::jsonb, @ErrorMessage, @CreatedAt, @CompletedAt)
            RETURNING id as Id, conversation_id as ConversationId,
                      action_type as ActionType, status as Status,
                      input_prompt as InputPrompt, parameters as Parameters,
                      result as Result, error_message as ErrorMessage,
                      created_at as CreatedAt, completed_at as CompletedAt";

        var dto = AgentActionDto.FromAgentAction(action);
        var result = await connection.QuerySingleAsync<AgentActionDto>(sql, dto);
        return result.ToAgentAction();
    }

    public async Task UpdateAsync(AgentAction action)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        
        const string sql = @"
            UPDATE agent_actions
            SET status = @Status,
                result = @Result::jsonb,
                error_message = @ErrorMessage,
                completed_at = @CompletedAt
            WHERE id = @Id";

        var dto = AgentActionDto.FromAgentAction(action);
        await connection.ExecuteAsync(sql, dto);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        
        const string sql = "DELETE FROM agent_actions WHERE id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public async Task<IEnumerable<AgentAction>> GetRecentPlaylistCreationsAsync(int userId, int limit)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        
        const string sql = @"
            SELECT aa.id as Id, aa.conversation_id as ConversationId,
                   aa.action_type as ActionType, aa.status as Status,
                   aa.input_prompt as InputPrompt, aa.parameters as Parameters,
                   aa.result as Result, aa.error_message as ErrorMessage,
                   aa.created_at as CreatedAt, aa.completed_at as CompletedAt
            FROM agent_actions aa
            INNER JOIN conversations c ON aa.conversation_id = c.id
            WHERE c.user_id = @UserId
                AND aa.action_type IN ('CreateSmartPlaylist', 'DiscoverNewMusic')
                AND aa.status = 'Completed'
                AND aa.result IS NOT NULL
            ORDER BY aa.created_at DESC
            LIMIT @Limit";

        var actions = await connection.QueryAsync<AgentActionDto>(sql, new { UserId = userId, Limit = limit });
        return actions.Select(a => a.ToAgentAction());
    }
}

internal class AgentActionDto
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public required string ActionType { get; set; }
    public required string Status { get; set; }
    public string? InputPrompt { get; set; }
    public string? Parameters { get; set; }
    public string? Result { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public AgentAction ToAgentAction()
    {
        return new AgentAction
        {
            Id = Id,
            ConversationId = ConversationId,
            ActionType = ActionType,
            Status = Status,
            InputPrompt = InputPrompt,
            Parameters = Parameters != null ? JsonDocument.Parse(Parameters) : null,
            Result = Result != null ? JsonDocument.Parse(Result) : null,
            ErrorMessage = ErrorMessage,
            CreatedAt = CreatedAt,
            CompletedAt = CompletedAt
        };
    }

    public static AgentActionDto FromAgentAction(AgentAction action)
    {
        return new AgentActionDto
        {
            Id = action.Id,
            ConversationId = action.ConversationId,
            ActionType = action.ActionType,
            Status = action.Status,
            InputPrompt = action.InputPrompt,
            Parameters = action.Parameters?.RootElement.GetRawText(),
            Result = action.Result?.RootElement.GetRawText(),
            ErrorMessage = action.ErrorMessage,
            CreatedAt = action.CreatedAt,
            CompletedAt = action.CompletedAt
        };
    }
}
