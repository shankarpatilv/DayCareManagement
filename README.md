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
- `P7` is **Done** for current migration scope: students/immunizations/teachers/renewals/state-rules API modules are implemented.
- `P8` is **Done** for current API parity scope: Blazor pages/forms and guarded workflows are implemented for those modules.
- `P9` is **In Progress** (`P9-01` release readiness runbook/checklist in execution).

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
  - `DELETE /students/{studentId}` (Teacher)
  - `GET /students/{studentId}/immunizations`
  - `POST /students/{studentId}/immunizations` (Teacher)
  - `PUT /students/{studentId}/immunizations/{immunizationId}/{immunizationDate}` (Teacher)
  - `DELETE /students/{studentId}/immunizations/{immunizationId}/{immunizationDate}` (Teacher)
- Teachers
  - `GET /teachers`
  - `GET /teachers/{teacherId}`
  - `POST /teachers` (Teacher)
  - `PUT /teachers/{teacherId}` (Teacher)
  - `DELETE /teachers/{teacherId}` (Teacher)
- Renewals and rules
  - `GET /renewals/due`
  - `POST /renewals/{studentId}` (Teacher)
  - `GET /state-rules`

## Current WebApp Scope (implemented)

- `GET /login` with API-backed auth (`POST /auth/login`) and session management.
- Protected `students` workflows: list/create/update/delete.
- Protected `student details` workflows: immunization list/create/update/delete.
- Protected `teachers` workflows: list/create/update/delete.
- Protected `renewals` workflow: due list + apply renewal.
- Protected `state-rules` workflow: list + age filter.
- JWT bearer is attached by delegated HTTP handler.
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
- P9 release readiness gate: [docs/P9-RELEASE-READINESS.md](docs/P9-RELEASE-READINESS.md)
- P9 smoke test runbook: [docs/P9-SMOKE-TESTS.md](docs/P9-SMOKE-TESTS.md)

## Next Work

- Execute `P9` release readiness checklist and final smoke verification.
- Execute `P9` hypercare readiness planning and operational runbook.
