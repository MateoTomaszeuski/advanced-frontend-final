using API.Data;
using API.Models;
using API.Repositories;
using FluentAssertions;
using TUnit.Core;
using Testcontainers.PostgreSql;

namespace API.UnitTests.Repositories;

public class UserRepositoryTests : IAsyncDisposable {
    private readonly PostgreSqlContainer _postgresContainer;
    private IDbConnectionFactory? _dbFactory;
    private UserRepository? _repository;

    public UserRepositoryTests() {
        _postgresContainer = new PostgreSqlBuilder()
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .Build();
    }

    [Before(Test)]
    public async Task Setup() {
        await _postgresContainer.StartAsync();
        var connectionString = _postgresContainer.GetConnectionString();

        await InitializeDatabase(connectionString);

        _dbFactory = new DbConnectionFactory(connectionString);
        _repository = new UserRepository(_dbFactory);
    }

    private async Task InitializeDatabase(string connectionString) {
        var initSql = @"
            CREATE TABLE IF NOT EXISTS users (
                id SERIAL PRIMARY KEY,
                email VARCHAR(255) NOT NULL UNIQUE,
                display_name VARCHAR(255),
                spotify_access_token TEXT,
                spotify_refresh_token TEXT,
                spotify_token_expiry TIMESTAMP,
                created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
            );";

        await using var connection = new Npgsql.NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new Npgsql.NpgsqlCommand(initSql, connection);
        await command.ExecuteNonQueryAsync();
    }

    [Test]
    public async Task CreateAsync_CreatesUserSuccessfully() {
        var user = new User {
            Email = "test@example.com",
            DisplayName = "Test User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _repository!.CreateAsync(user);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Email.Should().Be("test@example.com");
        result.DisplayName.Should().Be("Test User");
    }

    [Test]
    public async Task GetByEmailAsync_ReturnsExistingUser() {
        var user = new User {
            Email = "existing@example.com",
            DisplayName = "Existing User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository!.CreateAsync(user);

        var result = await _repository.GetByEmailAsync("existing@example.com");

        result.Should().NotBeNull();
        result!.Email.Should().Be("existing@example.com");
        result.DisplayName.Should().Be("Existing User");
    }

    [Test]
    public async Task GetByEmailAsync_ReturnsNullForNonExistentUser() {
        var result = await _repository!.GetByEmailAsync("nonexistent@example.com");

        result.Should().BeNull();
    }

    [Test]
    public async Task UpdateAsync_UpdatesUserSuccessfully() {
        var user = new User {
            Email = "update@example.com",
            DisplayName = "Original Name",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository!.CreateAsync(user);
        created.DisplayName = "Updated Name";
        created.SpotifyAccessToken = "new-token";
        created.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(created);

        var updated = await _repository.GetByIdAsync(created.Id);
        updated.Should().NotBeNull();
        updated!.DisplayName.Should().Be("Updated Name");
        updated.SpotifyAccessToken.Should().Be("new-token");
    }

    [Test]
    public async Task DeleteAsync_DeletesUserSuccessfully() {
        var user = new User {
            Email = "delete@example.com",
            DisplayName = "Delete User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _repository!.CreateAsync(user);
        await _repository.DeleteAsync(created.Id);

        var deleted = await _repository.GetByIdAsync(created.Id);
        deleted.Should().BeNull();
    }

    public async ValueTask DisposeAsync() {
        await _postgresContainer.DisposeAsync();
    }
}