using API.Repositories;
using API.Models;
using FluentAssertions;
using TUnit.Core;
using Testcontainers.PostgreSql;
using API.Data;
using Dapper;

namespace API.UnitTests.Repositories;

public class AgentActionRepositoryTests : IAsyncDisposable
{
    private readonly PostgreSqlContainer _postgresContainer;
    private IDbConnectionFactory _connectionFactory = null!;
    private AgentActionRepository _repository = null!;
    private ConversationRepository _conversationRepository = null!;
    private UserRepository _userRepository = null!;

    public AgentActionRepositoryTests()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithDatabase("testdb")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    [Before(Test)]
    public async Task Setup()
    {
        await _postgresContainer.StartAsync();

        _connectionFactory = new DbConnectionFactory(_postgresContainer.GetConnectionString());
        _repository = new AgentActionRepository(_connectionFactory);
        _conversationRepository = new ConversationRepository(_connectionFactory);
        _userRepository = new UserRepository(_connectionFactory);

        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        await connection.ExecuteAsync(@"
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
            );
        ");
    }

    [Test]
    public async Task CreateAsync_CreatesAgentAction()
    {
        var user = await _userRepository.CreateAsync(new User
        {
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var conversation = await _conversationRepository.CreateAsync(new Conversation
        {
            UserId = user.Id,
            Title = "Test Conversation",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var action = new AgentAction
        {
            ConversationId = conversation.Id,
            ActionType = "CreateSmartPlaylist",
            Status = "Processing",
            InputPrompt = "Create a playlist",
            CreatedAt = DateTime.UtcNow
        };

        var result = await _repository.CreateAsync(action);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.ConversationId.Should().Be(conversation.Id);
        result.ActionType.Should().Be("CreateSmartPlaylist");
        result.Status.Should().Be("Processing");
    }

    [Test]
    public async Task GetByIdAsync_ReturnsAction()
    {
        var user = await _userRepository.CreateAsync(new User
        {
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var conversation = await _conversationRepository.CreateAsync(new Conversation
        {
            UserId = user.Id,
            Title = "Test",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var created = await _repository.CreateAsync(new AgentAction
        {
            ConversationId = conversation.Id,
            ActionType = "Test",
            Status = "Completed",
            CreatedAt = DateTime.UtcNow
        });

        var result = await _repository.GetByIdAsync(created.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
    }

    [Test]
    public async Task UpdateAsync_UpdatesAction()
    {
        var user = await _userRepository.CreateAsync(new User
        {
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var conversation = await _conversationRepository.CreateAsync(new Conversation
        {
            UserId = user.Id,
            Title = "Test",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var action = await _repository.CreateAsync(new AgentAction
        {
            ConversationId = conversation.Id,
            ActionType = "Test",
            Status = "Processing",
            CreatedAt = DateTime.UtcNow
        });

        action.Status = "Completed";
        action.CompletedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(action);

        var updated = await _repository.GetByIdAsync(action.Id);
        updated!.Status.Should().Be("Completed");
        updated.CompletedAt.Should().NotBeNull();
    }

    [Test]
    public async Task GetByConversationIdAsync_ReturnsActionsForConversation()
    {
        var user = await _userRepository.CreateAsync(new User
        {
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var conversation = await _conversationRepository.CreateAsync(new Conversation
        {
            UserId = user.Id,
            Title = "Test",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await _repository.CreateAsync(new AgentAction
        {
            ConversationId = conversation.Id,
            ActionType = "Action1",
            Status = "Completed",
            CreatedAt = DateTime.UtcNow
        });

        await _repository.CreateAsync(new AgentAction
        {
            ConversationId = conversation.Id,
            ActionType = "Action2",
            Status = "Processing",
            CreatedAt = DateTime.UtcNow
        });

        var results = await _repository.GetByConversationIdAsync(conversation.Id);
        var actionsList = results.ToList();

        actionsList.Should().HaveCount(2);
        actionsList.Should().AllSatisfy(a => a.ConversationId.Should().Be(conversation.Id));
    }

    [Test]
    public async Task DeleteAsync_DeletesAction()
    {
        var user = await _userRepository.CreateAsync(new User
        {
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var conversation = await _conversationRepository.CreateAsync(new Conversation
        {
            UserId = user.Id,
            Title = "Test",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var action = await _repository.CreateAsync(new AgentAction
        {
            ConversationId = conversation.Id,
            ActionType = "Test",
            Status = "Completed",
            CreatedAt = DateTime.UtcNow
        });

        await _repository.DeleteAsync(action.Id);

        var result = await _repository.GetByIdAsync(action.Id);
        result.Should().BeNull();
    }

    [Test]
    public async Task GetRecentPlaylistCreationsAsync_ReturnsOnlyPlaylistActions()
    {
        var user = await _userRepository.CreateAsync(new User
        {
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var conversation = await _conversationRepository.CreateAsync(new Conversation
        {
            UserId = user.Id,
            Title = "Test",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var playlist1 = await _repository.CreateAsync(new AgentAction
        {
            ConversationId = conversation.Id,
            ActionType = "CreateSmartPlaylist",
            Status = "Completed",
            Result = System.Text.Json.JsonDocument.Parse("{\"playlistId\":\"123\"}"),
            CreatedAt = DateTime.UtcNow
        });

        await _repository.CreateAsync(new AgentAction
        {
            ConversationId = conversation.Id,
            ActionType = "ScanDuplicates",
            Status = "Completed",
            CreatedAt = DateTime.UtcNow
        });

        var results = await _repository.GetRecentPlaylistCreationsAsync(user.Id, 10);
        var resultsList = results.ToList();

        resultsList.Should().HaveCount(1);
        resultsList[0].ActionType.Should().Be("CreateSmartPlaylist");
    }

    public async ValueTask DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }
}
