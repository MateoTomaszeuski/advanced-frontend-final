using API.Models;

namespace API.Interfaces;

public interface IUserService {
    Task<User> GetOrCreateUserAsync(string email, string? displayName = null);
    Task<User?> GetUserByEmailAsync(string email);
    Task UpdateUserAsync(User user);
}