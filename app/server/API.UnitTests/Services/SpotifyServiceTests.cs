using API.Interfaces;
using API.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TUnit.Core;

namespace API.UnitTests.Services;

public class SpotifyServiceTests {
    private readonly Mock<ILogger<SpotifyService>> _mockLogger;
    private readonly Mock<API.Interfaces.ISpotifyAuthService> _mockAuthService;
    private readonly Mock<API.Interfaces.ISpotifyUserService> _mockUserService;
    private readonly Mock<API.Interfaces.ISpotifyPlaylistService> _mockPlaylistService;
    private readonly Mock<API.Interfaces.ISpotifyTrackService> _mockTrackService;
    private readonly SpotifyService _spotifyService;

    public SpotifyServiceTests() {
        _mockLogger = new Mock<ILogger<SpotifyService>>();
        _mockAuthService = new Mock<API.Interfaces.ISpotifyAuthService>();
        _mockUserService = new Mock<API.Interfaces.ISpotifyUserService>();
        _mockPlaylistService = new Mock<API.Interfaces.ISpotifyPlaylistService>();
        _mockTrackService = new Mock<API.Interfaces.ISpotifyTrackService>();

        _spotifyService = new SpotifyService(
            _mockAuthService.Object,
            _mockUserService.Object,
            _mockPlaylistService.Object,
            _mockTrackService.Object
        );
    }

    [Test]
    public async Task RefreshAccessTokenAsync_ValidRefreshToken_ReturnsNewAccessToken() {
        var refreshToken = "valid-refresh-token";
        var expectedAccessToken = "new-access-token-123";

        _mockAuthService
            .Setup(s => s.RefreshAccessTokenAsync(refreshToken))
            .ReturnsAsync(expectedAccessToken);

        var result = await _spotifyService.RefreshAccessTokenAsync(refreshToken);

        result.Should().Be(expectedAccessToken);
        _mockAuthService.Verify(s => s.RefreshAccessTokenAsync(refreshToken), Times.Once);
    }

    [Test]
    public async Task RefreshAccessTokenAsync_InvalidRefreshToken_ThrowsException() {
        var refreshToken = "invalid-refresh-token";

        _mockAuthService
            .Setup(s => s.RefreshAccessTokenAsync(refreshToken))
            .ThrowsAsync(new InvalidOperationException("Failed to refresh token"));

        Func<Task> act = async () => await _spotifyService.RefreshAccessTokenAsync(refreshToken);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}