using BCrypt.Net;
using Npgsql;
using HDA.Infrastructure.Data;
using HDA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<HdaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Blazor + SignalR with extended timeouts ───────────────────────────────────
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.WebHost.ConfigureKestrel(o =>
    o.Limits.MaxRequestBodySize = 10 * 1024 * 1024); // 10MB
builder.Services.AddSignalR(o =>
{
    o.MaximumReceiveMessageSize = 10 * 1024 * 1024;
    o.EnableDetailedErrors = true;
    o.KeepAliveInterval = TimeSpan.FromSeconds(15);
    o.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
});
builder.Services.AddMudServices();

// ── HTTP ──────────────────────────────────────────────────────────────────────
builder.Services.AddHttpClient<IOpenDotaService, OpenDotaService>(c =>
    c.Timeout = TimeSpan.FromSeconds(30));

// ── App Services ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<ITournamentService, TournamentService>();
builder.Services.AddScoped<IProPlayerService, ProPlayerService>();
builder.Services.AddScoped<INewsService, NewsService>();
builder.Services.AddScoped<IOpenDotaService, OpenDotaService>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IFileUploadService>(sp =>
    new FileUploadService(builder.Environment.WebRootPath));
builder.Services.AddSingleton<AppState>();
builder.Services.AddSingleton<ThemeService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// ── Seed + startup tasks ──────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var ctx  = scope.ServiceProvider.GetRequiredService<HdaDbContext>();
    var odota = scope.ServiceProvider.GetRequiredService<IOpenDotaService>();

    // Create schema using the same context that will seed
    // This ensures the same connection sees the tables it just created
    await ctx.Database.EnsureCreatedAsync();

    await DbSeeder.SeedAsync(ctx);

    // Ensure admin user exists with correct credentials (update if email changed)
    var adminUser = await ctx.Users.FirstOrDefaultAsync(u => u.Role == HDA.Domain.Enums.UserRole.Admin);
    if (adminUser != null)
    {
        adminUser.Email    = "admin12@example.com";
        adminUser.Username = "admin";
        adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin12parol12");
        await ctx.SaveChangesAsync();
    }

    // Fix legacy .svg logo paths
    var svgTeams = ctx.Teams.Where(t => t.LogoUrl != null && t.LogoUrl.EndsWith(".svg")).ToList();
    foreach (var t in svgTeams) t.LogoUrl = t.LogoUrl!.Replace(".svg", ".png");
    if (svgTeams.Any()) await ctx.SaveChangesAsync();

    // Sync heroes only if DB has fewer than 100 (missing most heroes)
    // Don't sync if we have enough - OpenDota returns wrong attrs for many heroes
    var heroCount = await ctx.Heroes.CountAsync();
    if (heroCount < 100)
    {
        try { await odota.SyncHeroesAsync(); }
        catch { /* offline - skip */ }
    }
}

app.Run();
