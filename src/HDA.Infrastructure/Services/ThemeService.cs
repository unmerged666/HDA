namespace HDA.Infrastructure.Services;

public class ThemeService
{
    private bool _isDark = true;
    public bool IsDark => _isDark;
    public event Func<Task>? OnChangeAsync;

    public async Task ToggleAsync()
    {
        _isDark = !_isDark;
        if (OnChangeAsync != null)
            await OnChangeAsync.Invoke();
    }

    public void Toggle()
    {
        _isDark = !_isDark;
        OnChangeAsync?.Invoke();
    }
}
