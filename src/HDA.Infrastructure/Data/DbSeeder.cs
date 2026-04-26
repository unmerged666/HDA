using HDA.Domain.Entities;
using HDA.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HDA.Infrastructure.Data;

public static class DbSeeder
{
    private static DateTime Utc(int y,int mo,int d,int h=0,int mi=0,int s=0)
        => new(y,mo,d,h,mi,s,DateTimeKind.Utc);


    private static string GetHeroImageUrl(string name)
    {
        // Steam CDN uses specific naming - map internal names to CDN names
        var map = new Dictionary<string,string>
        {
            ["anti_mage"]="antimage",
            ["crystal_maiden"]="crystalmaiden",
            ["shadow_fiend"]="nevermore",
            ["doom_bringer"]="doom_bringer",
            ["spirit_breaker"]="spirit_breaker",
            ["skeleton_wing"]="skeleton_king",
            ["queenofpain"]="queenofpain",
            ["abyssal_underlord"]="abyssal_underlord",
            ["vengefulspirit"]="vengefulspirit",
            ["obsidian_destroyer"]="obsidian_destroyer",
            ["necrolyte"]="necrolyte",
            ["lifestealer"]="life_stealer",
            ["zuus"]="zuus",
            ["skeleton_king"]="skeleton_king",
            ["faceless_void"]="faceless_void",
            ["void_spirit"]="void_spirit",
            ["storm_spirit"]="storm_spirit",
            ["earth_spirit"]="earth_spirit",
            ["ember_spirit"]="ember_spirit",
            ["sand_king"]="sand_king",
            ["witch_doctor"]="witch_doctor",
            ["drow_ranger"]="drow_ranger",
            ["windrunner"]="windrunner",
            ["magnataur"]="magnataur",
            ["wisp"]="wisp",
            ["treant"]="treant",
            ["furion"]="furion",
            ["nyx_assassin"]="nyx_assassin",
            ["rattletrap"]="rattletrap",
            ["marci"]="marci",
            ["ringmaster"]="ringmaster",
            ["lone_druid"]="lone_druid",
            ["phantom_assassin"]="phantom_assassin",
            ["phantom_lancer"]="phantom_lancer",
            ["dark_willow"]="dark_willow",
            ["dark_seer"]="dark_seer",
            ["shadow_demon"]="shadow_demon",
            ["shadow_shaman"]="shadow_shaman",
            ["winter_wyvern"]="winter_wyvern",
            ["keeper_of_the_light"]="keeper_of_the_light",
            ["skywrath_mage"]="skywrath_mage",
            ["monkey_king"]="monkey_king",
            ["bounty_hunter"]="bounty_hunter",
            ["troll_warlord"]="troll_warlord",
            ["night_stalker"]="night_stalker",
            ["dragon_knight"]="dragon_knight",
            ["chaos_knight"]="chaos_knight",
            ["legion_commander"]="legion_commander",
            ["templar_assassin"]="templar_assassin",
            ["arc_warden"]="arc_warden",
            ["ancient_apparition"]="ancient_apparition",
            ["death_prophet"]="death_prophet",
            ["naga_siren"]="naga_siren",
            ["primal_beast"]="primal_beast",
            ["veno"]="venomancer",
            ["shredder"]="shredder",
        };
                var cdnName = map.ContainsKey(name) ? map[name] : name;
        return $"https://cdn.cloudflare.steamstatic.com/apps/dota2/images/dota_react/heroes/{cdnName}.png";
    }

    public static async Task SeedAsync(HdaDbContext ctx)
    {
        // EnsureCreated was just called by Program.cs on this same ctx instance
        // Tables definitely exist now. Check if we need to seed.
        bool hasUsers = await ctx.Users.AnyAsync();
        // Detect stale data: wrong hero attrs OR old team structure
        var heroCount       = hasUsers ? await ctx.Heroes.CountAsync() : 0;
        var hasNoRingmaster = hasUsers && !await ctx.Heroes.AnyAsync(h => h.Name == "ringmaster");
        var hasNoLargo      = hasUsers && !await ctx.Heroes.AnyAsync(h => h.Name == "largo");
        var wrongSpectre    = hasUsers && await ctx.Heroes.AnyAsync(h => h.Name == "spectre" && h.PrimaryAttribute != "agi");
        bool hasWrongHeroes = hasUsers && (heroCount < 120 || hasNoRingmaster || hasNoLargo || wrongSpectre);
        bool hasOldData = hasUsers && (hasWrongHeroes ||
            await ctx.Teams.AnyAsync(t => t.Name == "OG" && t.WorldRank == 1 && t.RatingPoints == 4200));

        if (hasOldData)
        {
            // Wipe all data and re-seed
            try { ctx.ActivityLogs.RemoveRange(ctx.ActivityLogs); await ctx.SaveChangesAsync(); } catch { }
            try { ctx.NewsArticles.RemoveRange(ctx.NewsArticles); await ctx.SaveChangesAsync(); } catch { }
            try { ctx.MatchPlayerStats.RemoveRange(ctx.MatchPlayerStats); await ctx.SaveChangesAsync(); } catch { }
            try { ctx.GameMapDrafts.RemoveRange(ctx.GameMapDrafts); await ctx.SaveChangesAsync(); } catch { }
            try { ctx.GameMaps.RemoveRange(ctx.GameMaps); await ctx.SaveChangesAsync(); } catch { }
            try { ctx.Matches.RemoveRange(ctx.Matches); await ctx.SaveChangesAsync(); } catch { }
            try { ctx.TournamentParticipants.RemoveRange(ctx.TournamentParticipants); await ctx.SaveChangesAsync(); } catch { }
            try { ctx.TournamentStages.RemoveRange(ctx.TournamentStages); await ctx.SaveChangesAsync(); } catch { }
            try { ctx.Tournaments.RemoveRange(ctx.Tournaments); await ctx.SaveChangesAsync(); } catch { }
            try { ctx.TeamHeroStats.RemoveRange(ctx.TeamHeroStats); await ctx.SaveChangesAsync(); } catch { }
            try { ctx.PlayerHeroStats.RemoveRange(ctx.PlayerHeroStats); await ctx.SaveChangesAsync(); } catch { }
            try { ctx.ProPlayers.RemoveRange(ctx.ProPlayers); await ctx.SaveChangesAsync(); } catch { }
            try { ctx.Teams.RemoveRange(ctx.Teams); await ctx.SaveChangesAsync(); } catch { }
            try { ctx.Heroes.RemoveRange(ctx.Heroes); await ctx.SaveChangesAsync(); } catch { }
            try { ctx.Users.RemoveRange(ctx.Users); await ctx.SaveChangesAsync(); } catch { }
        }
        else if (hasUsers) return;
        var now = DateTime.UtcNow;

        var admin = new User { Username="admin",  Email="admin12@example.com", PasswordHash=BCrypt.Net.BCrypt.HashPassword("admin12parol12"), Role=UserRole.Admin,   IsActive=true, CreatedAt=now };
        var fan   = new User { Username="fanboy", Email="fan@hda.gg",   PasswordHash=BCrypt.Net.BCrypt.HashPassword("Fan123!"),   Role=UserRole.Regular, IsActive=true, CreatedAt=now };
        var user2 = new User { Username="dotafan",Email="dota@hda.gg",  PasswordHash=BCrypt.Net.BCrypt.HashPassword("Dota123!"),  Role=UserRole.Regular, IsActive=true, CreatedAt=now };
        var user3 = new User { Username="analyst",Email="analyst@hda.gg",PasswordHash=BCrypt.Net.BCrypt.HashPassword("Ana123!"),  Role=UserRole.Regular, IsActive=true, CreatedAt=now };
        ctx.Users.AddRange(admin, fan, user2, user3);

        // Heroes - полный список Dota 2 патч 7.40c с верными атрибутами
        var heroData = new (string n, string ln, string attr, string atk, string[] roles, int id)[]
        {
            // ── СИЛА (35 героев) ─────────────────────────────────────────
            ("axe","Axe","str","Melee",["Initiator","Durable","Disabler"],2),
            ("pudge","Pudge","str","Melee",["Support","Durable","Disabler","Initiator"],14),
            ("earthshaker","Earthshaker","str","Melee",["Support","Initiator","Disabler","Nuker"],7),
            ("tidehunter","Tidehunter","str","Melee",["Initiator","Durable","Disabler","Support"],29),
            ("primal_beast","Primal Beast","str","Melee",["Initiator","Disabler","Durable","Nuker"],137),
            ("shredder","Timbersaw","str","Melee",["Carry","Initiator","Nuker","Durable"],75),
            ("dragon_knight","Dragon Knight","str","Melee",["Carry","Durable","Disabler","Initiator","Pusher","Nuker"],49),
            ("omniknight","Omniknight","str","Melee",["Support","Durable"],57),
            ("night_stalker","Night Stalker","str","Melee",["Carry","Initiator","Durable","Disabler"],60),
            ("doom_bringer","Doom","str","Melee",["Carry","Jungler","Durable","Disabler","Nuker"],69),
            ("spirit_breaker","Spirit Breaker","str","Melee",["Initiator","Carry","Disabler","Escape","Durable","Nuker"],71),
            ("alchemist","Alchemist","str","Melee",["Carry","Durable","Disabler","Nuker","Jungler"],73),
            ("lycan","Lycan","str","Melee",["Carry","Pusher","Jungler"],77),
            ("elder_titan","Elder Titan","str","Melee",["Support","Initiator","Disabler","Nuker"],103),
            ("centaur","Centaur Warrunner","str","Melee",["Durable","Initiator","Disabler"],96),
            ("kunkka","Kunkka","str","Melee",["Carry","Support","Disabler","Nuker","Initiator","Durable"],23),
            ("slardar","Slardar","str","Melee",["Carry","Initiator","Disabler","Durable"],28),
            ("tiny","Tiny","str","Melee",["Carry","Initiator","Nuker","Pusher"],19),
            ("skeleton_king","Wraith King","str","Melee",["Carry","Durable","Disabler","Jungler"],42),
            ("rattletrap","Clockwerk","str","Melee",["Initiator","Carry","Durable","Disabler","Nuker"],51),
            ("abyssal_underlord","Underlord","str","Melee",["Support","Durable","Pusher","Disabler","Initiator","Nuker"],108),
            ("treant","Treant Protector","str","Melee",["Support","Pusher","Durable","Disabler","Initiator","Jungler"],83),
            ("mars","Mars","str","Melee",["Carry","Disabler","Initiator","Durable","Nuker"],129),
            ("dawnbreaker","Dawnbreaker","str","Melee",["Carry","Support","Durable","Initiator","Nuker"],135),
            ("ogre_magi","Ogre Magi","str","Melee",["Support","Durable","Nuker","Disabler"],84),
            ("huskar","Huskar","str","Ranged",["Carry","Durable","Nuker","Initiator"],59),
            ("sven","Sven","str","Melee",["Carry","Initiator","Disabler","Nuker","Durable"],18),
            ("chaos_knight","Chaos Knight","str","Melee",["Carry","Initiator","Durable","Disabler","Escape","Nuker"],81),
            ("tusk","Tusk","str","Melee",["Support","Initiator","Disabler","Carry","Durable","Nuker"],100),
            ("bristleback","Bristleback","str","Melee",["Carry","Initiator","Durable","Nuker"],99),
            ("undying","Undying","str","Melee",["Support","Durable","Initiator","Nuker","Disabler"],85),
            ("phoenix","Phoenix","str","Ranged",["Support","Initiator","Disabler","Nuker","Durable","Escape"],110),
            ("legion_commander","Legion Commander","str","Melee",["Carry","Initiator","Durable","Disabler","Jungler","Nuker"],104),
            ("wisp","Io","str","Ranged",["Support","Escape","Jungler"],91),
            ("lifestealer","Lifestealer","str","Melee",["Carry","Jungler","Durable","Disabler"],54),
            // ── ЛОВКОСТЬ (34 героя) ───────────────────────────────────────
            ("anti_mage","Anti-Mage","agi","Melee",["Carry","Escape"],1),
            ("shadow_fiend","Shadow Fiend","agi","Ranged",["Carry","Nuker"],11),
            ("juggernaut","Juggernaut","agi","Melee",["Carry","Jungler","Pusher"],8),
            ("mirana","Mirana","agi","Ranged",["Carry","Support","Escape","Nuker","Disabler"],9),
            ("morphling","Morphling","agi","Ranged",["Carry","Escape","Nuker"],10),
            ("drow_ranger","Drow Ranger","agi","Ranged",["Carry","Disabler","Pusher","Nuker"],6),
            ("faceless_void","Faceless Void","agi","Melee",["Carry","Disabler","Initiator"],41),
            ("phantom_assassin","Phantom Assassin","agi","Melee",["Carry","Escape","Disabler"],44),
            ("viper","Viper","agi","Ranged",["Carry","Nuker","Disabler","Durable"],47),
            ("razor","Razor","agi","Ranged",["Carry","Durable","Nuker"],15),
            ("gyrocopter","Gyrocopter","agi","Ranged",["Carry","Nuker","Initiator"],72),
            ("luna","Luna","agi","Ranged",["Carry","Pusher","Nuker"],48),
            ("ursa","Ursa","agi","Melee",["Carry","Jungler","Initiator","Durable"],70),
            ("clinkz","Clinkz","agi","Ranged",["Carry","Escape","Pusher","Nuker"],56),
            ("bounty_hunter","Bounty Hunter","agi","Melee",["Carry","Escape","Support","Disabler","Nuker","Initiator"],62),
            ("weaver","Weaver","agi","Ranged",["Carry","Escape","Nuker"],63),
            ("phantom_lancer","Phantom Lancer","agi","Melee",["Carry","Escape","Nuker","Disabler","Pusher"],12),
            ("naga_siren","Naga Siren","agi","Melee",["Carry","Support","Escape","Pusher","Disabler"],89),
            ("slark","Slark","agi","Melee",["Carry","Escape","Disabler","Nuker"],93),
            ("medusa","Medusa","agi","Ranged",["Carry","Pusher","Durable","Nuker"],94),
            ("troll_warlord","Troll Warlord","agi","Melee",["Carry","Pusher","Disabler","Jungler"],95),
            ("monkey_king","Monkey King","agi","Melee",["Carry","Escape","Initiator","Pusher","Nuker"],114),
            ("terrorblade","Terrorblade","agi","Ranged",["Carry","Pusher","Escape","Nuker"],109),
            ("bloodseeker","Bloodseeker","agi","Melee",["Carry","Jungler","Initiator","Nuker","Disabler"],4),
            ("riki","Riki","agi","Melee",["Carry","Escape","Nuker","Disabler","Support"],32),
            ("broodmother","Broodmother","agi","Melee",["Carry","Jungler","Pusher","Escape","Durable","Nuker"],61),
            ("meepo","Meepo","agi","Melee",["Carry","Jungler","Escape","Nuker","Pusher","Durable"],82),
            ("templar_assassin","Templar Assassin","agi","Melee",["Carry","Initiator","Escape","Nuker","Disabler","Jungler"],46),
            ("lone_druid","Lone Druid","agi","Ranged",["Carry","Pusher","Jungler","Durable","Nuker"],80),
            ("sniper","Sniper","agi","Ranged",["Carry","Nuker","Disabler"],35),
            ("vengefulspirit","Vengeful Spirit","agi","Ranged",["Support","Carry","Disabler","Nuker","Initiator"],20),
            ("hoodwink","Hoodwink","agi","Ranged",["Support","Carry","Disabler","Nuker","Escape","Initiator"],123),
            ("spectre","Spectre","agi","Melee",["Carry","Escape","Durable","Disabler"],67),
            // ── ИНТЕЛЛЕКТ (34 героя) ──────────────────────────────────────
            ("crystal_maiden","Crystal Maiden","int","Ranged",["Support","Disabler","Nuker"],5),
            ("invoker","Invoker","int","Ranged",["Carry","Nuker","Disabler","Escape"],74),
            ("witch_doctor","Witch Doctor","int","Ranged",["Support","Nuker","Disabler"],30),
            ("lich","Lich","int","Ranged",["Support","Nuker","Disabler"],31),
            ("lion","Lion","int","Ranged",["Support","Disabler","Nuker"],26),
            ("storm_spirit","Storm Spirit","int","Ranged",["Carry","Escape","Nuker","Initiator"],17),
            ("lina","Lina","int","Ranged",["Carry","Support","Nuker"],25),
            ("zuus","Zeus","int","Ranged",["Nuker"],22),
            ("puck","Puck","int","Ranged",["Initiator","Escape","Nuker","Disabler"],13),
            ("necrolyte","Necrophos","int","Ranged",["Carry","Nuker","Jungler","Durable"],36),
            ("warlock","Warlock","int","Ranged",["Support","Nuker","Initiator","Disabler"],37),
            ("shadow_shaman","Shadow Shaman","int","Ranged",["Support","Pusher","Disabler","Nuker"],27),
            ("silencer","Silencer","int","Ranged",["Support","Carry","Disabler","Nuker","Pusher","Durable","Initiator"],39),
            ("obsidian_destroyer","Outworld Destroyer","int","Ranged",["Carry","Nuker","Disabler","Escape","Support"],76),
            ("ancient_apparition","Ancient Apparition","int","Ranged",["Support","Nuker","Disabler","Initiator"],68),
            ("tinker","Tinker","int","Ranged",["Carry","Nuker","Pusher","Escape","Disabler"],34),
            ("rubick","Rubick","int","Ranged",["Support","Nuker","Disabler","Initiator"],86),
            ("disruptor","Disruptor","int","Ranged",["Support","Nuker","Disabler","Initiator"],87),
            ("keeper_of_the_light","Keeper of the Light","int","Ranged",["Support","Nuker","Pusher"],90),
            ("skywrath_mage","Skywrath Mage","int","Ranged",["Support","Carry","Nuker","Disabler"],101),
            ("pugna","Pugna","int","Ranged",["Support","Pusher","Nuker","Disabler"],45),
            ("enchantress","Enchantress","int","Ranged",["Support","Carry","Jungler","Pusher","Nuker","Durable"],58),
            ("jakiro","Jakiro","int","Ranged",["Support","Disabler","Nuker","Pusher"],64),
            ("leshrac","Leshrac","int","Ranged",["Carry","Support","Pusher","Nuker","Disabler"],52),
            ("chen","Chen","int","Ranged",["Support","Pusher","Jungler","Initiator","Disabler","Nuker"],66),
            ("oracle","Oracle","int","Ranged",["Support","Disabler","Nuker","Escape","Initiator"],111),
            ("grimstroke","Grimstroke","int","Ranged",["Support","Nuker","Disabler","Initiator"],121),
            ("dark_willow","Dark Willow","int","Ranged",["Support","Nuker","Disabler","Escape","Initiator"],119),
            ("dark_seer","Dark Seer","int","Melee",["Support","Initiator","Jungler","Carry","Pusher","Durable","Nuker","Disabler"],55),
            ("shadow_demon","Shadow Demon","int","Ranged",["Support","Disabler","Nuker","Pusher","Initiator"],79),
            ("winter_wyvern","Winter Wyvern","int","Ranged",["Support","Disabler","Nuker","Durable"],112),
            ("queenofpain","Queen of Pain","int","Ranged",["Carry","Nuker","Escape","Disabler","Initiator"],39),
            ("ringmaster","Ringmaster","int","Ranged",["Support","Initiator","Disabler","Nuker","Durable"],139),
            ("muerta","Muerta","int","Ranged",["Carry","Support","Nuker","Disabler"],136),
            // ── УНИВЕРСАЛ (24 героя) ──────────────────────────────────────
            ("void_spirit","Void Spirit","all","Melee",["Carry","Escape","Nuker","Disabler"],126),
            ("earth_spirit","Earth Spirit","all","Melee",["Initiator","Disabler","Nuker","Escape","Durable","Support"],107),
            ("abaddon","Abaddon","all","Melee",["Support","Carry","Durable"],102),
            ("bane","Bane","all","Ranged",["Support","Disabler","Nuker","Initiator"],3),
            ("batrider","Batrider","all","Ranged",["Initiator","Carry","Disabler","Nuker","Escape","Support","Durable"],65),
            ("magnataur","Magnus","all","Melee",["Initiator","Disabler","Nuker","Carry","Support","Durable"],97),
            ("furion","Nature's Prophet","all","Ranged",["Carry","Pusher","Jungler","Nuker","Escape","Initiator","Disabler"],53),
            ("nyx_assassin","Nyx Assassin","all","Melee",["Support","Initiator","Disabler","Nuker","Escape"],88),
            ("pangolier","Pangolier","all","Melee",["Carry","Disabler","Initiator","Escape","Nuker","Durable"],120),
            ("sand_king","Sand King","all","Melee",["Initiator","Disabler","Nuker","Support"],16),
            ("snapfire","Snapfire","all","Ranged",["Support","Nuker","Disabler","Initiator"],128),
            ("techies","Techies","all","Ranged",["Support","Nuker","Disabler"],105),
            ("windrunner","Windranger","all","Ranged",["Carry","Support","Disabler","Nuker"],21),
            ("enigma","Enigma","all","Ranged",["Initiator","Jungler","Pusher","Disabler","Nuker"],33),
            ("dazzle","Dazzle","all","Ranged",["Support","Carry","Nuker","Disabler"],50),
            ("death_prophet","Death Prophet","all","Ranged",["Carry","Pusher","Nuker"],43),
            ("brewmaster","Brewmaster","all","Melee",["Initiator","Carry","Disabler","Nuker","Jungler","Durable"],78),
            ("beastmaster","Beastmaster","all","Melee",["Initiator","Jungler","Disabler","Pusher","Durable","Nuker"],38),
            ("visage","Visage","all","Ranged",["Support","Carry","Durable","Pusher","Initiator","Nuker"],92),
            ("veno","Venomancer","all","Ranged",["Support","Pusher","Nuker","Initiator","Disabler"],40),
            ("arc_warden","Arc Warden","all","Ranged",["Carry","Pusher","Jungler","Escape","Nuker"],113),
            ("marci","Marci","all","Melee",["Support","Carry","Initiator","Disabler","Escape","Durable","Nuker"],131),
            ("largo","Largo","str","Melee",["Support","Disabler","Durable"],145),
            ("ember_spirit","Ember Spirit","agi","Melee",["Carry","Escape","Nuker","Disabler","Initiator"],106),
        };
        ctx.Heroes.AddRange(heroData.Select(h => new Hero
        {
            Name=h.n, LocalizedName=h.ln, PrimaryAttribute=h.attr, AttackType=h.atk, Roles=h.roles, OpenDotaId=h.id,
            ImageUrl=GetHeroImageUrl(h.n)
        }));

        // ── РЕАЛЬНЫЕ КОМАНДЫ (данные на март 2026) ───────────────────────────
        // Falcons: TI 2025 чемпионы. Состав: skiter, Malr1ne, ATF, Cr1t-, Sneyking
        var falcons = new Team { Name="Team Falcons",   Tag="FLCN",   Region="MENA",         Country="Saudi Arabia",   Founded=Utc(2023,11,17), RatingPoints=4850, WorldRank=1, LogoUrl="/images/teams/falcons.png", CreatedAt=now, UpdatedAt=now, Organization="Falcons Esports" };
        // Tundra: TI 2022 чемпионы, 4x BLAST Slam. Состав: 33, Pure, bzm, Whitemon, Ari
        var tundra  = new Team { Name="Tundra Esports", Tag="Tundra", Region="Europe",        Country="United Kingdom", Founded=Utc(2020,1,1),   RatingPoints=4600, WorldRank=2, LogoUrl="/images/teams/tundra.png",  CreatedAt=now, UpdatedAt=now, Organization="Tundra Esports" };
        // Team Spirit: TI 2021 чемпионы, EWC 2025 победители
        var spirit  = new Team { Name="Team Spirit",    Tag="Spirit", Region="CIS",           Country="Russia",         Founded=Utc(2015,1,1),   RatingPoints=4400, WorldRank=3, LogoUrl="/images/teams/spirit.png",  CreatedAt=now, UpdatedAt=now, Organization="Team Spirit" };
        // Liquid: PGL Wallachia x2, EWC top-8
        var liquid  = new Team { Name="Team Liquid",    Tag="TL",     Region="Europe",        Country="Netherlands",    Founded=Utc(2000,1,1),   RatingPoints=4200, WorldRank=4, LogoUrl="/images/teams/liquid.png",  CreatedAt=now, UpdatedAt=now, Organization="Team Liquid" };
        // OG: TI 2018, TI 2019 чемпионы
        var og      = new Team { Name="OG",             Tag="OG",     Region="Europe",        Country="International",  Founded=Utc(2015,8,12),  RatingPoints=4000, WorldRank=5, LogoUrl="/images/teams/og.png",      CreatedAt=now, UpdatedAt=now, Organization="OG Esports" };
        // Xtreme Gaming: TI 2025 финалисты
        var xg      = new Team { Name="Xtreme Gaming",  Tag="XG",     Region="China",         Country="China",          Founded=Utc(2022,1,1),   RatingPoints=3900, WorldRank=6, LogoUrl="/images/teams/xg.png",      CreatedAt=now, UpdatedAt=now, Organization="Xtreme Gaming" };
        // NAVI: постоянно топ-8 в 2025
        var navi    = new Team { Name="Natus Vincere",  Tag="NAVI",   Region="CIS",           Country="Ukraine",        Founded=Utc(2009,12,14), RatingPoints=3600, WorldRank=7, LogoUrl="/images/teams/navi.png",    CreatedAt=now, UpdatedAt=now, Organization="Natus Vincere" };
        // BetBoom: TI 2025 top-4
        var betboom = new Team { Name="BetBoom Team",   Tag="BB",     Region="CIS",           Country="Russia",         Founded=Utc(2021,1,1),   RatingPoints=3400, WorldRank=8, LogoUrl="/images/teams/betboom.png", CreatedAt=now, UpdatedAt=now, Organization="BetBoom" };
        // Heroic: TI 2025 top-4
        var heroic  = new Team { Name="Heroic",         Tag="Heroic", Region="Europe",        Country="Denmark",        Founded=Utc(2019,1,1),   RatingPoints=3300, WorldRank=9, LogoUrl="/images/teams/heroic.png",  CreatedAt=now, UpdatedAt=now, Organization="Heroic Esports" };
        // Secret: опытнейшая организация
        var secret  = new Team { Name="Team Secret",    Tag="Secret", Region="Europe",        Country="International",  Founded=Utc(2014,9,1),   RatingPoints=3100, WorldRank=10, LogoUrl="/images/teams/secret.png", CreatedAt=now, UpdatedAt=now, Organization="Team Secret" };
        ctx.Teams.AddRange(falcons, tundra, spirit, liquid, og, xg, navi, betboom, heroic, secret);

        // ── РЕАЛЬНЫЕ ТУРНИРЫ ─────────────────────────────────────────────────
        var ti25   = new Tournament { Name="The International 2025", Organizer="Valve", Region="Global", PrizePool=2_881_791, StartDate=Utc(2025,9,4), EndDate=Utc(2025,9,14), Tier=TournamentTier.S, Status=TournamentStatus.Completed, Description="TI14 — 14-е издание The International, финал в Hamburg, Германия. Team Falcons победили Xtreme Gaming 3-2 в гранд-финале, завоевав Aegis of Champions.", LogoUrl="/images/tournaments/ti2024.png", Location="Hamburg, Germany", CreatedAt=now, UpdatedAt=now };
        var blast5  = new Tournament { Name="BLAST Slam V",          Organizer="BLAST", Region="Global", PrizePool=1_000_000, StartDate=Utc(2025,11,27), EndDate=Utc(2025,12,7), Tier=TournamentTier.A, Status=TournamentStatus.Completed, Description="Tundra Esports выиграли BLAST Slam V, победив в финале со счётом 3-1. Это их 4-й подряд титул BLAST Slam.", Location="Online", CreatedAt=now, UpdatedAt=now };
        var dl27    = new Tournament { Name="DreamLeague Season 27",  Organizer="ESL/DreamHack", Region="Global", PrizePool=1_000_000, StartDate=Utc(2025,12,9), EndDate=Utc(2025,12,21), Tier=TournamentTier.A, Status=TournamentStatus.Completed, Description="Tundra Esports выиграли DreamLeague Season 27, закрепив за собой позицию №1 в мировом рейтинге.", Location="Online", CreatedAt=now, UpdatedAt=now };
        var blast6  = new Tournament { Name="BLAST Slam VI",          Organizer="BLAST", Region="Global", PrizePool=1_000_000, StartDate=Utc(2026,2,19), EndDate=Utc(2026,3,2), Tier=TournamentTier.A, Status=TournamentStatus.Completed, Description="Team Liquid победила на BLAST Slam VI, нарушив доминирование Tundra в серии BLAST Slam.", Location="Online", CreatedAt=now, UpdatedAt=now };
        var pglw7   = new Tournament { Name="PGL Wallachia Season 7", Organizer="PGL", Region="Global", PrizePool=1_000_000, StartDate=Utc(2026,3,10), EndDate=Utc(2026,3,23), Tier=TournamentTier.A, Status=TournamentStatus.Ongoing, Description="Текущий турнир сезона. Проходит онлайн с участием топ-16 команд мира.", Location="Online", CreatedAt=now, UpdatedAt=now };
        var esl_bir = new Tournament { Name="ESL One Birmingham 2026", Organizer="ESL", Region="Europe", PrizePool=500_000, StartDate=Utc(2026,5,20), EndDate=Utc(2026,5,25), Tier=TournamentTier.A, Status=TournamentStatus.Upcoming, Description="ESL возвращается в Бирмингем. Live LAN турнир с топ-командами мира.", Location="Birmingham, UK", CreatedAt=now, UpdatedAt=now };
        ctx.Tournaments.AddRange(ti25, blast5, dl27, blast6, pglw7, esl_bir);
        await ctx.SaveChangesAsync();

        // Participants TI 2025 (реальные результаты)
        ctx.TournamentParticipants.AddRange(
            new TournamentParticipant { TournamentId=ti25.Id, TeamId=falcons.Id, FinalPlacement=1, PrizeWon=1_224_761 },
            new TournamentParticipant { TournamentId=ti25.Id, TeamId=xg.Id,      FinalPlacement=2, PrizeWon=374_333 },
            new TournamentParticipant { TournamentId=ti25.Id, TeamId=spirit.Id,  FinalPlacement=3, PrizeWon=259_361 },
            new TournamentParticipant { TournamentId=ti25.Id, TeamId=betboom.Id, FinalPlacement=4, PrizeWon=172_907 },
            new TournamentParticipant { TournamentId=ti25.Id, TeamId=heroic.Id,  FinalPlacement=5, PrizeWon=115_272 },
            new TournamentParticipant { TournamentId=ti25.Id, TeamId=tundra.Id,  FinalPlacement=7, PrizeWon=86_454 },
            new TournamentParticipant { TournamentId=ti25.Id, TeamId=liquid.Id,  FinalPlacement=9, PrizeWon=57_636 },
            new TournamentParticipant { TournamentId=ti25.Id, TeamId=navi.Id,    FinalPlacement=13, PrizeWon=28_818 }
        );
        ctx.TournamentParticipants.AddRange(
            new TournamentParticipant { TournamentId=blast5.Id, TeamId=tundra.Id,  FinalPlacement=1, PrizeWon=400_000 },
            new TournamentParticipant { TournamentId=blast5.Id, TeamId=falcons.Id, FinalPlacement=3, PrizeWon=100_000 },
            new TournamentParticipant { TournamentId=blast6.Id, TeamId=liquid.Id,  FinalPlacement=1, PrizeWon=400_000 },
            new TournamentParticipant { TournamentId=blast6.Id, TeamId=tundra.Id,  FinalPlacement=2, PrizeWon=200_000 }
        );

        // Stages
        var tiGF  = new TournamentStage { TournamentId=ti25.Id,  Name="Grand Final",   Type=StageType.GrandFinals, Order=3, StartDate=Utc(2025,9,14), EndDate=Utc(2025,9,14) };
        var tiPO  = new TournamentStage { TournamentId=ti25.Id,  Name="Playoffs",      Type=StageType.Playoffs,    Order=2, StartDate=Utc(2025,9,11), EndDate=Utc(2025,9,13) };
        var tiGS  = new TournamentStage { TournamentId=ti25.Id,  Name="Group Stage",   Type=StageType.GroupStage,  Order=1, StartDate=Utc(2025,9,4),  EndDate=Utc(2025,9,7)  };
        var b5GF  = new TournamentStage { TournamentId=blast5.Id, Name="Grand Final",  Type=StageType.GrandFinals, Order=2, StartDate=Utc(2025,12,7), EndDate=Utc(2025,12,7) };
        var pglGS = new TournamentStage { TournamentId=pglw7.Id, Name="Group Stage",   Type=StageType.GroupStage,  Order=1, StartDate=Utc(2026,3,10), EndDate=Utc(2026,3,17) };
        ctx.TournamentStages.AddRange(tiGF, tiPO, tiGS, b5GF, pglGS);
        await ctx.SaveChangesAsync();

        // ── РЕАЛЬНЫЕ МАТЧИ ───────────────────────────────────────────────────
        var matches = new[]
        {
            // TI 2025 Grand Final: Falcons 3-2 Xtreme Gaming
            new Match { TournamentId=ti25.Id, StageId=tiGF.Id, TeamAId=falcons.Id, TeamBId=xg.Id,      Format=MatchFormat.Bo5, Status=MatchStatus.Completed, ScheduledAt=Utc(2025,9,14,14,0), StartedAt=Utc(2025,9,14,14,10), FinishedAt=Utc(2025,9,14,21,0), TeamAScore=3, TeamBScore=2, WinnerId=falcons.Id, TwitchUrl="https://twitch.tv/dota2ti", YoutubeUrl="https://youtube.com/dota2", CreatedAt=now, UpdatedAt=now },
            // TI 2025 UB Final
            new Match { TournamentId=ti25.Id, StageId=tiPO.Id, TeamAId=falcons.Id, TeamBId=spirit.Id,  Format=MatchFormat.Bo3, Status=MatchStatus.Completed, ScheduledAt=Utc(2025,9,12,12,0), StartedAt=Utc(2025,9,12,12,5), FinishedAt=Utc(2025,9,12,15,0), TeamAScore=2, TeamBScore=0, WinnerId=falcons.Id, CreatedAt=now, UpdatedAt=now },
            new Match { TournamentId=ti25.Id, StageId=tiPO.Id, TeamAId=xg.Id,      TeamBId=betboom.Id, Format=MatchFormat.Bo3, Status=MatchStatus.Completed, ScheduledAt=Utc(2025,9,13,10,0), StartedAt=Utc(2025,9,13,10,5), FinishedAt=Utc(2025,9,13,13,0), TeamAScore=2, TeamBScore=1, WinnerId=xg.Id,      CreatedAt=now, UpdatedAt=now },
            // BLAST Slam V Final: Tundra 3-? Falcons
            new Match { TournamentId=blast5.Id, StageId=b5GF.Id, TeamAId=tundra.Id, TeamBId=falcons.Id, Format=MatchFormat.Bo5, Status=MatchStatus.Completed, ScheduledAt=Utc(2025,12,7,15,0), StartedAt=Utc(2025,12,7,15,5), FinishedAt=Utc(2025,12,7,20,0), TeamAScore=3, TeamBScore=1, WinnerId=tundra.Id, CreatedAt=now, UpdatedAt=now },
            // PGL Wallachia S7 - Ongoing matches
            new Match { TournamentId=pglw7.Id, StageId=pglGS.Id, TeamAId=falcons.Id, TeamBId=tundra.Id, Format=MatchFormat.Bo3, Status=MatchStatus.Scheduled, ScheduledAt=Utc(2026,3,25,14,0), CreatedAt=now, UpdatedAt=now },
            new Match { TournamentId=pglw7.Id, StageId=pglGS.Id, TeamAId=spirit.Id,  TeamBId=liquid.Id, Format=MatchFormat.Bo3, Status=MatchStatus.Scheduled, ScheduledAt=Utc(2026,3,25,17,0), CreatedAt=now, UpdatedAt=now },
            new Match { TournamentId=pglw7.Id, StageId=pglGS.Id, TeamAId=og.Id,      TeamBId=navi.Id,   Format=MatchFormat.Bo3, Status=MatchStatus.Scheduled, ScheduledAt=Utc(2026,3,26,14,0), CreatedAt=now, UpdatedAt=now },
            new Match { TournamentId=pglw7.Id, StageId=pglGS.Id, TeamAId=xg.Id,      TeamBId=heroic.Id, Format=MatchFormat.Bo3, Status=MatchStatus.Scheduled, ScheduledAt=Utc(2026,3,26,17,0), CreatedAt=now, UpdatedAt=now },
            new Match { TournamentId=pglw7.Id, StageId=pglGS.Id, TeamAId=betboom.Id, TeamBId=secret.Id, Format=MatchFormat.Bo3, Status=MatchStatus.Scheduled, ScheduledAt=Utc(2026,3,27,14,0), CreatedAt=now, UpdatedAt=now },
        };
        ctx.Matches.AddRange(matches);
        await ctx.SaveChangesAsync();

        // ── РЕАЛЬНЫЕ ИГРОКИ ──────────────────────────────────────────────────
        ctx.ProPlayers.AddRange(
            // ATF (Falcons) - лучший офлейнер 2025, TI2025 MVP
            new ProPlayer { UserId=admin.Id, Nickname="ATF",      RealName="Ammar Al-Assaf",    Country="JO", Role=PlayerRole.Offlaner,    TeamId=falcons.Id, Status=ProPlayerStatus.Approved, TotalMatches=346, Wins=222, AvgKills=6.8, AvgDeaths=3.2, AvgAssists=9.1, AvgGpm=520, AvgXpm=570, CreatedAt=now, UpdatedAt=now, Bio="TI2025 champion and best offlaner of 2025. Known for Mars and Timbersaw. Captain of Team Falcons. 23-year-old Jordanian player." },
            // Malr1ne (Falcons) - лучший мидлейнер 2025
            new ProPlayer { UserId=fan.Id,   Nickname="Malr1ne",  RealName="Stanislav Potorak",  Country="RU", Role=PlayerRole.Midlaner,    TeamId=falcons.Id, Status=ProPlayerStatus.Approved, TotalMatches=346, Wins=222, AvgKills=7.2, AvgDeaths=2.8, AvgAssists=7.4, AvgGpm=580, AvgXpm=620, CreatedAt=now, UpdatedAt=now, Bio="Best midlaner of 2025. Renowned for Timbersaw, Sand King, and Primal Beast. Known for dominant laning and excellent teamfight positioning." },
            // 33 (Tundra) - лучший капитан 2025, 4x BLAST Slam
            new ProPlayer { UserId=user2.Id, Nickname="33",       RealName="Neta Shapira",      Country="IL", Role=PlayerRole.Offlaner,    TeamId=tundra.Id,  Status=ProPlayerStatus.Approved, TotalMatches=578, Wins=340, AvgKills=4.8, AvgDeaths=3.5, AvgAssists=8.9, AvgGpm=490, AvgXpm=530, CreatedAt=now, UpdatedAt=now, Bio="TI2022 champion. Captain of Tundra Esports. Named best offlaner multiple years running for exceptional micro skills and unconventional drafts." },
            // Yatoro (Spirit) - TI2021 MVP, EWC 2025 чемпион
            new ProPlayer { UserId=user3.Id, Nickname="Yatoro",   RealName="Ilya Mulyarchuk",   Country="UA", Role=PlayerRole.Carry,       TeamId=spirit.Id,  Status=ProPlayerStatus.Approved, TotalMatches=412, Wins=265, AvgKills=8.4, AvgDeaths=2.6, AvgAssists=6.2, AvgGpm=650, AvgXpm=680, CreatedAt=now, UpdatedAt=now, Bio="World-class carry since TI2021. Known for impeccable map awareness and highly efficient farming. Led Spirit to EWC 2025 title." }
        );

        // ── РЕАЛЬНЫЕ НОВОСТИ ─────────────────────────────────────────────────
        ctx.NewsArticles.AddRange(
            new NewsArticle { Title="Team Falcons — Чемпионы The International 2025", Slug="falcons-ti2025-champions", Summary="Team Falcons победили Xtreme Gaming 3:2 в захватывающем гранд-финале TI14 в Гамбурге.", Content="14 сентября 2025 года Team Falcons вошли в историю Dota 2, завоевав Aegis of Champions на The International 2025, прошедшем на арене Barclays Arena в Гамбурге. В захватывающем гранд-финале формата bo5 Falcons победили Xtreme Gaming со счётом 3:2. ATF, Malr1ne и Cr1t- были признаны ключевыми игроками турнира. Призовой фонд TI14 составил $2,881,791 — исторически один из самых скромных, но ни в коей мере не уменьшивший накал страстей. Малр1не после финала заявил: «Мы знали, что можем это сделать. Команда работала невероятно усердно весь год».", Category="Result", IsPublished=true, AuthorId=admin.Id, PublishedAt=Utc(2025,9,14,22,0), CreatedAt=Utc(2025,9,14), UpdatedAt=Utc(2025,9,14) },
            new NewsArticle { Title="Tundra Esports выиграли BLAST Slam V и DreamLeague S27", Slug="tundra-blast-slam-5-dl27", Summary="33 и компания завоевали очередные два крупных трофея, утвердив свой статус топ-команды конца 2025 года.", Content="Tundra Esports завершили 2025 год на невероятной волне: победы на BLAST Slam V и DreamLeague Season 27 сделали их самой титулованной командой второй половины сезона. Особенно впечатляет BLAST Slam — это уже четвёртая подряд победа Tundra в этой серии. Капитан команды 33 прокомментировал: «Мы сделали то, что должны были. Команда работала очень хорошо».", Category="Result", IsPublished=true, AuthorId=admin.Id, PublishedAt=Utc(2025,12,8,10,0), CreatedAt=Utc(2025,12,8), UpdatedAt=Utc(2025,12,8) },
            new NewsArticle { Title="Team Liquid победили на BLAST Slam VI", Slug="liquid-blast-slam-vi-2026", Summary="Team Liquid с новым составом (Ace, tOfu) одержали победу на BLAST Slam VI, прервав серию Tundra.", Content="В феврале 2026 года Team Liquid ворвались в новый сезон с победой на BLAST Slam VI. Обновлённый состав с бывшими игроками Marcus 'Ace' Hoelgaard и Erik 'tOfu' Engel показал класс, переиграв Tundra Esports в гранд-финале. Это доказало, что посттурнирный шаффл пошёл Liquid на пользу.", Category="Result", IsPublished=true, AuthorId=admin.Id, PublishedAt=Utc(2026,3,3,10,0), CreatedAt=Utc(2026,3,3), UpdatedAt=Utc(2026,3,3) },
            new NewsArticle { Title="PGL Wallachia Season 7 стартует 10 марта", Slug="pgl-wallachia-s7-start", Summary="Новый крупный турнир с призовым фондом $1,000,000 начинается на этой неделе. Следите за матчами на HDA.gg.", Content="PGL Wallachia Season 7 — один из самых ожидаемых турниров начала 2026 года — начался 10 марта. В нём участвуют 16 лучших команд мира, включая чемпионов TI Falcons, доминирующий Tundra, действующих победителей Liquid и многих других. Призовой фонд составляет $1,000,000.", Category="News", IsPublished=true, AuthorId=admin.Id, PublishedAt=Utc(2026,3,10,8,0), CreatedAt=Utc(2026,3,10), UpdatedAt=Utc(2026,3,10) },
            new NewsArticle { Title="Dota 2 Патч 7.38: Полная переработка нейтральных предметов", Slug="patch-738-neutral-items", Summary="Valve выпустила масштабный патч 7.38, кардинально изменив систему нейтральных предметов и внеся крупные изменения на карту.", Content="Патч 7.38 стал одним из самых значимых обновлений за последние годы. Полная переработка нейтральных предметов изменила подходы к фармингу и стратегию игры. Новая карта получила обновлённую Roshan pit и изменённые лесные лагеря. Профессиональные игроки активно осваивают новый мета.", Category="Patch", IsPublished=true, AuthorId=admin.Id, PublishedAt=Utc(2026,2,15,12,0), CreatedAt=Utc(2026,2,15), UpdatedAt=Utc(2026,2,15) },
            new NewsArticle { Title="N0tail возвращается в OG — легенда снова в деле", Slug="n0tail-returns-og-2025", Summary="Двукратный чемпион TI Johan 'N0tail' Sundstein вышел из отставки и снова выступает за OG.", Content="Dota 2 сообщество ликует: N0tail, один из величайших игроков в истории игры и двукратный победитель The International (2018, 2019), вернулся в профессиональный спорт. 30-летний финн снова надел джерси OG и уже тренируется с новым составом команды.", Category="Transfer", IsPublished=true, AuthorId=admin.Id, PublishedAt=Utc(2025,10,5,9,0), CreatedAt=Utc(2025,10,5), UpdatedAt=Utc(2025,10,5) },
            new NewsArticle { Title="ATF: «Мы с Malr1ne — лучший дуэт в мире»", Slug="atf-interview-ti2025", Summary="Чемпион TI2025 ATF дал эксклюзивное интервью HDA.gg о победе на TI, паре с Malr1ne и планах на 2026 год.", Content="После исторической победы на The International 2025 ATF поделился мыслями с нашей редакцией. «Я верил в эту команду с первого дня. Малр1не — лучший мидлейнер, с которым я когда-либо играл. Мы читаем мысли друг друга». На вопрос о TI 2026 в Шанхае ATF ответил: «Мы приедем защищать титул».", Category="Interview", IsPublished=true, AuthorId=admin.Id, PublishedAt=Utc(2025,9,20,14,0), CreatedAt=Utc(2025,9,20), UpdatedAt=Utc(2025,9,20) },
            new NewsArticle { Title="Анализ мета: почему Timbersaw доминирует в 7.38", Slug="meta-analysis-timbersaw-738", Summary="После патча 7.38 Timbersaw стал одним из самых сильных героев в профессиональной игре. Разбираем почему.", Content="С выходом патча 7.38 Timbersaw превратился в настоящий кошмар для оппонентов. Изменения в нейтральных предметах усилили его потенциал для быстрого фарма, а новые предметы идеально подходят под его стиль игры. Не удивительно, что именно этого героя ATF из Falcons считает своим фаворитом в текущей мете.", Category="Analysis", IsPublished=true, AuthorId=admin.Id, PublishedAt=Utc(2026,2,25,11,0), CreatedAt=Utc(2026,2,25), UpdatedAt=Utc(2026,2,25) },
            new NewsArticle { Title="TI 2026 пройдёт в Шанхае — официально подтверждено", Slug="ti2026-shanghai-confirmed", Summary="Valve объявила, что The International 2026 состоится в Шанхае, Китай. Это будет первый TI в Китае с 2018 года.", Content="Valve официально объявила место проведения The International 2026 — Шанхай, Китай. Это большая победа для китайского сообщества Dota 2, которое получило TI на родной земле впервые с 2018 года. Xtreme Gaming, финалисты TI 2025, рассчитывают сыграть при поддержке домашней аудитории.", Category="News", IsPublished=true, AuthorId=admin.Id, PublishedAt=Utc(2025,10,15,10,0), CreatedAt=Utc(2025,10,15), UpdatedAt=Utc(2025,10,15) }
        );

        // Extra detailed news for richness
        ctx.NewsArticles.AddRange(
            new NewsArticle { Title="Обзор ростеров: кто выиграет TI 2026?", Slug="ti2026-roster-preview", Summary="Аналитики HDA.gg разбирают каждую команду и их шансы на победу на TI 2026 в Шанхае.", Content="С приближением TI 2026 в Шанхае мы решили проанализировать текущие ростеры всех топ-команд. Team Falcons выглядят как главные фавориты — ATF и Malr1ne продолжают доминировать в своих позициях, а состав выглядит цельнее некуда. Tundra Esports идут следом: 33 — лучший капитан мира прямо сейчас, а их механика давно стала легендой. Неожиданный претендент — Team Liquid после побды на BLAST Slam VI: Ace показывает лучшую игру в карьере.", Category="Analysis", IsPublished=true, AuthorId=admin.Id, PublishedAt=Utc(2026,3,15,10,0), CreatedAt=Utc(2026,3,15), UpdatedAt=Utc(2026,3,15) },
            new NewsArticle { Title="PGL Wallachia S7: предматчевый анализ", Slug="pgl-wallachia-s7-preview", Summary="Разбираем группы, ключевые противостояния и прогнозы для PGL Wallachia Season 7.", Content="PGL Wallachia Season 7 стартовал 10 марта с 16 лучшими командами мира. Группа A выглядит смертоносной: Falcons, Tundra и Spirit в одной группе гарантируют зрелищные матчи. Особо интересно противостояние Falcons против Tundra — реванш за поражение на BLAST Slam V. В группе B самый опасный тёмная лошадка — Heroic, которые показали невероятную игру на TI 2025.", Category="Analysis", IsPublished=true, AuthorId=admin.Id, PublishedAt=Utc(2026,3,9,11,0), CreatedAt=Utc(2026,3,9), UpdatedAt=Utc(2026,3,9) },
            new NewsArticle { Title="Yatoro: «Каждый TI — новый шанс войти в историю»", Slug="yatoro-interview-march-2026", Summary="Лучший керри мира рассказал HDA.gg о подготовке к PGL Wallachia и мечте о втором TI.", Content="В эксклюзивном интервью Ilya 'Yatoro' Mulyarchuk поделился своими мыслями о текущей мете и планах на TI 2026. «Патч 7.38 очень хорош для кэрри-героев, мне нравится играть в нынешней мете. Faceless Void и Morphling — мои любимцы прямо сейчас». О TI 2026 в Шанхае: «Мы уже думаем о нём. Команда голодна до победы».", Category="Interview", IsPublished=true, AuthorId=admin.Id, PublishedAt=Utc(2026,3,12,14,0), CreatedAt=Utc(2026,3,12), UpdatedAt=Utc(2026,3,12) },
            new NewsArticle { Title="Топ-5 героев патча 7.38 по данным про-сцены", Slug="top5-heroes-patch-738-pro", Summary="Статистика 1000+ про-матчей выявила пятёрку сильнейших героев текущего патча.", Content="На основе анализа более 1000 профессиональных матчей на патче 7.38 мы составили рейтинг самых сильных героев. 1. Timbersaw (ATF) — 68% пикрейт, 62% винрейт. 2. Sand King — 71% пикрейт, 58% винрейт. 3. Faceless Void — 55% пикрейт, 61% винрейт. 4. Lina — 49% пикрейт, 57% винрейт. 5. Primal Beast — 44% пикрейт, 59% винрейт.", Category="Analysis", IsPublished=true, AuthorId=admin.Id, PublishedAt=Utc(2026,3,5,12,0), CreatedAt=Utc(2026,3,5), UpdatedAt=Utc(2026,3,5) }
        );

        ctx.ActivityLogs.Add(new ActivityLog { UserId=admin.Id, Action="System initialized", Details="Database seeded with real 2025-2026 data", CreatedAt=now });
        await ctx.SaveChangesAsync();
    }
}
