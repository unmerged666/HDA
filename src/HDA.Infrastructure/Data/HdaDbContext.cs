using HDA.Domain.Entities;
using HDA.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HDA.Infrastructure.Data;

public class HdaDbContext : DbContext
{
    public HdaDbContext(DbContextOptions<HdaDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<ProPlayer> ProPlayers => Set<ProPlayer>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<TournamentParticipant> TournamentParticipants => Set<TournamentParticipant>();
    public DbSet<TournamentStage> TournamentStages => Set<TournamentStage>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<GameMap> GameMaps => Set<GameMap>();
    public DbSet<MatchPlayerStat> MatchPlayerStats => Set<MatchPlayerStat>();
    public DbSet<GameMapDraft> GameMapDrafts => Set<GameMapDraft>();
    public DbSet<Hero> Heroes => Set<Hero>();
    public DbSet<PlayerHeroStat> PlayerHeroStats => Set<PlayerHeroStat>();
    public DbSet<TeamHeroStat> TeamHeroStats => Set<TeamHeroStat>();
    public DbSet<NewsArticle> NewsArticles => Set<NewsArticle>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.Username).IsUnique();
            e.Property(x => x.Role).HasConversion<string>();
        });

        // ── ProPlayer ─────────────────────────────────────────────────────────
        modelBuilder.Entity<ProPlayer>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User)
                .WithOne(x => x.ProPlayerProfile)
                .HasForeignKey<ProPlayer>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Team)
                .WithMany(x => x.Players)
                .HasForeignKey(x => x.TeamId)
                .OnDelete(DeleteBehavior.SetNull);
            e.Property(x => x.Role).HasConversion<string>();
            e.Property(x => x.Status).HasConversion<string>();
        });

        // ── Team ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<Team>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Name).IsUnique();
        });

        // ── Tournament ────────────────────────────────────────────────────────
        modelBuilder.Entity<Tournament>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.PrizePool).HasColumnType("decimal(18,2)");
            e.Property(x => x.Tier).HasConversion<string>();
            e.Property(x => x.Status).HasConversion<string>();
        });

        modelBuilder.Entity<TournamentParticipant>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TournamentId, x.TeamId }).IsUnique();
            e.HasOne(x => x.Tournament).WithMany(x => x.Participants).HasForeignKey(x => x.TournamentId);
            e.HasOne(x => x.Team).WithMany(x => x.TournamentParticipations).HasForeignKey(x => x.TeamId);
            e.Property(x => x.PrizeWon).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<TournamentStage>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Tournament).WithMany(x => x.Stages).HasForeignKey(x => x.TournamentId);
            e.Property(x => x.Type).HasConversion<string>();
        });

        // ── Match ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Match>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Tournament).WithMany(x => x.Matches).HasForeignKey(x => x.TournamentId);
            e.HasOne(x => x.Stage).WithMany(x => x.Matches).HasForeignKey(x => x.StageId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.TeamA).WithMany(x => x.HomeMatches).HasForeignKey(x => x.TeamAId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.TeamB).WithMany(x => x.AwayMatches).HasForeignKey(x => x.TeamBId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Winner).WithMany().HasForeignKey(x => x.WinnerId).OnDelete(DeleteBehavior.SetNull);
            e.Property(x => x.Format).HasConversion<string>();
            e.Property(x => x.Status).HasConversion<string>();
        });

        // ── GameMap ───────────────────────────────────────────────────────────
        modelBuilder.Entity<GameMap>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Match).WithMany(x => x.Games).HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.RadiantTeam).WithMany().HasForeignKey(x => x.RadiantTeamId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.DireTeam).WithMany().HasForeignKey(x => x.DireTeamId).OnDelete(DeleteBehavior.SetNull);
        });

        // ── MatchPlayerStat ───────────────────────────────────────────────────
        modelBuilder.Entity<MatchPlayerStat>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.GameMap).WithMany(x => x.PlayerStats).HasForeignKey(x => x.GameMapId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.ProPlayer).WithMany(x => x.MatchStats).HasForeignKey(x => x.ProPlayerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Hero).WithMany().HasForeignKey(x => x.HeroId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── GameMapDraft ──────────────────────────────────────────────────────
        modelBuilder.Entity<GameMapDraft>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.GameMap).WithMany(x => x.Drafts).HasForeignKey(x => x.GameMapId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Hero).WithMany().HasForeignKey(x => x.HeroId).OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.Action).HasConversion<string>();
        });

        // ── Hero ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<Hero>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.OpenDotaId).IsUnique();
            e.Property(x => x.Roles).HasColumnType("text[]");
        });

        // ── PlayerHeroStat ────────────────────────────────────────────────────
        modelBuilder.Entity<PlayerHeroStat>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.ProPlayerId, x.HeroId }).IsUnique();
            e.HasOne(x => x.ProPlayer).WithMany(x => x.HeroStats).HasForeignKey(x => x.ProPlayerId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Hero).WithMany(x => x.PlayerStats).HasForeignKey(x => x.HeroId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── TeamHeroStat ──────────────────────────────────────────────────────
        modelBuilder.Entity<TeamHeroStat>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TeamId, x.HeroId }).IsUnique();
            e.HasOne(x => x.Team).WithMany(x => x.HeroStats).HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Hero).WithMany(x => x.TeamStats).HasForeignKey(x => x.HeroId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── NewsArticle ───────────────────────────────────────────────────────
        modelBuilder.Entity<NewsArticle>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Slug).IsUnique();
            e.HasOne(x => x.Author).WithMany().HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.Restrict);
        });

        // ── ActivityLog ───────────────────────────────────────────────────────
        modelBuilder.Entity<ActivityLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User).WithMany(x => x.ActivityLogs).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        });
    }
}
