using HDA.Domain.Entities;
using HDA.Domain.Enums;
using HDA.Infrastructure.Data;
using HDA.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HDA.Infrastructure.Services;

public interface IAuthService
{
    Task<(bool Success, User? User, string? Error)> LoginAsync(string email, string password);
    Task<(bool Success, User? User, string? Error)> RegisterAsync(string username, string email, string password);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task LogActivityAsync(int? userId, string action, string? entityType = null, int? entityId = null, string? details = null, string? ip = null);
}

public class AuthService : IAuthService
{
    private readonly HdaDbContext _context;

    public AuthService(HdaDbContext context) => _context = context;

    public async Task<(bool Success, User? User, string? Error)> LoginAsync(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        if (user == null) return (false, null, "Invalid email or password.");
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return (false, null, "Invalid email or password.");

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await LogActivityAsync(user.Id, "Login", ip: null);
        return (true, user, null);
    }

    public async Task<(bool Success, User? User, string? Error)> RegisterAsync(string username, string email, string password)
    {
        if (await _context.Users.AnyAsync(u => u.Email == email))
            return (false, null, "Email already in use.");
        if (await _context.Users.AnyAsync(u => u.Username == username))
            return (false, null, "Username already taken.");

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.Regular,
            CreatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        await LogActivityAsync(user.Id, "Register");
        return (true, user, null);
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || !BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash)) return false;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task LogActivityAsync(int? userId, string action, string? entityType = null, int? entityId = null, string? details = null, string? ip = null)
    {
        _context.ActivityLogs.Add(new ActivityLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            IpAddress = ip,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }
}
