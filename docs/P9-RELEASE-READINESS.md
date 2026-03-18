# P9 Release Readiness

Purpose: Define the minimum operational gate for releasing the current migrated scope (API + WebApp parity modules).

## Entry Criteria

- P7 and P8 tracker items are marked Done with merged code.
- Schema and data scripts are present and versioned (`scripts/001_schema.sql`, `scripts/002_load_data.sql`).
- Core build succeeds for solution (`dotnet build DayCareManagement.sln`).
- Release notes/issues for known limitations are recorded.

## Mandatory Checks

1. **Code/Build Health**
   - Solution restore/build succeeds in local and CI.
   - CI workflow executes build + migration consistency check + tests.
2. **Data and Migration Integrity**
   - EF migration consistency check reports no pending model changes.
   - Connection string and migration commands validated against target environment.
3. **Security and Access Controls**
   - Auth login, identity endpoint, and teacher-only authorization are validated.
   - JWT config values are present and non-placeholder.
4. **Smoke Test Coverage**
   - Execute [P9-SMOKE-TESTS.md](P9-SMOKE-TESTS.md) for API and WebApp.
5. **Operational Readiness**
   - Rollback checklist reviewed and deploy operator assigned.
   - Evidence table populated with links/artifacts for each gate.

## Go / No-Go Criteria

**Go** when all are true:

- No Sev-1/Sev-2 open defects for implemented modules.
- Mandatory checks complete with evidence.
- Blockers have explicit mitigation accepted by owner.

**No-Go** when any are true:

- Build fails in target release branch.
- Authentication/authorization regression in smoke tests.
- Data migration inconsistency or unresolved destructive migration risk.
- Missing rollback owner or unverified rollback path.

## Blocker Policy

- A blocker is any issue that prevents safe deploy, core user flow completion, or data integrity.
- Mark blocker in tracker with impact, owner, ETA, and workaround status.
- Only project owner can approve temporary waiver, and waiver must include expiry date.
- No blocker is closed without evidence of fix and verification rerun.

## Rollback Checklist

- Confirm previous deployable artifact/tag is available.
- Confirm database rollback approach (point-in-time restore or approved backward migration).
- Freeze writes if required by rollback plan.
- Re-deploy previous stable artifact.
- Run post-rollback smoke tests for auth, students, teachers, renewals.
- Announce rollback status and incident summary in team channel.
- Create follow-up ticket with root cause and prevention actions.

## Evidence Table

| Gate                  | Evidence Required                                               | Owner | Status  | Link / Artifact                                                                                                 |
| --------------------- | --------------------------------------------------------------- | ----- | ------- | --------------------------------------------------------------------------------------------------------------- |
| Build                 | `dotnet build DayCareManagement.sln` output (release branch)    | Team  | Pass    | Local run (2026-03-18): `dotnet build DayCareManagement.sln --configuration Release --no-restore`               |
| Tests                 | `dotnet test DayCareManagement.sln` output (or blocker record)  | Team  | Pass    | Local run (2026-03-18): `dotnet test DayCareManagement.sln --configuration Release --no-build` (35 passed)      |
| Migration consistency | CI `has-pending-model-changes` step pass                        | Team  | Pass    | Local run (2026-03-18): `dotnet ef migrations has-pending-model-changes ... --configuration Release --no-build` |
| API smoke tests       | Completed [P9-SMOKE-TESTS.md](P9-SMOKE-TESTS.md) API section    | Team  | Pending | TBD                                                                                                             |
| UI smoke tests        | Completed [P9-SMOKE-TESTS.md](P9-SMOKE-TESTS.md) WebApp section | Team  | Pending | TBD                                                                                                             |
| Rollback readiness    | Rollback owner + dry run confirmation                           | Team  | Pending | TBD                                                                                                             |
