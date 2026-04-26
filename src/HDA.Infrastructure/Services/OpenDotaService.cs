using System.Text.Json;
using HDA.Domain.Entities;
using HDA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HDA.Infrastructure.Services;

public interface IOpenDotaService
{
    Task SyncHeroesAsync();
    Task<List<Hero>> GetHeroesAsync();
}

public class OpenDotaService : IOpenDotaService
{
    private readonly HttpClient _http;
    private readonly HdaDbContext _ctx;
    private const string BaseUrl = "https://api.opendota.com/api";

    public OpenDotaService(HttpClient http, HdaDbContext ctx)
    {
        _http = http;
        _ctx = ctx;
    }

    public async Task SyncHeroesAsync()
    {
        try
        {
            var response = await _http.GetStringAsync($"{BaseUrl}/heroes");
            var heroData = JsonSerializer.Deserialize<List<OpenDotaHero>>(response,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (heroData == null) return;

            foreach (var h in heroData)
            {
                var existing = await _ctx.Heroes.FirstOrDefaultAsync(x => x.OpenDotaId == h.Id);
                if (existing == null)
                {
                    _ctx.Heroes.Add(new Hero
                    {
                        OpenDotaId = h.Id,
                        Name = h.Name?.Replace("npc_dota_hero_", "") ?? "",
                        LocalizedName = h.LocalizedName ?? "",
                        // Use API attr but default to "str" if API returns generic "all"
                        PrimaryAttribute = (h.PrimaryAttr?.ToLowerInvariant() is "str" or "agi" or "int" or "all")
                            ? h.PrimaryAttr?.ToLowerInvariant() : "str",
                        AttackType = h.AttackType,
                        Roles = h.Roles,
                        ImageUrl = $"https://cdn.cloudflare.steamstatic.com/apps/dota2/images/dota_react/heroes/{h.Name?.Replace("npc_dota_hero_", "")}.png"
                    });
                }
                else
                {
                    existing.LocalizedName = h.LocalizedName ?? existing.LocalizedName;
                    // Only update attr if API returns specific value (not "all" which is often wrong)
                    var apiAttr = h.PrimaryAttr?.ToLowerInvariant();
                    if (!string.IsNullOrEmpty(apiAttr) && apiAttr != "all")
                        existing.PrimaryAttribute = apiAttr;
                    existing.AttackType = h.AttackType ?? existing.AttackType;
                    existing.Roles      = h.Roles ?? existing.Roles;
                    existing.ImageUrl   = $"https://cdn.cloudflare.steamstatic.com/apps/dota2/images/dota_react/heroes/{h.Name?.Replace("npc_dota_hero_", "")}.png";
                }
            }
            await _ctx.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log and continue — not critical
            Console.WriteLine($"[OpenDota] Hero sync failed: {ex.Message}");
        }
    }

    public Task<List<Hero>> GetHeroesAsync() => _ctx.Heroes.OrderBy(h => h.LocalizedName).ToListAsync();

    private record OpenDotaHero(
        int Id,
        string? Name,
        string? LocalizedName,
        string? PrimaryAttr,
        string? AttackType,
        string[]? Roles
    );
}
