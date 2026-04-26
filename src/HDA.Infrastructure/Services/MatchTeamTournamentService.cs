using HDA.Domain.Entities;
using HDA.Domain.Enums;
using HDA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HDA.Infrastructure.Services;

// ─────────────────────────── MATCH SERVICE ────────────────────────────────────

public interface IMatchService
{
    Task<List<Match>> GetUpcomingAsync(int count = 10);
    Task<List<Match>> GetRecentAsync(int count = 10);
    Task<List<Match>> GetLiveAsync();
    Task<List<Match>> GetByTournamentAsync(int tournamentId);
    Task<Match?> GetDetailAsync(int id);
    Task<Match> CreateAsync(Match match);
    Task UpdateAsync(Match match);
    Task DeleteAsync(int id);
}

public class MatchService : IMatchService
{
    private readonly HdaDbContext _ctx;
    public MatchService(HdaDbContext ctx) => _ctx = ctx;

    private IQueryable<Match> BaseQuery() => _ctx.Matches
        .Include(m => m.TeamA)
        .Include(m => m.TeamB)
        .Include(m => m.Winner)
        .Include(m => m.Tournament)
        .Include(m => m.Stage);

    public Task<List<Match>> GetUpcomingAsync(int count = 10) => BaseQuery()
        .Where(m => m.Status == MatchStatus.Scheduled)
        .OrderBy(m => m.ScheduledAt)
        .Take(count).ToListAsync();

    public Task<List<Match>> GetRecentAsync(int count = 10) => BaseQuery()
        .Where(m => m.Status == MatchStatus.Completed)
        .OrderByDescending(m => m.FinishedAt)
        .Take(count).ToListAsync();

    public Task<List<Match>> GetLiveAsync() => BaseQuery()
        .Where(m => m.Status == MatchStatus.Live)
        .ToListAsync();

    public Task<List<Match>> GetByTournamentAsync(int tournamentId) => BaseQuery()
        .Where(m => m.TournamentId == tournamentId)
        .OrderBy(m => m.ScheduledAt).ToListAsync();

    public Task<Match?> GetDetailAsync(int id) => BaseQuery()
        .Include(m => m.Games).ThenInclude(g => g.PlayerStats).ThenInclude(p => p.ProPlayer)
        .Include(m => m.Games).ThenInclude(g => g.PlayerStats).ThenInclude(p => p.Hero)
        .Include(m => m.Games).ThenInclude(g => g.Drafts).ThenInclude(d => d.Hero)
        .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<Match> CreateAsync(Match match)
    {
        _ctx.Matches.Add(match);
        await _ctx.SaveChangesAsync();
        return match;
    }

    public async Task UpdateAsync(Match match)
    {
        match.UpdatedAt = DateTime.UtcNow;
        _ctx.Matches.Update(match);
        await _ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var m = await _ctx.Matches.FindAsync(id);
        if (m != null) { _ctx.Matches.Remove(m); await _ctx.SaveChangesAsync(); }
    }
}

// ─────────────────────────── TEAM SERVICE ─────────────────────────────────────

public interface ITeamService
{
    Task<List<Team>> GetRankingsAsync();
    Task<Team?> GetDetailAsync(int id);
    Task<Team?> GetByNameAsync(string name);
    Task<Team> CreateAsync(Team team);
    Task UpdateAsync(Team team);
    Task DeleteAsync(int id);
    Task<List<Match>> GetRecentMatchesAsync(int teamId, int count = 5);
}

public class TeamService : ITeamService
{
    private readonly HdaDbContext _ctx;
    public TeamService(HdaDbContext ctx) => _ctx = ctx;

    public Task<List<Team>> GetRankingsAsync() => _ctx.Teams
        .OrderBy(t => t.WorldRank)
        .ToListAsync();

    public Task<Team?> GetDetailAsync(int id) => _ctx.Teams
        .Include(t => t.Players).ThenInclude(p => p.User)
        .Include(t => t.HeroStats).ThenInclude(s => s.Hero)
        .FirstOrDefaultAsync(t => t.Id == id);

    public Task<Team?> GetByNameAsync(string name) => _ctx.Teams.FirstOrDefaultAsync(t => t.Name == name);

    public async Task<Team> CreateAsync(Team team)
    {
        _ctx.Teams.Add(team);
        await _ctx.SaveChangesAsync();
        return team;
    }

    public async Task UpdateAsync(Team team)
    {
        team.UpdatedAt = DateTime.UtcNow;
        _ctx.Teams.Update(team);
        await _ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var t = await _ctx.Teams.FindAsync(id);
        if (t != null) { _ctx.Teams.Remove(t); await _ctx.SaveChangesAsync(); }
    }

    public Task<List<Match>> GetRecentMatchesAsync(int teamId, int count = 5) =>
        _ctx.Matches
            .Include(m => m.TeamA).Include(m => m.TeamB).Include(m => m.Tournament)
            .Where(m => (m.TeamAId == teamId || m.TeamBId == teamId) && m.Status == MatchStatus.Completed)
            .OrderByDescending(m => m.FinishedAt)
            .Take(count).ToListAsync();
}

// ─────────────────────────── TOURNAMENT SERVICE ───────────────────────────────

public interface ITournamentService
{
    Task<List<Tournament>> GetAllAsync(TournamentStatus? status = null);
    Task<Tournament?> GetDetailAsync(int id);
    Task<Tournament> CreateAsync(Tournament tournament);
    Task UpdateAsync(Tournament tournament);
    Task DeleteAsync(int id);
}

public class TournamentService : ITournamentService
{
    private readonly HdaDbContext _ctx;
    public TournamentService(HdaDbContext ctx) => _ctx = ctx;

    public Task<List<Tournament>> GetAllAsync(TournamentStatus? status = null)
    {
        var q = _ctx.Tournaments.AsQueryable();
        if (status.HasValue) q = q.Where(t => t.Status == status.Value);
        return q.OrderByDescending(t => t.StartDate).ToListAsync();
    }

    public Task<Tournament?> GetDetailAsync(int id) => _ctx.Tournaments
        .Include(t => t.Participants).ThenInclude(p => p.Team)
        .Include(t => t.Stages).ThenInclude(s => s.Matches).ThenInclude(m => m.TeamA)
        .Include(t => t.Stages).ThenInclude(s => s.Matches).ThenInclude(m => m.TeamB)
        .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<Tournament> CreateAsync(Tournament tournament)
    {
        _ctx.Tournaments.Add(tournament);
        await _ctx.SaveChangesAsync();
        return tournament;
    }

    public async Task UpdateAsync(Tournament tournament)
    {
        tournament.UpdatedAt = DateTime.UtcNow;
        _ctx.Tournaments.Update(tournament);
        await _ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var t = await _ctx.Tournaments.FindAsync(id);
        if (t != null) { _ctx.Tournaments.Remove(t); await _ctx.SaveChangesAsync(); }
    }
}
