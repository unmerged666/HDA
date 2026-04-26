using HDA.Domain.Enums;

namespace HDA.Domain.Entities;

public class ProPlayer
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string? RealName { get; set; }
    public string? Country { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public PlayerRole Role { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? SteamId { get; set; }
    public int? TeamId { get; set; }
    public ProPlayerStatus Status { get; set; } = ProPlayerStatus.PendingApproval;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Career stats
    public int TotalMatches { get; set; }
    public int Wins { get; set; }
    public double AvgKills { get; set; }
    public double AvgDeaths { get; set; }
    public double AvgAssists { get; set; }
    public double AvgGpm { get; set; }
    public double AvgXpm { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Team? Team { get; set; }
    public ICollection<PlayerHeroStat> HeroStats { get; set; } = new List<PlayerHeroStat>();
    public ICollection<MatchPlayerStat> MatchStats { get; set; } = new List<MatchPlayerStat>();
}
