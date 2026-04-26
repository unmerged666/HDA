namespace HDA.Domain.Entities;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty; // e.g. "OG", "NAVI"
    public string? LogoUrl { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public string? Organization { get; set; }
    public DateTime Founded { get; set; }
    public int RatingPoints { get; set; }
    public int WorldRank { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<ProPlayer> Players { get; set; } = new List<ProPlayer>();
    public ICollection<Match> HomeMatches { get; set; } = new List<Match>();
    public ICollection<Match> AwayMatches { get; set; } = new List<Match>();
    public ICollection<TournamentParticipant> TournamentParticipations { get; set; } = new List<TournamentParticipant>();
    public ICollection<TeamHeroStat> HeroStats { get; set; } = new List<TeamHeroStat>();
}
