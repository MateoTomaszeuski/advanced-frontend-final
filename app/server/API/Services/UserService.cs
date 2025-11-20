using System.Security.Claims;
using API.Interfaces;
using API.Models;
using API.Repositories;

namespace API.Services;

public class UserService : IUserService {
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger) {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<User> GetOrCreateUserAsync(string email, string? displayName = null) {
        var user = await _userRepository.GetByEmailAsync(email);

        if (user == null) {
            user = new User {
                Email = email,
                DisplayName = displayName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            user = await _userRepository.CreateAsync(user);
            _logger.LogInformation("Created new user: {Email}", email);
        }

        return user;
    }

    public async Task<User?> GetUserByEmailAsync(string email) {
        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task UpdateUserAsync(User user) {
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
    }
}