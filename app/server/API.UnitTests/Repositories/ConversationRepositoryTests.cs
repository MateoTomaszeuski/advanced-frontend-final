using API.Data;
using API.Models;
using API.Repositories;
using FluentAssertions;
using TUnit.Core;
using Testcontainers.PostgreSql;

namespace API.UnitTests.Repositories;

public class ConversationRepositoryTests : IAsyncDisposable {
    private readonly PostgreSqlContainer _postgresContainer;
    private IDbConnectionFactory? _dbFactory;
    private ConversationRepository? _repository;
    private UserRepository? _userRepository;
    private int _testUserId;

    public ConversationRepositoryTests() {
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
        _repository = new ConversationRepository(_dbFactory);
        _userRepository = new UserRepository(_dbFactory);

        var user = await _userRepository.CreateAsync(new User {
            Email = "test@example.com",
            DisplayName = "Test User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        _testUserId = user.Id;
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
            );

            CREATE TABLE IF NOT EXISTS conversations (
                id SERIAL PRIMARY KEY,
                user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
                title VARCHAR(500) NOT NULL,
                created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS agent_actions (
                id SERIAL PRIMARY KEY,
                conversation_id INTEGER NOT NULL REFERENCES conversations(id) ON DELETE CASCADE,
                action_type VARCHAR(100) NOT NULL,
                status VARCHAR(50) NOT NULL,
                input_prompt TEXT,
                parameters JSONB,
                result JSONB,
                error_message TEXT,
                created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                completed_at TIMESTAMP
            );";

        await using var connection = new Npgsql.NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new Npgsql.NpgsqlCommand(initSql, connection);
        await command.ExecuteNonQueryAsync();
    }

    [Test]
    public async Task CreateAsync_CreatesConversationSuccessfully() {
        var conversation = new Conversation {
            UserId = _testUserId,
            Title = "Test Conversation",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _repository!.CreateAsync(conversation);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Title.Should().Be("Test Conversation");
        result.UserId.Should().Be(_testUserId);
    }

    [Test]
    public async Task GetByUserIdAsync_ReturnsUserConversations() {
        await _repository!.CreateAsync(new Conversation {
            UserId = _testUserId,
            Title = "Conversation 1",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await _repository.CreateAsync(new Conversation {
            UserId = _testUserId,
            Title = "Conversation 2",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var conversations = await _repository.GetByUserIdAsync(_testUserId);

        conversations.Should().HaveCount(2);
        conversations.Should().OnlyContain(c => c.UserId == _testUserId);
    }

    [Test]
    public async Task UpdateAsync_UpdatesConversationSuccessfully() {
        var conversation = await _repository!.CreateAsync(new Conversation {
            UserId = _testUserId,
            Title = "Original Title",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        conversation.Title = "Updated Title";
        conversation.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(conversation);

        var updated = await _repository.GetByIdAsync(conversation.Id);
        updated.Should().NotBeNull();
        updated!.Title.Should().Be("Updated Title");
    }

    [Test]
    public async Task DeleteAsync_DeletesConversationSuccessfully() {
        var conversation = await _repository!.CreateAsync(new Conversation {
            UserId = _testUserId,
            Title = "To Delete",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await _repository.DeleteAsync(conversation.Id);

        var deleted = await _repository.GetByIdAsync(conversation.Id);
        deleted.Should().BeNull();
    }

    public async ValueTask DisposeAsync() {
        await _postgresContainer.DisposeAsync();
    }
}