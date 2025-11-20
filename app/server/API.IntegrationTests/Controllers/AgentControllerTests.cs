using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TUnit.Core;

namespace API.IntegrationTests.Controllers;

public class AgentControllerTests {
    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;
    private string? _authToken;

    [Before(Test)]
    public async Task Setup() {
        var connectionString = "Host=localhost;Port=5433;Database=testdb;Username=testuser;Password=testpass";

        await InitializeDatabase(connectionString);

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => {
                builder.ConfigureServices(services => {
                    Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", connectionString);
                });
            });

        _client = _factory.CreateClient();

        _authToken = GenerateMockJwtToken("test@example.com", "Test User");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
    }

    private async Task InitializeDatabase(string connectionString) {
        await using var connection = new Npgsql.NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var cleanupSql = @"
            TRUNCATE TABLE agent_actions, conversations, users RESTART IDENTITY CASCADE;
        ";

        await using var command = new Npgsql.NpgsqlCommand(cleanupSql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private string GenerateMockJwtToken(string email, string displayName) {
        var header = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));
        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{{\"email\":\"{email}\",\"name\":\"{displayName}\"}}"));
        var signature = Convert.ToBase64String(Encoding.UTF8.GetBytes("mock-signature"));
        return $"{header}.{payload}.{signature}";
    }

    [Test]
    public async Task CreateSmartPlaylist_WithoutAuth_ReturnsUnauthorized() {
        _client!.DefaultRequestHeaders.Authorization = null;

        var request = new {
            ConversationId = 1,
            Prompt = "Upbeat workout music"
        };

        var response = await _client.PostAsJsonAsync("/api/agent/create-smart-playlist", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task DiscoverNewMusic_WithoutAuth_ReturnsUnauthorized() {
        _client!.DefaultRequestHeaders.Authorization = null;

        var request = new {
            ConversationId = 1,
            Limit = 10
        };

        var response = await _client.PostAsJsonAsync("/api/agent/discover-new-music", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [After(Test)]
    public void Cleanup() {
        _client?.Dispose();
        _factory?.Dispose();
    }
}