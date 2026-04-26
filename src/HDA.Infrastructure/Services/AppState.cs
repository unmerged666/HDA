using HDA.Domain.Entities;
using HDA.Domain.Enums;

namespace HDA.Infrastructure.Services;

public class AppState
{
    public User? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;
    public bool IsAdmin => CurrentUser?.Role == UserRole.Admin;
    public bool IsProPlayer => CurrentUser?.Role == UserRole.ProPlayer || IsAdmin;

    // Use Action (sync) - callers must use InvokeAsync themselves
    public event Action? OnChange;

    public void SetUser(User? user)
    {
        CurrentUser = user;
        OnChange?.Invoke();
    }

    public void Logout()
    {
        CurrentUser = null;
        OnChange?.Invoke();
    }
}
