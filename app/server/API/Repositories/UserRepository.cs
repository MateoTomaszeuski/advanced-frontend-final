using API.Data;
using API.Models;
using Dapper;

namespace API.Repositories;

public interface IUserRepository {
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
}

public class UserRepository : IUserRepository {
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public UserRepository(IDbConnectionFactory dbConnectionFactory) {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<User?> GetByIdAsync(int id) {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        const string sql = @"
            SELECT id as Id, email as Email, display_name as DisplayName,
                   spotify_access_token as SpotifyAccessToken, 
                   spotify_refresh_token as SpotifyRefreshToken,
                   spotify_token_expiry as SpotifyTokenExpiry,
                   created_at as CreatedAt, updated_at as UpdatedAt
            FROM users
            WHERE id = @Id";

        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<User?> GetByEmailAsync(string email) {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        const string sql = @"
            SELECT id as Id, email as Email, display_name as DisplayName,
                   spotify_access_token as SpotifyAccessToken, 
                   spotify_refresh_token as SpotifyRefreshToken,
                   spotify_token_expiry as SpotifyTokenExpiry,
                   created_at as CreatedAt, updated_at as UpdatedAt
            FROM users
            WHERE email = @Email";

        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<User> CreateAsync(User user) {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        const string sql = @"
            INSERT INTO users (email, display_name, created_at, updated_at)
            VALUES (@Email, @DisplayName, @CreatedAt, @UpdatedAt)
            ON CONFLICT (email) DO UPDATE
            SET display_name = EXCLUDED.display_name,
                updated_at = EXCLUDED.updated_at
            RETURNING id as Id, email as Email, display_name as DisplayName,
                      spotify_access_token as SpotifyAccessToken, 
                      spotify_refresh_token as SpotifyRefreshToken,
                      spotify_token_expiry as SpotifyTokenExpiry,
                      created_at as CreatedAt, updated_at as UpdatedAt";

        return await connection.QuerySingleAsync<User>(sql, user);
    }

    public async Task UpdateAsync(User user) {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        const string sql = @"
            UPDATE users
            SET display_name = @DisplayName,
                spotify_access_token = @SpotifyAccessToken,
                spotify_refresh_token = @SpotifyRefreshToken,
                spotify_token_expiry = @SpotifyTokenExpiry,
                updated_at = @UpdatedAt
            WHERE id = @Id";

        await connection.ExecuteAsync(sql, user);
    }

    public async Task DeleteAsync(int id) {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        const string sql = "DELETE FROM users WHERE id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }
}