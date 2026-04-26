namespace HDA.Domain.Entities;

public class Hero
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LocalizedName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? IconUrl { get; set; }
    public string? PrimaryAttribute { get; set; } // str/agi/int/universal
    public string? AttackType { get; set; } // Melee/Ranged
    public string[]? Roles { get; set; }
    public int OpenDotaId { get; set; }

    public ICollection<PlayerHeroStat> PlayerStats { get; set; } = new List<PlayerHeroStat>();
    public ICollection<TeamHeroStat> TeamStats { get; set; } = new List<TeamHeroStat>();
}

public class PlayerHeroStat
{
    public int Id { get; set; }
    public int ProPlayerId { get; set; }
    public int HeroId { get; set; }
    public int GamesPlayed { get; set; }
    public int Wins { get; set; }
    public double AvgKda { get; set; }
    public double AvgGpm { get; set; }

    public ProPlayer ProPlayer { get; set; } = null!;
    public Hero Hero { get; set; } = null!;
}

public class TeamHeroStat
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public int HeroId { get; set; }
    public int Picks { get; set; }
    public int Bans { get; set; }
    public int Wins { get; set; }

    public Team Team { get; set; } = null!;
    public Hero Hero { get; set; } = null!;
}

public class NewsArticle
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? ImageUrl { get; set; }
    public string? Category { get; set; } // Transfer, Result, Patch, Interview
    public bool IsPublished { get; set; }
    public int AuthorId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }

    public User Author { get; set; } = null!;
}

public class ActivityLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
