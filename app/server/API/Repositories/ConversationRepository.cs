using API.Data;
using API.Models;
using Dapper;

namespace API.Repositories;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(int id);
    Task<IEnumerable<Conversation>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Conversation>> GetAllByUserIdAsync(int userId);
    Task<Conversation> CreateAsync(Conversation conversation);
    Task UpdateAsync(Conversation conversation);
    Task DeleteAsync(int id);
}

public class ConversationRepository : IConversationRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public ConversationRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Conversation?> GetByIdAsync(int id)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        
        const string sql = @"
            SELECT id as Id, user_id as UserId, title as Title,
                   created_at as CreatedAt, updated_at as UpdatedAt
            FROM conversations
            WHERE id = @Id";

        return await connection.QueryFirstOrDefaultAsync<Conversation>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Conversation>> GetByUserIdAsync(int userId)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        
        const string sql = @"
            SELECT c.id as Id, c.user_id as UserId, c.title as Title,
                   c.created_at as CreatedAt, c.updated_at as UpdatedAt,
                   COUNT(a.id) as ActionCount
            FROM conversations c
            LEFT JOIN agent_actions a ON c.id = a.conversation_id
            WHERE c.user_id = @UserId
            GROUP BY c.id, c.user_id, c.title, c.created_at, c.updated_at
            ORDER BY c.updated_at DESC";

        return await connection.QueryAsync<Conversation>(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<Conversation>> GetAllByUserIdAsync(int userId)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        
        const string sql = @"
            SELECT id as Id, user_id as UserId, title as Title,
                   created_at as CreatedAt, updated_at as UpdatedAt
            FROM conversations
            WHERE user_id = @UserId
            ORDER BY created_at DESC";

        return await connection.QueryAsync<Conversation>(sql, new { UserId = userId });
    }

    public async Task<Conversation> CreateAsync(Conversation conversation)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        
        const string sql = @"
            INSERT INTO conversations (user_id, title, created_at, updated_at)
            VALUES (@UserId, @Title, @CreatedAt, @UpdatedAt)
            RETURNING id as Id, user_id as UserId, title as Title,
                      created_at as CreatedAt, updated_at as UpdatedAt";

        return await connection.QuerySingleAsync<Conversation>(sql, conversation);
    }

    public async Task UpdateAsync(Conversation conversation)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        
        const string sql = @"
            UPDATE conversations
            SET title = @Title,
                updated_at = @UpdatedAt
            WHERE id = @Id";

        await connection.ExecuteAsync(sql, conversation);
    }

    public async Task DeleteAsync(int id)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        
        const string sql = "DELETE FROM conversations WHERE id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }
}
