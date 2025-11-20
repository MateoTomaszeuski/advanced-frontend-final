using API.Interfaces;
using API.Models;
using API.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TUnit.Core;

namespace API.UnitTests.Services;

public class SpotifyTokenServiceTests {
    private readonly Mock<ISpotifyService> _mockSpotifyService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ILogger<SpotifyTokenService>> _mockLogger;
    private readonly SpotifyTokenService _tokenService;

    public SpotifyTokenServiceTests() {
        _mockSpotifyService = new Mock<ISpotifyService>();
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<SpotifyTokenService>>();
        _tokenService = new SpotifyTokenService(
            _mockSpotifyService.Object,
            _mockUserService.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public void IsTokenExpired_WhenNoTokenExpiry_ReturnsTrue() {
        var user = new User {
            Id = 1,
            Email = "test@example.com",
            SpotifyAccessToken = "token",
            SpotifyRefreshToken = "refresh",
            SpotifyTokenExpiry = null
        };

        var result = _tokenService.IsTokenExpired(user);

        result.Should().BeTrue();
    }

    [Test]
    public void IsTokenExpired_WhenTokenExpired_ReturnsTrue() {
        var user = new User {
            Id = 1,
            Email = "test@example.com",
            SpotifyAccessToken = "token",
            SpotifyRefreshToken = "refresh",
            SpotifyTokenExpiry = DateTime.UtcNow.AddMinutes(-10)
        };

        var result = _tokenService.IsTokenExpired(user);

        result.Should().BeTrue();
    }

    [Test]
    public void IsTokenExpired_WhenTokenValidWithBuffer_ReturnsFalse() {
        var user = new User {
            Id = 1,
            Email = "test@example.com",
            SpotifyAccessToken = "token",
            SpotifyRefreshToken = "refresh",
            SpotifyTokenExpiry = DateTime.UtcNow.AddMinutes(10)
        };

        var result = _tokenService.IsTokenExpired(user);

        result.Should().BeFalse();
    }

    [Test]
    public void IsTokenExpired_WhenTokenWithinBufferWindow_ReturnsTrue() {
        var user = new User {
            Id = 1,
            Email = "test@example.com",
            SpotifyAccessToken = "token",
            SpotifyRefreshToken = "refresh",
            SpotifyTokenExpiry = DateTime.UtcNow.AddMinutes(3)
        };

        var result = _tokenService.IsTokenExpired(user);

        result.Should().BeTrue();
    }

    [Test]
    public async Task GetValidAccessTokenAsync_WhenNoAccessToken_ThrowsException() {
        var user = new User {
            Id = 1,
            Email = "test@example.com",
            SpotifyAccessToken = null
        };

        var act = async () => await _tokenService.GetValidAccessTokenAsync(user);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Spotify account not connected");
    }

    [Test]
    public async Task GetValidAccessTokenAsync_WhenTokenValid_ReturnsExistingToken() {
        var validToken = "valid-token";
        var user = new User {
            Id = 1,
            Email = "test@example.com",
            SpotifyAccessToken = validToken,
            SpotifyRefreshToken = "refresh",
            SpotifyTokenExpiry = DateTime.UtcNow.AddHours(1)
        };

        var result = await _tokenService.GetValidAccessTokenAsync(user);

        result.Should().Be(validToken);
        _mockSpotifyService.Verify(s => s.RefreshAccessTokenAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GetValidAccessTokenAsync_WhenTokenExpired_RefreshesToken() {
        var oldToken = "old-token";
        var newToken = "new-token";
        var refreshToken = "refresh-token";

        var user = new User {
            Id = 1,
            Email = "test@example.com",
            SpotifyAccessToken = oldToken,
            SpotifyRefreshToken = refreshToken,
            SpotifyTokenExpiry = DateTime.UtcNow.AddMinutes(-10)
        };

        _mockSpotifyService
            .Setup(s => s.RefreshAccessTokenAsync(refreshToken))
            .ReturnsAsync(newToken);

        _mockUserService
            .Setup(s => s.UpdateUserAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var result = await _tokenService.GetValidAccessTokenAsync(user);

        result.Should().Be(newToken);
        user.SpotifyAccessToken.Should().Be(newToken);
        user.SpotifyTokenExpiry.Should().BeCloseTo(DateTime.UtcNow.AddHours(1), TimeSpan.FromMinutes(1));

        _mockSpotifyService.Verify(s => s.RefreshAccessTokenAsync(refreshToken), Times.Once);
        _mockUserService.Verify(s => s.UpdateUserAsync(user), Times.Once);
    }

    [Test]
    public async Task GetValidAccessTokenAsync_WhenNoRefreshToken_ThrowsException() {
        var user = new User {
            Id = 1,
            Email = "test@example.com",
            SpotifyAccessToken = "token",
            SpotifyRefreshToken = null,
            SpotifyTokenExpiry = DateTime.UtcNow.AddMinutes(-10)
        };

        var act = async () => await _tokenService.GetValidAccessTokenAsync(user);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Spotify refresh token not available*");
    }

    [Test]
    public async Task GetValidAccessTokenAsync_WhenRefreshFails_ThrowsException() {
        var user = new User {
            Id = 1,
            Email = "test@example.com",
            SpotifyAccessToken = "token",
            SpotifyRefreshToken = "refresh",
            SpotifyTokenExpiry = DateTime.UtcNow.AddMinutes(-10)
        };

        _mockSpotifyService
            .Setup(s => s.RefreshAccessTokenAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Refresh failed"));

        var act = async () => await _tokenService.GetValidAccessTokenAsync(user);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to refresh Spotify token*");
    }
}