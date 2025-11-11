using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using API.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using TUnit.Core;

namespace API.UnitTests.Services;

public class SpotifyServiceTests
{
    private readonly Mock<ILogger<SpotifyService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly SpotifyService _spotifyService;

    public SpotifyServiceTests()
    {
        _mockLogger = new Mock<ILogger<SpotifyService>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://api.spotify.com/")
        };

        _mockConfiguration.Setup(c => c["Spotify:ClientId"]).Returns("test-client-id");
        _mockConfiguration.Setup(c => c["Spotify:ClientSecret"]).Returns("test-client-secret");

        _spotifyService = new SpotifyService(httpClient, _mockLogger.Object, _mockConfiguration.Object);
    }

    [Test]
    public async Task RefreshAccessTokenAsync_ValidRefreshToken_ReturnsNewAccessToken()
    {
        // Arrange
        var refreshToken = "valid-refresh-token";
        var expectedAccessToken = "new-access-token-123";

        var tokenResponse = new
        {
            access_token = expectedAccessToken,
            token_type = "Bearer",
            expires_in = 3600,
            scope = "user-read-private user-read-email"
        };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(tokenResponse)
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString().Contains("token")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _spotifyService.RefreshAccessTokenAsync(refreshToken);

        // Assert
        result.Should().Be(expectedAccessToken);

        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri!.ToString().Contains("token")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Test]
    public async Task RefreshAccessTokenAsync_InvalidRefreshToken_ThrowsException()
    {
        // Arrange
        var refreshToken = "invalid-refresh-token";

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.BadRequest,
            Content = new StringContent(JsonSerializer.Serialize(new { error = "invalid_grant" }))
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        Func<Task> act = async () => await _spotifyService.RefreshAccessTokenAsync(refreshToken);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Test]
    public async Task RefreshAccessTokenAsync_NetworkError_ThrowsException()
    {
        // Arrange
        var refreshToken = "valid-refresh-token";

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        Func<Task> act = async () => await _spotifyService.RefreshAccessTokenAsync(refreshToken);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Network error");
    }

    [Test]
    public async Task RefreshAccessTokenAsync_EmptyAccessToken_ThrowsException()
    {
        // Arrange
        var refreshToken = "valid-refresh-token";

        var tokenResponse = new
        {
            access_token = "",
            token_type = "Bearer",
            expires_in = 3600
        };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(tokenResponse)
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        Func<Task> act = async () => await _spotifyService.RefreshAccessTokenAsync(refreshToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*access token*");
    }

    [Test]
    public async Task RefreshAccessTokenAsync_SendsCorrectAuthorizationHeader()
    {
        // Arrange
        var refreshToken = "test-refresh-token";
        HttpRequestMessage? capturedRequest = null;

        var tokenResponse = new
        {
            access_token = "new-token",
            token_type = "Bearer",
            expires_in = 3600
        };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(tokenResponse)
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(responseMessage);

        // Act
        await _spotifyService.RefreshAccessTokenAsync(refreshToken);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.Authorization.Should().NotBeNull();
        capturedRequest.Headers.Authorization!.Scheme.Should().Be("Basic");

        // Verify the authorization header contains base64 encoded client credentials
        var authValue = capturedRequest.Headers.Authorization.Parameter;
        authValue.Should().NotBeNullOrEmpty();

        var decodedAuth = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(authValue!));
        decodedAuth.Should().Contain("test-client-id");
        decodedAuth.Should().Contain("test-client-secret");
    }

    [Test]
    public async Task RefreshAccessTokenAsync_SendsCorrectFormData()
    {
        // Arrange
        var refreshToken = "test-refresh-token";
        HttpRequestMessage? capturedRequest = null;

        var tokenResponse = new
        {
            access_token = "new-token",
            token_type = "Bearer",
            expires_in = 3600
        };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(tokenResponse)
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(responseMessage);

        // Act
        await _spotifyService.RefreshAccessTokenAsync(refreshToken);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Content.Should().NotBeNull();

        var formContent = await capturedRequest.Content!.ReadAsStringAsync();
        formContent.Should().Contain("grant_type=refresh_token");
        formContent.Should().Contain($"refresh_token={refreshToken}");
    }
}
