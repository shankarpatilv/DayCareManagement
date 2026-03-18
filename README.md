# DayCareManagement

Migration workspace for the Java Day Care Management System into a clean .NET 8 web architecture.

## Architecture

- `src/DayCareManagement.Domain` — Core entities and shared domain primitives.
- `src/DayCareManagement.Application` — Application contracts and business-facing abstractions.
- `src/DayCareManagement.Infrastructure` — EF Core persistence, configuration, and system services.
- `src/DayCareManagement.WebApi` — ASP.NET Core minimal API host.
- `src/DayCareManagement.WebApp` — Blazor host (UI parity work in progress).
- `tests/DayCareManagement.Application.Tests` — Unit tests for application and API helper logic.

## Current Delivery Status

- `P0` to `P6` are complete (foundation, persistence, security hardening, integrity hardening, CI quality gate, auth/roles).
- `P7` is **In Progress**: first feature slice is implemented.
  - Students endpoints
  - Immunizations endpoints
  - Renewals endpoints
  - State-rules read endpoint
- `P8` is **In Progress**: first WebApp slice is implemented (login + students read flow).
- `P9` (release/hypercare) is pending.

See tracker: `docs/PROJECT-TASKS.md`.

## Current API Surface (implemented)

- Auth
  - `POST /auth/login`
  - `GET /auth/me`
  - `GET /auth/teacher-only`
- Students and immunizations
  - `GET /students`
  - `GET /students/{studentId}`
  - `POST /students` (Teacher)
  - `PUT /students/{studentId}` (Teacher)
  - `GET /students/{studentId}/immunizations`
  - `POST /students/{studentId}/immunizations` (Teacher)
  - `PUT /students/{studentId}/immunizations/{immunizationId}/{immunizationDate}` (Teacher)
  - `DELETE /students/{studentId}/immunizations/{immunizationId}/{immunizationDate}` (Teacher)
- Renewals and rules
  - `GET /renewals/due`
  - `POST /renewals/{studentId}` (Teacher)
  - `GET /state-rules`

## Current WebApp Slice (implemented)

- `GET /login` page with API-backed authentication (`POST /auth/login`).
- Protected students list page (`/students`) using authenticated API calls.
- Protected student details page (`/students/{studentId}`) with immunization list.
- JWT token attached to API calls via delegated HTTP handler.
- Session is scoped/in-memory for now (no persistent browser storage yet).

## Quickstart

### Prerequisites

- .NET 8 SDK
- PostgreSQL
- EF Core CLI (`dotnet-ef`)

Install EF CLI globally:

```bash
dotnet tool install --global dotnet-ef
```

### Configure connection string

From `DayCareManagement/`:

```bash
cat > .env <<'EOF'
DAYCAREMANAGEMENT_CONNECTIONSTRING=Host=localhost;Port=5432;Database=daycaremanagement;Username=postgres;Password=postgres
EOF
```

### Configure JWT settings (required)

Set values using environment variables, user-secrets, or secure configuration.

Required keys:

- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__SigningKey` (must be 32+ chars and not placeholder/default)
- `Jwt__ExpiresMinutes`

Example:

```bash
export Jwt__Issuer=DayCareManagement
export Jwt__Audience=DayCareManagementClients
export Jwt__SigningKey='use-a-unique-random-secret-at-least-32-characters'
export Jwt__ExpiresMinutes=60
```

### Build and test

```bash
dotnet restore DayCareManagement.sln
dotnet build DayCareManagement.sln
dotnet test DayCareManagement.sln
```

If tests fail with `Microsoft.AspNetCore.App 8.0.0` missing, install .NET 8 ASP.NET Core runtime/SDK and rerun.

### EF model/migration consistency check (CI-equivalent)

Run a Release build first, then run the EF check:

```bash
dotnet build DayCareManagement.sln --configuration Release --no-restore

DAYCAREMANAGEMENT_CONNECTIONSTRING='Host=localhost;Port=5432;Database=ci_dummy;Username=ci_dummy;Password=ci_dummy' \
dotnet ef migrations has-pending-model-changes \
  --project src/DayCareManagement.Infrastructure/DayCareManagement.Infrastructure.csproj \
  --context DayCareManagementDbContext \
  --configuration Release \
  --no-build
```

### Apply migrations

```bash
dotnet ef database update \
  --project src/DayCareManagement.Infrastructure/DayCareManagement.Infrastructure.csproj \
  --startup-project src/DayCareManagement.WebApi/DayCareManagement.WebApi.csproj \
  --context DayCareManagementDbContext
```

### Run API host

```bash
dotnet run --project src/DayCareManagement.WebApi/DayCareManagement.WebApi.csproj
```

### Run WebApp host

Set WebApp API base URL in `src/DayCareManagement.WebApp/appsettings.json`:

```json
{
	"Api": {
		"BaseUrl": "http://localhost:5125"
	}
}
```

Run WebApp:

```bash
dotnet run --project src/DayCareManagement.WebApp/DayCareManagement.WebApp.csproj
```

## Collaboration Docs

- GitHub bootstrap: [docs/GITHUB-SETUP.md](docs/GITHUB-SETUP.md)
- Git flow: [docs/GIT-FLOW.md](docs/GIT-FLOW.md)
- Data dictionary: [docs/DATA-DICTIONARY.md](docs/DATA-DICTIONARY.md)
- Entity model notes: [docs/M2-ENTITY-MODEL.md](docs/M2-ENTITY-MODEL.md)

## Next Work

- Continue `P7` module migration to full legacy parity.
- Complete `P8` Blazor UI integration and parity views.
- Execute `P9` release readiness and hypercare checklist.
