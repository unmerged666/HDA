using HDA.Domain.Enums;

namespace HDA.Domain.Entities;

public class Tournament
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Organizer { get; set; }
    public string? Region { get; set; }
    public decimal PrizePool { get; set; }
    public string? PrizePoolCurrency { get; set; } = "USD";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TournamentTier Tier { get; set; }
    public TournamentStatus Status { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public string? LiquipediaUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<TournamentParticipant> Participants { get; set; } = new List<TournamentParticipant>();
    public ICollection<Match> Matches { get; set; } = new List<Match>();
    public ICollection<TournamentStage> Stages { get; set; } = new List<TournamentStage>();
}

public class TournamentParticipant
{
    public int Id { get; set; }
    public int TournamentId { get; set; }
    public int TeamId { get; set; }
    public int? FinalPlacement { get; set; }
    public decimal? PrizeWon { get; set; }

    public Tournament Tournament { get; set; } = null!;
    public Team Team { get; set; } = null!;
}

public class TournamentStage
{
    public int Id { get; set; }
    public int TournamentId { get; set; }
    public string Name { get; set; } = string.Empty; // "Group Stage", "Playoffs"
    public StageType Type { get; set; }
    public int Order { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public Tournament Tournament { get; set; } = null!;
    public ICollection<Match> Matches { get; set; } = new List<Match>();
}
