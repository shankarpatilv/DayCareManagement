# DayCareManagement

Migration workspace for the Java Day Care Management System into a clean .NET 8 web architecture (foundation through auth baseline completed).

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
- M2 data hardening: case-insensitive unique email indexes and index cleanup applied.
- M3 quality gate: CI enforces build + test + EF model/migration consistency check.
- M3 auth baseline implemented in Web API: `/auth/login`, `/auth/me`, and role-policy protected endpoints.
- JWT startup hardening is enabled: API startup fails fast if `Jwt:SigningKey` is missing, too short, or placeholder/default.
- Layered solution structure and references compile with current solution setup.
- Application test project covers auth service behavior and JWT signing-key policy validation.

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
cat > .env <<'EOF'
DAYCAREMANAGEMENT_CONNECTIONSTRING=Host=localhost;Port=5432;Database=daycaremanagement;Username=postgres;Password=postgres
EOF
```

Set `DAYCAREMANAGEMENT_CONNECTIONSTRING` in `.env`.

### Configure JWT settings (required)

Set JWT settings via environment variables, user-secrets, or `appsettings`.

Environment variable names:

- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__SigningKey` (must be 32+ characters and not a placeholder/tutorial value)
- `Jwt__ExpiresMinutes`

Example:

```bash
export Jwt__Issuer=DayCareManagement
export Jwt__Audience=DayCareManagementClients
export Jwt__SigningKey='use-a-unique-random-secret-at-least-32-characters'
export Jwt__ExpiresMinutes=60
```

### Restore, build, and test

```bash
dotnet restore DayCareManagement.sln
dotnet build DayCareManagement.sln
dotnet test DayCareManagement.sln
```

### Verify EF model/migration consistency (same check as CI)

```bash
DAYCAREMANAGEMENT_CONNECTIONSTRING='Host=localhost;Port=5432;Database=ci_dummy;Username=ci_dummy;Password=ci_dummy' \
dotnet ef migrations has-pending-model-changes \
  --project src/DayCareManagement.Infrastructure/DayCareManagement.Infrastructure.csproj \
  --context DayCareManagementDbContext \
  --configuration Release \
  --no-build
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

- Continue migrating legacy feature modules beyond current auth and persistence scope.
- Add importer pipeline with CSV validation and mapping rules.
- Expand Web App integration against existing API endpoints.
- Expand automated tests for business rules and integration paths.
