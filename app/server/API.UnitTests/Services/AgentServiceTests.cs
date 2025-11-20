using API.DTOs.Agent;
using API.Interfaces;
using API.Models;
using API.Services;
using FluentAssertions;
using Moq;
using TUnit.Core;

namespace API.UnitTests.Services;

public class AgentServiceTests {
    private readonly Mock<API.Interfaces.IDuplicateCleanerService> _mockDuplicateCleanerService;
    private readonly Mock<API.Interfaces.IPlaylistCreatorService> _mockPlaylistCreatorService;
    private readonly Mock<API.Interfaces.IMusicDiscoveryService> _mockMusicDiscoveryService;
    private readonly Mock<API.Interfaces.IMusicSuggestionService> _mockMusicSuggestionService;
    private readonly AgentService _agentService;

    public AgentServiceTests() {
        _mockDuplicateCleanerService = new Mock<API.Interfaces.IDuplicateCleanerService>();
        _mockPlaylistCreatorService = new Mock<API.Interfaces.IPlaylistCreatorService>();
        _mockMusicDiscoveryService = new Mock<API.Interfaces.IMusicDiscoveryService>();
        _mockMusicSuggestionService = new Mock<API.Interfaces.IMusicSuggestionService>();

        _agentService = new AgentService(
            _mockDuplicateCleanerService.Object,
            _mockPlaylistCreatorService.Object,
            _mockMusicDiscoveryService.Object,
            _mockMusicSuggestionService.Object
        );
    }

    [Test]
    public async Task CreateSmartPlaylistAsync_DelegatesToPlaylistCreatorService() {
        var user = new User {
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

        var conversationId = 1;

        var expectedResponse = new AgentActionResponse(
            ActionId: 1,
            ActionType: "CreateSmartPlaylist",
            Status: "Completed",
            Result: new { playlistId = "123", trackCount = 20 }
        );

        _mockPlaylistCreatorService
            .Setup(s => s.CreateSmartPlaylistAsync(user, request, conversationId))
            .ReturnsAsync(expectedResponse);

        var result = await _agentService.CreateSmartPlaylistAsync(user, request, conversationId);

        result.Should().Be(expectedResponse);
        _mockPlaylistCreatorService.Verify(s => s.CreateSmartPlaylistAsync(user, request, conversationId), Times.Once);
    }

    [Test]
    public async Task DiscoverNewMusicAsync_DelegatesToMusicDiscoveryService() {
        var user = new User {
            Id = 1,
            Email = "test@example.com",
            SpotifyAccessToken = "valid-token",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var request = new DiscoverNewMusicRequest(Limit: 20);
        var conversationId = 1;

        var expectedResponse = new AgentActionResponse(
            ActionId: 1,
            ActionType: "DiscoverNewMusic",
            Status: "Completed",
            Result: new { playlistId = "456", trackCount = 20 }
        );

        _mockMusicDiscoveryService
            .Setup(s => s.DiscoverNewMusicAsync(user, request, conversationId))
            .ReturnsAsync(expectedResponse);

        var result = await _agentService.DiscoverNewMusicAsync(user, request, conversationId);

        result.Should().Be(expectedResponse);
        _mockMusicDiscoveryService.Verify(s => s.DiscoverNewMusicAsync(user, request, conversationId), Times.Once);
    }

    [Test]
    public async Task ScanForDuplicatesAsync_DelegatesToDuplicateCleanerService() {
        var user = new User {
            Id = 1,
            Email = "test@example.com",
            SpotifyAccessToken = "valid-token",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var playlistId = "playlist123";
        var conversationId = 1;

        var expectedResponse = new RemoveDuplicatesResponse(
            PlaylistId: playlistId,
            PlaylistName: "Test Playlist",
            TotalDuplicateGroups: 0,
            TotalDuplicateTracks: 0,
            DuplicateGroups: Array.Empty<DuplicateGroup>()
        );

        _mockDuplicateCleanerService
            .Setup(s => s.ScanForDuplicatesAsync(user, playlistId, conversationId))
            .ReturnsAsync(expectedResponse);

        var result = await _agentService.ScanForDuplicatesAsync(user, playlistId, conversationId);

        result.Should().Be(expectedResponse);
        _mockDuplicateCleanerService.Verify(s => s.ScanForDuplicatesAsync(user, playlistId, conversationId), Times.Once);
    }

    [Test]
    public async Task ConfirmRemoveDuplicatesAsync_DelegatesToDuplicateCleanerService() {
        var user = new User {
            Id = 1,
            Email = "test@example.com",
            SpotifyAccessToken = "valid-token",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var request = new ConfirmRemoveDuplicatesRequest(
            PlaylistId: "playlist123",
            TrackUrisToRemove: Array.Empty<string>()
        );
        var conversationId = 1;

        var expectedResponse = new AgentActionResponse(
            ActionId: 1,
            ActionType: "RemoveDuplicates",
            Status: "Completed",
            Result: new { removedCount = 0 }
        );

        _mockDuplicateCleanerService
            .Setup(s => s.ConfirmRemoveDuplicatesAsync(user, request, conversationId))
            .ReturnsAsync(expectedResponse);

        var result = await _agentService.ConfirmRemoveDuplicatesAsync(user, request, conversationId);

        result.Should().Be(expectedResponse);
        _mockDuplicateCleanerService.Verify(s => s.ConfirmRemoveDuplicatesAsync(user, request, conversationId), Times.Once);
    }

    [Test]
    public async Task SuggestMusicByContextAsync_DelegatesToMusicSuggestionService() {
        var user = new User {
            Id = 1,
            Email = "test@example.com",
            SpotifyAccessToken = "valid-token",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var request = new SuggestMusicRequest(
            PlaylistId: "playlist123",
            Context: "workout",
            Limit: 10
        );
        var conversationId = 1;

        var expectedResponse = new SuggestMusicResponse(
            PlaylistId: "playlist123",
            PlaylistName: "Test Playlist",
            Context: "workout",
            SuggestionCount: 10,
            Suggestions: Array.Empty<SuggestedTrack>()
        );

        _mockMusicSuggestionService
            .Setup(s => s.SuggestMusicByContextAsync(user, request, conversationId))
            .ReturnsAsync(expectedResponse);

        var result = await _agentService.SuggestMusicByContextAsync(user, request, conversationId);

        result.Should().Be(expectedResponse);
        _mockMusicSuggestionService.Verify(s => s.SuggestMusicByContextAsync(user, request, conversationId), Times.Once);
    }
}