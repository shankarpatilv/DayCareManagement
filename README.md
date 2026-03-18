# DayCareManagement

Milestone M1 foundation scaffold for migrating the Java Day Care Management System into a clean .NET 8 web architecture.

## Architecture

- `src/DayCareManagement.Domain` — Core domain model and shared base types.
- `src/DayCareManagement.Application` — Application contracts and use-case orchestration abstractions.
- `src/DayCareManagement.Infrastructure` — External/system implementations for application abstractions.
- `src/DayCareManagement.WebApi` — ASP.NET Core API host.
- `src/DayCareManagement.WebApp` — Blazor web application host.
- `tests/DayCareManagement.Application.Tests` — Unit tests for application-layer behavior.

## M1 Scope

- Project layout and baseline solution conventions.
- Minimal project references between layers.
- Starter abstractions and one infrastructure implementation.
- Minimal host bootstrapping for Web API and Web App.
- Basic test project with placeholder compiling test.

## Build (when .NET SDK is available)

```bash
dotnet restore DayCareManagement.sln
dotnet build DayCareManagement.sln
```

## Collaboration Workflow

- GitHub setup and repository bootstrap: [docs/GITHUB-SETUP.md](docs/GITHUB-SETUP.md)
- Branching, release, and hotfix process: [docs/GIT-FLOW.md](docs/GIT-FLOW.md)

## Next Steps (M2)

- Add domain entities for students, teachers, classrooms, immunizations.
- Add application use cases and DTO contracts.
- Add persistence abstractions and infrastructure data adapters.
- Add first API endpoints and initial Blazor pages.
- Add test coverage for core business rules and use cases.
