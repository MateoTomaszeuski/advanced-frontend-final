using API.Models;
using API.Repositories;
using API.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TUnit.Core;

namespace API.UnitTests.Services;

public class UserServiceTests {
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly UserService _userService;

    public UserServiceTests() {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<UserService>>();
        _userService = new UserService(_mockUserRepository.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetOrCreateUserAsync_WhenUserExists_ReturnsExistingUser() {
        var email = "test@example.com";
        var existingUser = new User {
            Id = 1,
            Email = email,
            DisplayName = "Test User",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(existingUser);

        var result = await _userService.GetOrCreateUserAsync(email);

        result.Should().NotBeNull();
        result.Email.Should().Be(email);
        result.Id.Should().Be(1);
        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public async Task GetOrCreateUserAsync_WhenUserDoesNotExist_CreatesNewUser() {
        var email = "newuser@example.com";
        var displayName = "New User";

        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        var createdUser = new User {
            Id = 2,
            Email = email,
            DisplayName = displayName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockUserRepository
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);

        var result = await _userService.GetOrCreateUserAsync(email, displayName);

        result.Should().NotBeNull();
        result.Email.Should().Be(email);
        result.DisplayName.Should().Be(displayName);
        _mockUserRepository.Verify(r => r.CreateAsync(It.Is<User>(u =>
            u.Email == email && u.DisplayName == displayName)), Times.Once);
    }

    [Test]
    public async Task GetUserByEmailAsync_ReturnsUser() {
        var email = "test@example.com";
        var user = new User {
            Id = 1,
            Email = email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(email))
            .ReturnsAsync(user);

        var result = await _userService.GetUserByEmailAsync(email);

        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    [Test]
    public async Task UpdateUserAsync_UpdatesUserAndSetsUpdatedAt() {
        var user = new User {
            Id = 1,
            Email = "test@example.com",
            DisplayName = "Updated Name",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var originalUpdatedAt = user.UpdatedAt;

        _mockUserRepository
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        await _userService.UpdateUserAsync(user);

        user.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        _mockUserRepository.Verify(r => r.UpdateAsync(user), Times.Once);
    }
}