# SSD

SSD stands for Special Sound & Screen Discovery.

It is a mood-based mobile app that recommends music, movies, or both based on how the user feels. The target stack is:

- .NET MAUI mobile app
- ASP.NET Core Web API backend
- PostgreSQL in production
- SQLite for local mobile persistence

## Current Milestone

This repository now contains the first project foundation:

- `src/Backend/SSD.Api`: minimal API for recommendation discovery
- `src/Backend/SSD.Application`: application services and contracts
- `src/Backend/SSD.Domain`: core domain models
- `src/Mobile/SSD.Mobile`: initial .NET MAUI app shell
- `tests/SSD.Api.Tests`: backend test project

## Implemented So Far

- Mood-driven recommendation discovery endpoint
- In-memory seed recommendation provider shaped for future Spotify/TMDb integrations
- Validation and correlation-aware API responses
- Initial MAUI shell with mood selection and recommendation cards
- Test project scaffold for backend validation rules

## API Example

`POST /api/recommendations/discover`

```json
{
  "mood": "Calm",
  "energy": "low",
  "timeOfDay": "evening",
  "familyFriendlyOnly": true,
  "includeMusic": true,
  "includeMovies": true
}
```

## Build Notes

Backend commands in this environment should use:

```bash
DOTNET_CLI_HOME=/workspaces/SSD/.dotnet-home dotnet build
```

Notes:

- The current container does not have .NET MAUI workloads installed, so the mobile project is scaffolded but may not build here yet.
- Test package restore may need network access to NuGet depending on sandbox permissions.

## Next Steps

- Add ASP.NET Core identity and token-based authentication
- Add PostgreSQL persistence and EF Core migrations
- Add SQLite offline persistence in the mobile app
- Replace seed recommendations with Spotify and TMDb integrations
- Add favorites, history, and recommendation explanation persistence
- Add CI/CD and deployment automation
