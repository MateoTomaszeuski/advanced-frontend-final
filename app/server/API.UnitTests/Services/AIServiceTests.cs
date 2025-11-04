using System.Net;
using System.Text;
using System.Text.Json;
using API.Models.AI;
using API.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using TUnit.Core;

namespace API.UnitTests.Services;

public class AIServiceTests
{
    private readonly Mock<ILogger<AIService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly AIService _aiService;

    public AIServiceTests()
    {
        _mockLogger = new Mock<ILogger<AIService>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);

        _mockConfiguration.Setup(c => c["AI:ApiBaseUrl"]).Returns("https://test.api/chat/completions");
        _mockConfiguration.Setup(c => c["AI:Model"]).Returns("test-model");
        _mockConfiguration.Setup(c => c["AI:ApiKey"]).Returns("test-api-key");

        _aiService = new AIService(httpClient, _mockConfiguration.Object, _mockLogger.Object);
    }

    [Test]
    public void EstimateTokenCount_ReturnsCorrectEstimate()
    {
        var text = "This is a test string with multiple words";
        var tokenCount = _aiService.EstimateTokenCount(text);
        
        tokenCount.Should().BeGreaterThan(0);
        tokenCount.Should().Be((int)Math.Ceiling(text.Length / 4.0));
    }

    [Test]
    public void CountMessageTokens_CountsAllMessages()
    {
        var messages = new List<AIMessage>
        {
            new AIMessage("user", "Hello, how are you?"),
            new AIMessage("assistant", "I'm doing well, thank you!")
        };

        var tokenCount = _aiService.CountMessageTokens(messages);
        
        tokenCount.Should().BeGreaterThan(0);
    }

    [Test]
    public void NeedsSummarization_ReturnsFalseForSmallTokenCount()
    {
        var tokenCount = 50000;
        
        var result = _aiService.NeedsSummarization(tokenCount);
        
        result.Should().BeFalse();
    }
}
