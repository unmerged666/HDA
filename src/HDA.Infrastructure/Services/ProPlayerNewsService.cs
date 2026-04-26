using HDA.Domain.Entities;
using HDA.Domain.Enums;
using HDA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HDA.Infrastructure.Services;

// ─────────────────────────── PRO PLAYER SERVICE ───────────────────────────────

public interface IProPlayerService
{
    Task<List<ProPlayer>> GetAllAsync(ProPlayerStatus? status = null);
    Task<ProPlayer?> GetByUserIdAsync(int userId);
    Task<ProPlayer?> GetDetailAsync(int id);
    Task<ProPlayer> CreateOrUpdateProfileAsync(ProPlayer player);
    Task<bool> ApproveAsync(int id, bool approve);
    Task UpdateStatsAsync(int id);
    Task UpdateAvatarAsync(int id, string avatarUrl);
}

public class ProPlayerService : IProPlayerService
{
    private readonly HdaDbContext _ctx;
    public ProPlayerService(HdaDbContext ctx) => _ctx = ctx;

    public Task<List<ProPlayer>> GetAllAsync(ProPlayerStatus? status = null)
    {
        var q = _ctx.ProPlayers.Include(p => p.User).Include(p => p.Team).AsQueryable();
        if (status.HasValue) q = q.Where(p => p.Status == status.Value);
        return q.OrderBy(p => p.Nickname).ToListAsync();
    }

    public Task<ProPlayer?> GetByUserIdAsync(int userId) =>
        _ctx.ProPlayers.Include(p => p.User).Include(p => p.Team)
            .FirstOrDefaultAsync(p => p.UserId == userId);

    public Task<ProPlayer?> GetDetailAsync(int id) =>
        _ctx.ProPlayers
            .Include(p => p.User)
            .Include(p => p.Team)
            .Include(p => p.HeroStats).ThenInclude(h => h.Hero)
            .Include(p => p.MatchStats).ThenInclude(m => m.GameMap).ThenInclude(g => g.Match)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<ProPlayer> CreateOrUpdateProfileAsync(ProPlayer player)
    {
        var existing = await _ctx.ProPlayers.FindAsync(player.Id);
        if (existing == null)
        {
            player.Status = ProPlayerStatus.PendingApproval;
            player.CreatedAt = DateTime.UtcNow;
            _ctx.ProPlayers.Add(player);
        }
        else
        {
            existing.Nickname = player.Nickname;
            existing.RealName = player.RealName;
            existing.Country = player.Country;
            existing.DateOfBirth = player.DateOfBirth;
            existing.Role = player.Role;
            existing.Bio = player.Bio;
            existing.SteamId = player.SteamId;
            existing.TeamId = player.TeamId;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        await _ctx.SaveChangesAsync();
        return player;
    }

    public async Task<bool> ApproveAsync(int id, bool approve)
    {
        var player = await _ctx.ProPlayers.FindAsync(id);
        if (player == null) return false;
        player.Status = approve ? ProPlayerStatus.Approved : ProPlayerStatus.Rejected;
        player.UpdatedAt = DateTime.UtcNow;

        if (approve)
        {
            var user = await _ctx.Users.FindAsync(player.UserId);
            if (user != null) user.Role = UserRole.ProPlayer;
        }

        await _ctx.SaveChangesAsync();
        return true;
    }

    public async Task UpdateStatsAsync(int id)
    {
        var player = await _ctx.ProPlayers
            .Include(p => p.MatchStats)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (player == null) return;

        var stats = player.MatchStats;
        player.TotalMatches = stats.Count;
        if (stats.Any())
        {
            player.AvgKills = stats.Average(s => s.Kills);
            player.AvgDeaths = stats.Average(s => s.Deaths);
            player.AvgAssists = stats.Average(s => s.Assists);
            player.AvgGpm = stats.Average(s => s.Gpm);
            player.AvgXpm = stats.Average(s => s.Xpm);
        }
        player.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();
    }

    public async Task UpdateAvatarAsync(int id, string avatarUrl)
    {
        var player = await _ctx.ProPlayers.FindAsync(id);
        if (player == null) return;
        player.AvatarUrl = avatarUrl;
        player.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();
    }
}

// ─────────────────────────── NEWS SERVICE ─────────────────────────────────────

public interface INewsService
{
    Task<List<NewsArticle>> GetPublishedAsync(string? category = null, int page = 1, int pageSize = 10);
    Task<NewsArticle?> GetBySlugAsync(string slug);
    Task<List<NewsArticle>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<NewsArticle> CreateAsync(NewsArticle article);
    Task UpdateAsync(NewsArticle article);
    Task DeleteAsync(int id);
    Task PublishAsync(int id, bool publish);
}

public class NewsService : INewsService
{
    private readonly HdaDbContext _ctx;
    public NewsService(HdaDbContext ctx) => _ctx = ctx;

    public Task<List<NewsArticle>> GetPublishedAsync(string? category = null, int page = 1, int pageSize = 10)
    {
        var q = _ctx.NewsArticles.Include(n => n.Author)
            .Where(n => n.IsPublished);
        if (!string.IsNullOrEmpty(category)) q = q.Where(n => n.Category == category);
        return q.OrderByDescending(n => n.PublishedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public Task<NewsArticle?> GetBySlugAsync(string slug) =>
        _ctx.NewsArticles.Include(n => n.Author).FirstOrDefaultAsync(n => n.Slug == slug);

    public Task<List<NewsArticle>> GetAllAsync(int page = 1, int pageSize = 20) =>
        _ctx.NewsArticles.Include(n => n.Author)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

    public async Task<NewsArticle> CreateAsync(NewsArticle article)
    {
        article.Slug = GenerateSlug(article.Title);
        article.CreatedAt = DateTime.UtcNow;
        _ctx.NewsArticles.Add(article);
        await _ctx.SaveChangesAsync();
        return article;
    }

    public async Task UpdateAsync(NewsArticle article)
    {
        article.UpdatedAt = DateTime.UtcNow;
        _ctx.NewsArticles.Update(article);
        await _ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var a = await _ctx.NewsArticles.FindAsync(id);
        if (a != null) { _ctx.NewsArticles.Remove(a); await _ctx.SaveChangesAsync(); }
    }

    public async Task PublishAsync(int id, bool publish)
    {
        var a = await _ctx.NewsArticles.FindAsync(id);
        if (a == null) return;
        a.IsPublished = publish;
        a.PublishedAt = publish ? DateTime.UtcNow : null;
        a.UpdatedAt = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();
    }

    private static string GenerateSlug(string title) =>
        string.Join("-", title.ToLower()
            .Replace("'", "").Replace("\"", "")
            .Split(new[] { ' ', ',', '.', '!', '?', '/' }, StringSplitOptions.RemoveEmptyEntries))
        + "-" + DateTime.UtcNow.Ticks.ToString()[^5..];
}
