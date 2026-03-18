# DayCareManagement

Milestones M1 + M2 persistence foundation for migrating the Java Day Care Management System into a clean .NET 8 web architecture.

## Architecture

- `src/DayCareManagement.Domain` — Core domain model and shared base types.
- `src/DayCareManagement.Application` — Application contracts and use-case orchestration abstractions.
- `src/DayCareManagement.Infrastructure` — External/system implementations for application abstractions.
- `src/DayCareManagement.WebApi` — ASP.NET Core API host.
- `src/DayCareManagement.WebApp` — Blazor web application host.
- `tests/DayCareManagement.Application.Tests` — Unit tests for application-layer behavior.

## Current Status

- M1 foundation: project layout, baseline conventions, and host bootstrapping.
- M2 persistence foundation: domain entities, EF Core mappings, PostgreSQL context wiring, and initial migration.
- Layered solution structure and references compile with current solution setup.
- Test project scaffold is present and included in solution-level test runs.

## Quickstart

### Prerequisites

- .NET 8 SDK
- PostgreSQL
- EF Core CLI (`dotnet-ef`)

Install EF CLI globally (recommended):

```bash
dotnet tool install --global dotnet-ef
```

### Configure local connection string

From `DayCareManagement/`:

```bash
cp .env.example .env
```

Set `DAYCAREMANAGEMENT_CONNECTIONSTRING` in `.env`.

### Restore, build, and test

```bash
dotnet restore DayCareManagement.sln
dotnet build DayCareManagement.sln
dotnet test DayCareManagement.sln
```

### Apply database migration

```bash
dotnet ef database update \
  --project src/DayCareManagement.Infrastructure/DayCareManagement.Infrastructure.csproj \
  --startup-project src/DayCareManagement.WebApi/DayCareManagement.WebApi.csproj \
  --context DayCareManagementDbContext
```

## Collaboration Workflow

- GitHub setup and repository bootstrap: [docs/GITHUB-SETUP.md](docs/GITHUB-SETUP.md)
- Branching, release, and hotfix process: [docs/GIT-FLOW.md](docs/GIT-FLOW.md)

## Next Steps (M3+)

- Implement application-layer use cases and validation workflows.
- Add importer pipeline with CSV validation and mapping rules.
- Add API endpoints and corresponding Web App features.
- Expand automated tests for business rules and integration paths.
