# HDA.gg — High Dota Analytics

> Профессиональная аналитическая платформа для отслеживания киберспортивной сцены Dota 2

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-512BD4)](https://dotnet.microsoft.com)
[![Blazor Server](https://img.shields.io/badge/Blazor-Server-7B2D8B)](https://blazor.net)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791)](https://postgresql.org)
[![MudBlazor](https://img.shields.io/badge/MudBlazor-7-594AE2)](https://mudblazor.com)

---

## Содержание

- [О проекте](#о-проекте)
- [Архитектура](#архитектура)
- [Схема базы данных](#схема-базы-данных)
- [Требования](#требования)
- [Установка и запуск](#установка-и-запуск)
- [Перенос на другой компьютер](#перенос-на-другой-компьютер)
- [Аккаунты по умолчанию](#аккаунты-по-умолчанию)
- [Структура проекта](#структура-проекта)
- [Страницы приложения](#страницы-приложения)
- [API интеграция](#api-интеграция)

---

## О проекте

**HDA.gg** — веб-платформа для аналитики профессиональной сцены Dota 2. Предоставляет доступ к статистике матчей, рейтингам команд, турнирной информации, профилям игроков и новостной ленте.

**Ключевые возможности:**
- 🏆 Рейтинг профессиональных команд с детальной статистикой
- ⚔ Матчи и результаты турниров в реальном времени
- 🧙 Галерея всех 126+ героев Dota 2 (патч 7.40c) с фильтрацией
- 👥 Профили профессиональных игроков с KDA-статистикой
- 📰 Новостная лента профессиональной сцены
- ⚙ Полная панель администратора с CRUD-операциями
- 🎮 Система верификации про-игроков
- 🌗 Поддержка тёмной и светлой темы

---

## Архитектура

Проект построен на принципах **Clean Architecture** и разделён на три независимых слоя:

```
HDA/
├── src/
│   ├── HDA.Domain/          # Доменный слой
│   │   ├── Entities/        # Сущности (User, Team, Match, Hero, ...)
│   │   └── Enums/           # Перечисления (UserRole, ProPlayerStatus, ...)
│   │
│   ├── HDA.Infrastructure/  # Инфраструктурный слой
│   │   ├── Data/
│   │   │   ├── HdaDbContext.cs      # EF Core DbContext
│   │   │   └── DbSeeder.cs          # Начальное заполнение БД
│   │   └── Services/                # Сервисы (Team, Match, News, OpenDota, ...)
│   │
│   └── HDA.Web/             # Слой представления (Blazor Server)
│       ├── Components/
│       │   ├── Layout/      # MainLayout, NavMenu
│       │   └── Pages/       # Страницы приложения
│       ├── wwwroot/
│       │   ├── css/hda.css  # Глобальные стили
│       │   └── js/hda.js    # Вспомогательные скрипты
│       └── Program.cs       # Точка входа, DI-регистрация
```

### Диаграмма слоёв

```
┌─────────────────────────────────────────┐
│              HDA.Web                    │
│     (Blazor Server, MudBlazor UI)       │
└──────────────┬──────────────────────────┘
               │ зависит от
┌──────────────▼──────────────────────────┐
│           HDA.Infrastructure            │
│   (EF Core, DbContext, Services)        │
└──────────────┬──────────────────────────┘
               │ зависит от
┌──────────────▼──────────────────────────┐
│             HDA.Domain                  │
│      (Entities, Enums — без зависимостей)│
└─────────────────────────────────────────┘
```

### Технологический стек

| Компонент | Технология |
|-----------|-----------|
| Бэкенд | ASP.NET Core 8.0 |
| UI | Blazor Server |
| Компоненты | MudBlazor 7 |
| ORM | Entity Framework Core 8 |
| База данных | PostgreSQL 16/18 |
| Аутентификация | BCrypt.Net + Cookie Auth |
| Внешний API | OpenDota API |
| Шрифты | Syne, Manrope, DM Mono |

---

## Схема базы данных

```
Users ──────────────────── ProPlayers
  │ id (PK)                  │ id (PK)
  │ username                 │ userId (FK → Users)
  │ email                    │ nickname
  │ passwordHash             │ realName
  │ role (Regular/Pro/Admin) │ country
  │ isActive                 │ steamId
  └─────────────────────────►│ teamId (FK → Teams)
                             │ status (Pending/Approved/Rejected)
                             │ role (Carry/Mid/Off/Sup4/Sup5)

Teams ──────────────────── Matches
  │ id (PK)                  │ id (PK)
  │ name                     │ teamAId (FK → Teams)
  │ tag                      │ teamBId (FK → Teams)
  │ logoUrl                  │ scoreA / scoreB
  │ country                  │ tournamentId (FK → Tournaments)
  │ worldRank                │ status (Upcoming/Live/Finished)
  └─────────────────────────►│ format (Bo1/Bo2/Bo3/Bo5)

Tournaments                Heroes
  │ id (PK)                  │ id (PK)
  │ name                     │ name (internal key)
  │ prizePool                │ localizedName
  │ status                   │ primaryAttribute (str/agi/int/all)
  │ tier                     │ attackType (Melee/Ranged)
  └──────────────────────────│ imageUrl
                             │ roles (text[])

NewsArticles               ActivityLogs
  │ id (PK)                  │ id (PK)
  │ title                    │ userId (FK → Users)
  │ content                  │ action
  │ category                 │ entityType
  │ imageUrl                 │ createdAt
  └──────────────────────────┘
```

---

## Требования

### Обязательные
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 16 или 18](https://www.postgresql.org/download/)

### Опциональные
- Docker (для запуска через контейнеры)
- Git

---

## Установка и запуск

### 1. Клонирование / копирование проекта

```bash
git clone https://github.com/yourname/hda.git
cd hda
```

Или скопируй папку проекта на компьютер.

### 2. Настройка базы данных PostgreSQL

Открой **SQL Shell (psql)** или выполни в PowerShell:

```powershell
# Создать пользователя и базу данных
psql -U postgres -c "CREATE USER hda_user WITH PASSWORD 'hda_pass';"
psql -U postgres -c "CREATE DATABASE hda_db OWNER hda_user;"
psql -U postgres -d hda_db -c "GRANT ALL ON SCHEMA public TO hda_user;"
```

### 3. Строка подключения

Строка подключения в `src/HDA.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=hda_db;Username=hda_user;Password=hda_pass"
  }
}
```

### 4. Восстановление зависимостей

```bash
dotnet restore src/HDA.Web/HDA.Web.csproj
```

### 5. Применение миграций

```bash
dotnet ef database update --project src/HDA.Infrastructure --startup-project src/HDA.Web
```

> **Примечание:** Миграции применяются автоматически при первом запуске через `app.MigrateDatabase()` в `Program.cs`. Ручной запуск не обязателен.

### 6. Запуск приложения

```bash
cd src/HDA.Web
dotnet run
```

Приложение будет доступно по адресу: **http://localhost:5000**

При первом запуске `DbSeeder` автоматически заполнит базу данных:
- 126 героев Dota 2 (патч 7.40c)
- Профессиональные команды, матчи, турниры
- Тестовые аккаунты пользователей
- Новостные статьи

---

## Запуск через Docker

### docker-compose.yml

```yaml
version: '3.8'

services:
  db:
    image: postgres:16
    environment:
      POSTGRES_USER: hda_user
      POSTGRES_PASSWORD: hda_pass
      POSTGRES_DB: hda_db
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data

  web:
    build: .
    ports:
      - "5000:8080"
    environment:
      - ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=hda_db;Username=hda_user;Password=hda_pass
    depends_on:
      - db

volumes:
  pgdata:
```

### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/HDA.Web/HDA.Web.csproj", "src/HDA.Web/"]
COPY ["src/HDA.Infrastructure/HDA.Infrastructure.csproj", "src/HDA.Infrastructure/"]
COPY ["src/HDA.Domain/HDA.Domain.csproj", "src/HDA.Domain/"]
RUN dotnet restore "src/HDA.Web/HDA.Web.csproj"
COPY . .
RUN dotnet publish "src/HDA.Web/HDA.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HDA.Web.dll"]
```

```bash
# Сборка и запуск
docker compose build
docker compose up -d

# Просмотр логов
docker compose logs -f web
```

---

## Перенос на другой компьютер

### Резервная копия базы данных

```powershell
# Создать дамп
& "C:\Program Files\PostgreSQL\18\bin\pg_dump.exe" `
  -U hda_user -h localhost -d hda_db -F c `
  -f "C:\путь\до\hda_backup.dump"
```

### Восстановление на новом компьютере

```powershell
# 1. Создать БД
psql -U postgres -c "CREATE USER hda_user WITH PASSWORD 'hda_pass';"
psql -U postgres -c "CREATE DATABASE hda_db OWNER hda_user;"
psql -U postgres -d hda_db -c "GRANT ALL ON SCHEMA public TO hda_user;"

# 2. Восстановить дамп
& "C:\Program Files\PostgreSQL\18\bin\pg_restore.exe" `
  -U hda_user -h localhost -d hda_db "C:\путь\до\hda_backup.dump"

# 3. Запустить проект
cd C:\путь\до\HDA\src\HDA.Web
dotnet run
```

---

## Аккаунты по умолчанию

| Роль | Email | Пароль |
|------|-------|--------|
| Администратор | `admin12@example.com` | `admin12parol12` |
| Пользователь | `fan@hda.gg` | `Fan123!` |
| Пользователь | `dota@hda.gg` | `Dota123!` |
| Аналитик | `analyst@hda.gg` | `Ana123!` |

---

## Структура проекта

```
src/
├── HDA.Domain/
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Team.cs
│   │   ├── Match.cs
│   │   ├── Tournament.cs
│   │   ├── Hero.cs
│   │   ├── ProPlayer.cs
│   │   ├── NewsArticle.cs
│   │   └── ActivityLog.cs
│   └── Enums/
│       └── Enums.cs          # UserRole, ProPlayerStatus, PlayerRole, ...
│
├── HDA.Infrastructure/
│   ├── Data/
│   │   ├── HdaDbContext.cs
│   │   └── DbSeeder.cs       # Автозаполнение БД при запуске
│   └── Services/
│       ├── AuthService.cs
│       ├── TeamService.cs
│       ├── MatchService.cs
│       ├── TournamentService.cs
│       ├── NewsService.cs
│       ├── ProPlayerService.cs
│       ├── OpenDotaService.cs # Интеграция с OpenDota API
│       └── AppState.cs        # Глобальное состояние приложения
│
└── HDA.Web/
    ├── Components/
    │   ├── Layout/
    │   │   ├── MainLayout.razor
    │   │   └── NavMenu.razor
    │   └── Pages/
    │       ├── Home.razor
    │       ├── Matches/
    │       ├── Teams/
    │       ├── Tournaments/
    │       ├── Heroes/
    │       ├── Players/
    │       ├── News/
    │       ├── Auth/
    │       └── Admin/
    │           ├── AdminDashboard.razor
    │           ├── AdminOverviewPanel.razor
    │           ├── AdminUsersPanel.razor
    │           ├── AdminTeamsEditor.razor
    │           ├── AdminMatchesEditor.razor
    │           ├── AdminTournamentsEditor.razor
    │           ├── AdminNewsPanel.razor
    │           ├── AdminPlayersEditor.razor
    │           ├── AdminConsole.razor
    │           ├── AdminLogsPanel.razor
    │           └── AdminProApplicationsPanel.razor
    ├── wwwroot/
    │   ├── css/hda.css
    │   └── js/hda.js
    ├── appsettings.json
    └── Program.cs
```

---

## Страницы приложения

| URL | Описание |
|-----|----------|
| `/` | Главная страница с анимацией, статистикой, матчами |
| `/matches` | Список матчей с фильтрацией |
| `/teams` | Рейтинг команд |
| `/teams/{id}` | Детальная страница команды |
| `/teams/compare` | Сравнение двух команд |
| `/tournaments` | Список турниров |
| `/heroes` | Галерея героев с фильтрами |
| `/players` | Профили про-игроков |
| `/news` | Новостная лента |
| `/login` | Вход в систему |
| `/register` | Регистрация |
| `/profile` | Личный кабинет |
| `/pro-profile` | Форма заявки на верификацию |
| `/admin` | Панель администратора |

---

## API интеграция

### OpenDota API

Приложение использует публичный [OpenDota API](https://api.opendota.com) для синхронизации данных о героях Dota 2.

**Эндпоинт:** `GET https://api.opendota.com/api/heroes`

Синхронизация выполняется автоматически при запуске (`OpenDotaService.SyncHeroesAsync()`). Если данные в БД актуальны, синхронизация пропускается.

### Steam CDN

Изображения героев загружаются с официального Steam CDN:

```
https://cdn.cloudflare.steamstatic.com/apps/dota2/images/dota_react/heroes/{hero_name}.png
```

---

## Лицензия

Проект разработан в учебных целях в рамках курсового проекта по дисциплине «Кроссплатформенная среда исполнения программного обеспечения».
