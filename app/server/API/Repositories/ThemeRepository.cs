using API.Data;
using API.Models;
using Dapper;

namespace API.Repositories;

public interface IThemeRepository
{
    Task<UserTheme?> GetByUserIdAsync(int userId);
    Task<UserTheme> CreateAsync(UserTheme theme);
    Task UpdateAsync(UserTheme theme);
    Task DeleteAsync(int userId);
}

public class ThemeRepository : IThemeRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public ThemeRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<UserTheme?> GetByUserIdAsync(int userId)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        
        const string sql = @"
            SELECT id as Id, user_id as UserId, theme_data as ThemeData,
                   description as Description, created_at as CreatedAt, 
                   updated_at as UpdatedAt
            FROM user_themes
            WHERE user_id = @UserId";

        return await connection.QueryFirstOrDefaultAsync<UserTheme>(sql, new { UserId = userId });
    }

    public async Task<UserTheme> CreateAsync(UserTheme theme)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        
        const string sql = @"
            INSERT INTO user_themes (user_id, theme_data, description, created_at, updated_at)
            VALUES (@UserId, @ThemeData::jsonb, @Description, @CreatedAt, @UpdatedAt)
            RETURNING id as Id, user_id as UserId, theme_data as ThemeData,
                      description as Description, created_at as CreatedAt, 
                      updated_at as UpdatedAt";

        return await connection.QuerySingleAsync<UserTheme>(sql, theme);
    }

    public async Task UpdateAsync(UserTheme theme)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        
        const string sql = @"
            UPDATE user_themes
            SET theme_data = @ThemeData::jsonb,
                description = @Description,
                updated_at = @UpdatedAt
            WHERE user_id = @UserId";

        await connection.ExecuteAsync(sql, theme);
    }

    public async Task DeleteAsync(int userId)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        
        const string sql = "DELETE FROM user_themes WHERE user_id = @UserId";
        await connection.ExecuteAsync(sql, new { UserId = userId });
    }
}
