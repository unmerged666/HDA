# HDA.gg — High Dota Analytics

> The most comprehensive Dota 2 esports statistics platform.  
> Live scores · Match history · Team rankings · Deep analytics

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 8 + Blazor Server |
| UI | MudBlazor 7 (Material Design, dark theme) |
| ORM | Entity Framework Core 8 (Code-First) |
| Database | PostgreSQL 16 |
| Container | Docker + Docker Compose |
| Auth | Custom BCrypt session auth via AppState |
| External API | OpenDota API (hero sync) |

---

## Project Structure

```
HDA/
├── HDA.sln
├── Dockerfile
├── docker-compose.yml
└── src/
    ├── HDA.Domain/              # Entities, Enums
    │   ├── Entities/            # User, Team, ProPlayer, Match, Tournament, Hero...
    │   └── Enums/               # UserRole, MatchStatus, TournamentTier...
    ├── HDA.Infrastructure/      # Data access, services
    │   ├── Data/                # HdaDbContext, DbSeeder
    │   ├── Repositories/        # Generic repository
    │   └── Services/            # AuthService, MatchService, TeamService...
    └── HDA.Web/                 # Blazor Server app
        ├── Components/
        │   ├── Layout/          # MainLayout.razor
        │   ├── Pages/
        │   │   ├── Admin/       # Dashboard, MatchEditor, TeamEditor
        │   │   ├── Auth/        # Login, Register, UserProfile, ProProfile
        │   │   ├── Matches/     # Matches, MatchDetail
        │   │   ├── Teams/       # Teams, TeamDetail
        │   │   ├── Players/     # Players, PlayerDetail
        │   │   ├── Tournaments/ # Tournaments, TournamentDetail
        │   │   └── News/        # News
        │   └── Shared/          # MatchCard, TournamentGrid
        └── wwwroot/
            ├── css/hda.css
            └── js/hda.js
```

---

## Database Schema

### Key Entities & Relationships

```
User (1) ──── (0..1) ProPlayer
ProPlayer (N) ──── (1) Team
Team (1) ──── (N) Match [as TeamA / TeamB]
Tournament (1) ──── (N) TournamentStage (1) ──── (N) Match
Match (1) ──── (N) GameMap (1) ──── (N) MatchPlayerStat
GameMap (1) ──── (N) GameMapDraft
ProPlayer (N:N) Hero [via PlayerHeroStat]
Team (N:N) Hero [via TeamHeroStat]
User (1) ──── (N) ActivityLog
```

---

## Roles & Permissions

| Role | Access |
|------|--------|
| **Regular** | View all public pages, profile, can apply for pro status |
| **ProPlayer** | All of above + edit own pro profile, upload avatar |
| **Admin** | Full access + admin panel, approve/reject pro applications, manage teams/matches/tournaments, view logs, run console commands |

### Default Seed Accounts

| Email | Password | Role |
|-------|----------|------|
| `admin@hda.gg` | `Admin123!` | Admin |
| `fan@hda.gg` | `Fan123!` | Regular |

---

## Quick Start

### Option 1: Docker Compose (recommended)

```bash
git clone <repo-url> HDA
cd HDA
docker compose up --build
```

App will be available at **http://localhost:8080**  
Database will be auto-migrated and seeded on first run.

### Option 2: Local Development

**Prerequisites:** .NET 8 SDK, PostgreSQL 16

```bash
# 1. Clone and navigate
git clone <repo-url> HDA && cd HDA

# 2. Start PostgreSQL (or update connection string)
# Edit src/HDA.Web/appsettings.Development.json with your local DB credentials

# 3. Restore & run migrations
cd src/HDA.Web
dotnet restore
dotnet ef database update --project ../HDA.Infrastructure

# 4. Run
dotnet run
```

App available at **https://localhost:5001** / **http://localhost:5000**

---

## EF Core Migrations

```bash
# From /src/HDA.Web directory:

# Add a new migration
dotnet ef migrations add MigrationName --project ../HDA.Infrastructure

# Apply migrations
dotnet ef database update --project ../HDA.Infrastructure

# Remove last migration
dotnet ef migrations remove --project ../HDA.Infrastructure
```

---

## Features

### Public Pages
- **Home** — Live matches, upcoming schedule, recent results, latest news
- **Matches** — Tabs: Upcoming / Live / Results with tournament filter
- **Match Detail** — Full scoreboard, draft picks/bans, per-game stats
- **Teams** — World ranking table with rating points
- **Team Detail** — Roster, recent results, hero pick/ban stats
- **Players** — Pro player gallery with search
- **Player Detail** — Career stats, hero pool, recent games
- **Tournaments** — Grouped by status (Ongoing/Upcoming/Completed)
- **Tournament Detail** — Participants with placements/prizes, matches by stage
- **Heroes** — Full hero grid with attribute/role filters
- **News** — Category-filtered news feed

### User Features
- Register / Login
- Profile page with password change
- Apply for Pro Player status (pending admin approval)
- Pro Profile — edit nickname, team, role, bio, country, upload avatar photo

### Admin Panel (`/admin`)
- Stats dashboard (users, teams, matches, tournaments)
- **Pending Approvals** — Approve/reject pro player applications
- **Teams** — Create/edit/delete teams, upload logos
- **Matches** — Schedule/edit/delete matches, set scores and winners
- **Activity Logs** — Full audit trail of all actions
- **Console** — Built-in admin console:
  - `sync-heroes` — Pull hero data from OpenDota API
  - `clear-logs` — Delete log entries older than 30 days
  - `list-users` — List all registered users
  - `stats` — Print database stats

---

## Data Sources

| Source | Usage |
|--------|-------|
| **OpenDota API** | Hero sync (`/api/heroes`) |
| **PandaScore** | Ready for integration (match/tournament data) |
| **Steam Web API** | Ready for integration (match replays) |
| Manual entry | Admin panel — teams, matches, tournaments |

---

## Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `ConnectionStrings__DefaultConnection` | See appsettings | PostgreSQL connection string |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Environment name |

---

## License

Educational project. Not affiliated with Valve Corporation or any Dota 2 partner.  
Data from OpenDota is used under their open API terms.
