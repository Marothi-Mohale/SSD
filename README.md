# SSD

SSD stands for Special Sound & Screen Discovery.

This repository is a production-oriented .NET monorepo for a mood-based platform that recommends music and movies. It is structured so the domain and application layers stay isolated from transport and deployment concerns, while the API, infrastructure, mobile-facing code, tests, and deployment assets can evolve independently.

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
- Clean architecture boundaries across `Domain`, `Application`, `Infrastructure`, and `Api`
- Centralized package management in [Directory.Packages.props](/workspaces/SSD/Directory.Packages.props)
- Shared repository-wide build standards in [Directory.Build.props](/workspaces/SSD/Directory.Build.props) and [.editorconfig](/workspaces/SSD/.editorconfig)

## Quick Start

1. Copy [.env.example](/workspaces/SSD/.env.example) into your local environment or secrets manager.
2. Copy [appsettings.Template.json](/workspaces/SSD/src/SSD.Api/appsettings.Template.json) to `appsettings.Development.json` if you want local API overrides.
3. Restore, build, and test:

```bash
HOME=/workspaces/SSD/.home DOTNET_CLI_HOME=/workspaces/SSD/.dotnet-home NUGET_PACKAGES=/workspaces/SSD/.nuget/packages dotnet restore SSD.sln
HOME=/workspaces/SSD/.home DOTNET_CLI_HOME=/workspaces/SSD/.dotnet-home NUGET_PACKAGES=/workspaces/SSD/.nuget/packages dotnet build SSD.sln --no-restore
HOME=/workspaces/SSD/.home DOTNET_CLI_HOME=/workspaces/SSD/.dotnet-home NUGET_PACKAGES=/workspaces/SSD/.nuget/packages dotnet test SSD.sln --no-build
```

4. Run the API:

```bash
HOME=/workspaces/SSD/.home DOTNET_CLI_HOME=/workspaces/SSD/.dotnet-home NUGET_PACKAGES=/workspaces/SSD/.nuget/packages dotnet run --project src/SSD.Api/SSD.Api.csproj
```

## What Is Included

- A minimal recommendation API with health checks and mood discovery
- Domain entities and value objects for recommendation matching
- Application services and interfaces for orchestration
- Infrastructure seed data provider ready to be replaced with Spotify and TMDb integrations
- A workload-free `SSD.Mobile` project that holds mobile-facing presentation models and formatting logic while staying buildable in CI
- Unit test projects for API, application, and mobile logic
- Baseline container and CI assets in [deploy](/workspaces/SSD/deploy/README.md) and [ci.yml](/workspaces/SSD/.github/workflows/ci.yml)

## Next Steps

- Add PostgreSQL persistence in `SSD.Infrastructure`
- Introduce authentication and secure token flows
- Replace the seed provider with Spotify and TMDb adapters
- Add a MAUI app head once the target build environment includes the required workloads
- Expand CI with linting, coverage reporting, and deployment environments
