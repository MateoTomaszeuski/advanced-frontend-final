using System.Net;
using System.Net.Http.Json;
using API.Data;
using API.Models;
using API.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TUnit.Core;

namespace API.IntegrationTests.Controllers;

public class ConversationsControllerTests {
    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;
    private string? _connectionString;

    [Before(Test)]
    public async Task Setup() {
        _connectionString = "Host=localhost;Port=5433;Database=testdb;Username=testuser;Password=testpass";

        await InitializeDatabase();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => {
                builder.ConfigureServices(services => {
                    var dbFactoryDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IDbConnectionFactory));

                    if (dbFactoryDescriptor != null)
                        services.Remove(dbFactoryDescriptor);

                    services.AddSingleton<IDbConnectionFactory>(
                        new DbConnectionFactory(_connectionString));
                });
            });

        _client = _factory.CreateClient();
    }

    private async Task InitializeDatabase() {
        await using var connection = new Npgsql.NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var cleanupSql = @"
            TRUNCATE TABLE agent_actions, conversations, users RESTART IDENTITY CASCADE;
        ";

        await using var command = new Npgsql.NpgsqlCommand(cleanupSql, connection);
        await command.ExecuteNonQueryAsync();
    }

    [Test]
    public async Task GetConversations_WithoutAuth_ReturnsUnauthorized() {
        var response = await _client!.GetAsync("/api/conversations");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task CreateConversation_WithAuth_CreatesConversation() {
        var testUser = await CreateTestUser("test@example.com");
        _client!.DefaultRequestHeaders.Add("X-Test-User-Id", testUser.Id.ToString());

        var request = new { Title = "Test Conversation" };
        var response = await _client.PostAsJsonAsync("/api/conversations", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var conversation = await response.Content.ReadFromJsonAsync<ConversationResponse>();
        conversation.Should().NotBeNull();
        conversation!.Title.Should().Be("Test Conversation");
    }

    private async Task<User> CreateTestUser(string email) {
        var factory = new DbConnectionFactory(_connectionString!);
        var repository = new UserRepository(factory);

        return await repository.CreateAsync(new User {
            Email = email,
            DisplayName = "Test User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
    }

    [After(Test)]
    public void Cleanup() {
        _client?.Dispose();
        _factory?.Dispose();
    }

    private record ConversationResponse(int Id, string Title, string CreatedAt, string UpdatedAt);
}