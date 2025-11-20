using System.Net;
using System.Net.Http.Json;
using API.DTOs.Agent;
using API.IntegrationTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TUnit.Core;

namespace API.IntegrationTests.Controllers;

public class SpotifyControllerTests : IAsyncDisposable {
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SpotifyControllerTests() {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [Test]
    public async Task GetStatus_WithoutAuth_ReturnsUnauthorized() {
        var response = await _client.GetAsync("/api/spotify/status");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetPlaylists_WithoutAuth_ReturnsUnauthorized() {
        var response = await _client.GetAsync("/api/spotify/playlists");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetProfile_WithoutAuth_ReturnsUnauthorized() {
        var response = await _client.GetAsync("/api/spotify/profile");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Disconnect_WithoutAuth_ReturnsUnauthorized() {
        var response = await _client.PostAsync("/api/spotify/disconnect", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ExchangeCode_WithoutAuth_ReturnsUnauthorized() {
        var request = new { Code = "test-code", RedirectUri = "http://localhost" };
        var response = await _client.PostAsJsonAsync("/api/spotify/exchange-code", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public async ValueTask DisposeAsync() {
        _client.Dispose();
        await _factory.DisposeAsync();
    }
}