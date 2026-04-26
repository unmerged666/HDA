using HDA.Domain.Enums;

namespace HDA.Domain.Entities;

public class Match
{
    public int Id { get; set; }
    public int TournamentId { get; set; }
    public int? StageId { get; set; }
    public int TeamAId { get; set; }
    public int TeamBId { get; set; }
    public MatchFormat Format { get; set; } // bo1, bo2, bo3, bo5
    public MatchStatus Status { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public int? TeamAScore { get; set; }
    public int? TeamBScore { get; set; }
    public int? WinnerId { get; set; }
    public string? TwitchUrl { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? PandaScoreId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Tournament Tournament { get; set; } = null!;
    public TournamentStage? Stage { get; set; }
    public Team TeamA { get; set; } = null!;
    public Team TeamB { get; set; } = null!;
    public Team? Winner { get; set; }
    public ICollection<GameMap> Games { get; set; } = new List<GameMap>();
}

public class GameMap
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public int MapNumber { get; set; } // 1, 2, 3...
    public int? RadiantTeamId { get; set; }
    public int? DireTeamId { get; set; }
    public int? WinnerTeamId { get; set; }
    public int DurationSeconds { get; set; }
    public string? DotaMatchId { get; set; } // from Valve/OpenDota
    public DateTime? PlayedAt { get; set; }

    // Scoreboard summary
    public int RadiantKills { get; set; }
    public int DireKills { get; set; }

    public Match Match { get; set; } = null!;
    public Team? RadiantTeam { get; set; }
    public Team? DireTeam { get; set; }
    public ICollection<MatchPlayerStat> PlayerStats { get; set; } = new List<MatchPlayerStat>();
    public ICollection<GameMapDraft> Drafts { get; set; } = new List<GameMapDraft>();
}

public class MatchPlayerStat
{
    public int Id { get; set; }
    public int GameMapId { get; set; }
    public int ProPlayerId { get; set; }
    public int HeroId { get; set; }
    public bool IsRadiant { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int Gpm { get; set; }
    public int Xpm { get; set; }
    public int NetWorth { get; set; }
    public int HeroDamage { get; set; }
    public int TowerDamage { get; set; }
    public int HeroHealing { get; set; }
    public int LastHits { get; set; }
    public int Denies { get; set; }
    public int Level { get; set; }

    public GameMap GameMap { get; set; } = null!;
    public ProPlayer ProPlayer { get; set; } = null!;
    public Hero Hero { get; set; } = null!;
}

public class GameMapDraft
{
    public int Id { get; set; }
    public int GameMapId { get; set; }
    public int HeroId { get; set; }
    public DraftAction Action { get; set; } // Pick or Ban
    public bool IsRadiant { get; set; }
    public int Order { get; set; }

    public GameMap GameMap { get; set; } = null!;
    public Hero Hero { get; set; } = null!;
}
