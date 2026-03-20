# SSD

SSD stands for Special Sound & Screen Discovery.

This repository is a production-oriented .NET monorepo for a mood-based platform that recommends music and movies. It uses clean architecture boundaries so domain and application logic stay isolated from transport, persistence, and deployment concerns.

## Monorepo Layout

```text
src/
  SSD.Api
  SSD.Application
  SSD.Domain
  SSD.Infrastructure
  SSD.Mobile
tests/
  SSD.Api.Tests
  SSD.Application.Tests
  SSD.Mobile.UnitTests
deploy/
.github/workflows/
```

## Technology Choices

- .NET 10 via [global.json](/workspaces/SSD/global.json)
- ASP.NET Core for the API host
- PostgreSQL-ready persistence through EF Core in `SSD.Infrastructure`
- Clean architecture boundaries across `Domain`, `Application`, `Infrastructure`, and `Api`
- Centralized package management in [Directory.Packages.props](/workspaces/SSD/Directory.Packages.props)
- Shared repository-wide build standards in [Directory.Build.props](/workspaces/SSD/Directory.Build.props) and [.editorconfig](/workspaces/SSD/.editorconfig)

## Project Status

The current repository is partially aligned with the original SSD product brief.

### Implemented Today

- User registration, login, refresh-token rotation, logout, and authenticated profile lookup in `SSD.Api`
- Mood selection and recommendation discovery endpoints
- PostgreSQL-ready domain model, EF Core `DbContext`, and initial migration in `SSD.Infrastructure`
- Spotify integration for compliant metadata-only usage:
  - OAuth account linking
  - Spotify `/me`
  - Spotify track URL parsing
  - Spotify track metadata resolution
- Favorites, recommendation history, feedback, and audit entities in the domain model
- Baseline CI, deployment assets, analyzers, nullable reference types, and centralized package management
- Unit and integration test projects for API, application, and mobile-facing logic

### Not Yet Complete

- `SSD.Mobile` now has a real `.NET MAUI` shell, but full device builds still depend on installing the MAUI workloads in the target environment.
- TMDb integration is not implemented yet.
- End-to-end favorites/history UI and real persistence workflows are not fully wired through API + mobile.
- SQLite local persistence for the mobile app is not implemented yet.
- Observability is still baseline rather than production-complete.
- CI/CD exists, but it is not yet a full production deployment pipeline with environment promotion, secrets integration, and release automation.
- The full solution now builds cleanly in this repository. In this environment, the most reliable command path is the serial MSBuild mode shown below.

## Milestone Workflow

Every milestone in this repo should follow the same loop:

1. State the plan.
2. Implement the change.
3. Run builds and tests.
4. Fix issues surfaced by verification.
5. Summarize status and next steps.

## Quick Start

1. Copy [.env.example](/workspaces/SSD/.env.example) into your local environment or secrets manager.
2. Copy [appsettings.Template.json](/workspaces/SSD/src/SSD.Api/appsettings.Template.json) to `appsettings.Development.json` if you want local API overrides.
3. Restore, build, and test:

```bash
HOME=/workspaces/SSD/.home DOTNET_CLI_HOME=/workspaces/SSD/.dotnet-home NUGET_PACKAGES=/workspaces/SSD/.nuget/packages dotnet restore SSD.sln -m:1
HOME=/workspaces/SSD/.home DOTNET_CLI_HOME=/workspaces/SSD/.dotnet-home NUGET_PACKAGES=/workspaces/SSD/.nuget/packages dotnet build SSD.sln --no-restore -m:1
HOME=/workspaces/SSD/.home DOTNET_CLI_HOME=/workspaces/SSD/.dotnet-home NUGET_PACKAGES=/workspaces/SSD/.nuget/packages dotnet test SSD.sln --no-build -m:1
```

The repo also disables build-time NuGet vulnerability auditing during normal restore/build so local and CI builds do not fail when the feed is temporarily unreachable. Run a dedicated security audit step separately in connected environments.

4. Run the API:

```bash
HOME=/workspaces/SSD/.home DOTNET_CLI_HOME=/workspaces/SSD/.dotnet-home NUGET_PACKAGES=/workspaces/SSD/.nuget/packages dotnet run --project src/SSD.Api/SSD.Api.csproj
```

## What Is Included

- An ASP.NET Core Web API host with auth, recommendation, and Spotify endpoints
- Domain entities and value objects for users, sessions, favorites, feedback, search history, audit logs, and linked Spotify accounts
- Application contracts and interfaces for auth, recommendations, and Spotify integration
- PostgreSQL-ready infrastructure with EF Core mappings and migrations
- A hybrid `SSD.Mobile` project with:
  - a real `.NET MAUI` app shell for Android, iOS, and Mac Catalyst
  - a `net10.0` target that keeps shared mobile logic buildable and testable in environments without MAUI workloads
- Unit and integration test projects for API, application, and mobile-facing logic
- Baseline container and CI assets in [deploy](/workspaces/SSD/deploy/README.md) and [ci.yml](/workspaces/SSD/.github/workflows/ci.yml)

## Next Steps

- Replace the seed recommendation provider with real Spotify + TMDb-backed recommendation orchestration
- Wire the new `.NET MAUI` shell to real API calls, auth state, favorites, and SQLite local persistence
- Expose favorites, recommendation history, and feedback flows through API endpoints
- Expand observability with structured tracing, metrics, and operational dashboards
- Upgrade CI/CD into a production-grade pipeline with deployment environments, secrets management, and release automation
