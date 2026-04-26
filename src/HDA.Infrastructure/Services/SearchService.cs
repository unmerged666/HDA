using HDA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HDA.Infrastructure.Services;

public class SearchResult
{
    public string Type { get; set; } = "";  // team, player, tournament, news
    public string Title { get; set; } = "";
    public string? Subtitle { get; set; }
    public string Url { get; set; } = "";
    public string? ImageUrl { get; set; }
}

public interface ISearchService
{
    Task<List<SearchResult>> SearchAsync(string query, int limit = 8);
}

public class SearchService : ISearchService
{
    private readonly HdaDbContext _ctx;
    public SearchService(HdaDbContext ctx) => _ctx = ctx;

    public async Task<List<SearchResult>> SearchAsync(string query, int limit = 8)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return [];

        var q = query.ToLower();
        var results = new List<SearchResult>();

        var teams = await _ctx.Teams
            .Where(t => t.Name.ToLower().Contains(q) || t.Tag.ToLower().Contains(q))
            .Take(3).ToListAsync();
        results.AddRange(teams.Select(t => new SearchResult
        {
            Type = "team", Title = t.Name,
            Subtitle = $"{t.Tag} · {t.Region}",
            Url = $"/teams/{t.Id}", ImageUrl = t.LogoUrl
        }));

        var players = await _ctx.ProPlayers
            .Include(p => p.Team)
            .Where(p => p.Nickname.ToLower().Contains(q) || (p.RealName != null && p.RealName.ToLower().Contains(q)))
            .Take(3).ToListAsync();
        results.AddRange(players.Select(p => new SearchResult
        {
            Type = "player", Title = p.Nickname,
            Subtitle = $"{p.RealName} · {p.Team?.Tag ?? "Free Agent"}",
            Url = $"/players/{p.Id}", ImageUrl = p.AvatarUrl
        }));

        var tournaments = await _ctx.Tournaments
            .Where(t => t.Name.ToLower().Contains(q))
            .Take(2).ToListAsync();
        results.AddRange(tournaments.Select(t => new SearchResult
        {
            Type = "tournament", Title = t.Name,
            Subtitle = $"{t.Organizer} · {t.Region}",
            Url = $"/tournaments/{t.Id}", ImageUrl = t.LogoUrl
        }));

        var news = await _ctx.NewsArticles
            .Where(n => n.IsPublished && n.Title.ToLower().Contains(q))
            .Take(2).ToListAsync();
        results.AddRange(news.Select(n => new SearchResult
        {
            Type = "news", Title = n.Title,
            Subtitle = n.Category,
            Url = $"/news/{n.Slug}"
        }));

        return results.Take(limit).ToList();
    }
}
