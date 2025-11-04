using System.Text.Json;
using API.DTOs.Agent;
using API.DTOs.Spotify;
using API.Models;
using API.Repositories;
using API.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TUnit.Core;

namespace API.UnitTests.Services;

public class AgentServiceTests
{
    private readonly Mock<IAgentActionRepository> _mockActionRepository;
    private readonly Mock<ISpotifyService> _mockSpotifyService;
    private readonly Mock<IAIService> _mockAIService;
    private readonly Mock<ILogger<AgentService>> _mockLogger;
    private readonly AgentService _agentService;

    public AgentServiceTests()
    {
        _mockActionRepository = new Mock<IAgentActionRepository>();
        _mockSpotifyService = new Mock<ISpotifyService>();
        _mockAIService = new Mock<IAIService>();
        _mockLogger = new Mock<ILogger<AgentService>>();
        _agentService = new AgentService(
            _mockActionRepository.Object,
            _mockSpotifyService.Object,
            _mockAIService.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task CreateSmartPlaylistAsync_WithValidUser_CreatesPlaylist()
    {
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            SpotifyAccessToken = "valid-token",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var request = new CreateSmartPlaylistRequest(
            Prompt: "Create a workout playlist",
            Preferences: null
        );

        var tracks = new[]
        {
            new SpotifyTrack(
                "track1",
                "Track 1",
                "spotify:track:1",
                new[] { new SpotifyArtist("artist1", "Artist 1", "spotify:artist:1") },
                new SpotifyAlbum("album1", "Album 1", "spotify:album:1", Array.Empty<SpotifyImage>()),
                180000,
                80
            )
        };

        var playlist = new SpotifyPlaylist(
            "playlist1",
            "AI Workout Mix",
            "AI-generated playlist: Create a workout playlist",
            "spotify:playlist:1",
            1
        );

        _mockActionRepository
            .Setup(r => r.CreateAsync(It.IsAny<AgentAction>()))
            .ReturnsAsync((AgentAction action) =>
            {
                action.Id = 1;
                return action;
            });

        _mockSpotifyService
            .Setup(s => s.SearchTracksAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(tracks);

        _mockSpotifyService
            .Setup(s => s.GetCurrentUserIdAsync(It.IsAny<string>()))
            .ReturnsAsync("spotify-user-123");

        _mockSpotifyService
            .Setup(s => s.CreatePlaylistAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CreatePlaylistRequest>()))
            .ReturnsAsync(playlist);

        _mockSpotifyService
            .Setup(s => s.AddTracksToPlaylistAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns(Task.CompletedTask);

        _mockActionRepository
            .Setup(r => r.UpdateAsync(It.IsAny<AgentAction>()))
            .Returns(Task.CompletedTask);

        var result = await _agentService.CreateSmartPlaylistAsync(user, request, 1);

        result.Should().NotBeNull();
        result.Status.Should().Be("Completed");
        result.ActionId.Should().Be(1);
        result.Result.Should().NotBeNull();

        _mockActionRepository.Verify(r => r.CreateAsync(It.Is<AgentAction>(a =>
            a.ActionType == "CreateSmartPlaylist" &&
            a.Status == "Processing"
        )), Times.Once);

        _mockActionRepository.Verify(r => r.UpdateAsync(It.Is<AgentAction>(a =>
            a.Status == "Completed" &&
            a.Result != null
        )), Times.Once);
    }

    [Test]
    public async Task CreateSmartPlaylistAsync_WithoutSpotifyToken_ReturnsFailure()
    {
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            SpotifyAccessToken = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var request = new CreateSmartPlaylistRequest(
            Prompt: "Create a workout playlist",
            Preferences: null
        );

        _mockActionRepository
            .Setup(r => r.CreateAsync(It.IsAny<AgentAction>()))
            .ReturnsAsync((AgentAction action) =>
            {
                action.Id = 1;
                return action;
            });

        _mockActionRepository
            .Setup(r => r.UpdateAsync(It.IsAny<AgentAction>()))
            .Returns(Task.CompletedTask);

        var result = await _agentService.CreateSmartPlaylistAsync(user, request, 1);

        result.Should().NotBeNull();
        result.Status.Should().Be("Failed");
        result.ErrorMessage.Should().Contain("Spotify account");

        _mockActionRepository.Verify(r => r.UpdateAsync(It.Is<AgentAction>(a =>
            a.Status == "Failed" &&
            a.ErrorMessage != null
        )), Times.Once);
    }

    [Test]
    public async Task DiscoverNewMusicAsync_WithValidUser_CreatesDiscoveryPlaylist()
    {
        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            SpotifyAccessToken = "valid-token",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var request = new DiscoverNewMusicRequest(
            Limit: 10,
            SourcePlaylistIds: null
        );

        var savedTracks = new[]
        {
            new SpotifyTrack(
                "saved1",
                "Saved Track 1",
                "spotify:track:saved1",
                new[] { new SpotifyArtist("artist1", "Artist 1", "spotify:artist:1") },
                new SpotifyAlbum("album1", "Album 1", "spotify:album:1", Array.Empty<SpotifyImage>()),
                180000,
                80
            )
        };

        var recommendations = new[]
        {
            new SpotifyTrack(
                "new1",
                "New Track 1",
                "spotify:track:new1",
                new[] { new SpotifyArtist("artist2", "Artist 2", "spotify:artist:2") },
                new SpotifyAlbum("album2", "Album 2", "spotify:album:2", Array.Empty<SpotifyImage>()),
                200000,
                75
            )
        };

        var playlist = new SpotifyPlaylist(
            "playlist1",
            $"Discover Weekly - {DateTime.UtcNow:MMM dd, yyyy}",
            "AI-generated music discovery based on your listening habits",
            "spotify:playlist:1",
            1
        );

        _mockActionRepository
            .Setup(r => r.CreateAsync(It.IsAny<AgentAction>()))
            .ReturnsAsync((AgentAction action) =>
            {
                action.Id = 1;
                return action;
            });

        _mockSpotifyService
            .Setup(s => s.GetUserSavedTracksAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(savedTracks);

        _mockSpotifyService
            .Setup(s => s.GetRecommendationsAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<int>()))
            .ReturnsAsync(recommendations);

        _mockSpotifyService
            .Setup(s => s.GetCurrentUserIdAsync(It.IsAny<string>()))
            .ReturnsAsync("spotify-user-123");

        _mockSpotifyService
            .Setup(s => s.CreatePlaylistAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CreatePlaylistRequest>()))
            .ReturnsAsync(playlist);

        _mockSpotifyService
            .Setup(s => s.AddTracksToPlaylistAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string[]>()))
            .Returns(Task.CompletedTask);

        _mockActionRepository
            .Setup(r => r.UpdateAsync(It.IsAny<AgentAction>()))
            .Returns(Task.CompletedTask);

        var result = await _agentService.DiscoverNewMusicAsync(user, request, 1);

        result.Should().NotBeNull();
        result.Status.Should().Be("Completed");
        result.ActionId.Should().Be(1);

        _mockActionRepository.Verify(r => r.CreateAsync(It.Is<AgentAction>(a =>
            a.ActionType == "DiscoverNewMusic"
        )), Times.Once);
    }
}
