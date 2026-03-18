# Project Tasks Tracker

Purpose: Track execution status for the DayCareManagement project with clear ownership, status, and notes for delivery readiness.

## Status Legend

- **Not Started**: Task is defined but work has not begun.
- **In Progress**: Work is actively underway.
- **Done**: Task is completed and verified.
- **Blocked**: Task cannot proceed due to a dependency or issue.

## Execution Checklist

| ID    | Phase               | Task                                                     | Owner   | Status      | Notes                                                                                                                         |
| ----- | ------------------- | -------------------------------------------------------- | ------- | ----------- | ----------------------------------------------------------------------------------------------------------------------------- |
| P0-01 | Foundation          | Stack finalized                                          | Team    | Done        | .NET + PostgreSQL + Web API + Blazor established.                                                                             |
| P0-02 | Foundation          | Web architecture defined                                 | Team    | Done        | Layered solution structure in place (Domain/Application/Infrastructure/Web).                                                  |
| P0-03 | Foundation          | Persistence foundation done                              | Team    | Done        | Base schema, data access, and migration path established.                                                                     |
| P1-01 | Governance          | Branch hygiene / push PR                                 | You     | Done        | Confirm clean branch, push latest commits, open/update PR.                                                                    |
| P2-01 | Data Migration      | Schema script completion                                 | Copilot | Done        | Finalize and validate `scripts/001_schema.sql`.                                                                               |
| P2-02 | Data Migration      | Load script completion                                   | Copilot | Done        | Finalize and validate `scripts/002_load_data.sql`.                                                                            |
| P3-01 | Security Hardening  | Keep `Password` field name and enforce hash-only storage | Copilot | Done        | Domain entities and EF persistence keep `Password` column/property name with hash-only storage policy.                        |
| P3-02 | Security Hardening  | Add in-place migration for password hash transition      | Copilot | Done        | Added migration `BackfillPasswordHashesInPlace` to backfill existing plaintext values to hashes in `Password` without rename. |
| P3-03 | Security Hardening  | Secret scan and cleanup                                  | Copilot | Done        | Repo scanned for hardcoded secrets in source/docs/scripts; .env.example sanitized; rotate any previously exposed local creds. |
| P4-01 | Integrity Hardening | Enforce case-insensitive unique email                    | Team    | Not Started | Add normalized unique constraint/index strategy.                                                                              |
| P4-02 | Integrity Hardening | Index cleanup and validation                             | Team    | Not Started | Remove duplicates/redundant indexes and verify query plans.                                                                   |
| P5-01 | Quality Gate        | Testing and CI quality gate                              | Team    | Not Started | Require build + tests + migration checks in CI.                                                                               |
| P6-01 | M3 Auth             | Auth/roles implementation (M3)                           | Team    | Not Started | Implement authentication, authorization roles, and policy checks.                                                             |
| P7-01 | Feature Migration   | Feature migration modules                                | Team    | Not Started | Port remaining legacy features module-by-module.                                                                              |
| P8-01 | Blazor UI           | Blazor UI completion                                     | Team    | Not Started | Complete parity views/forms/validation with API integration.                                                                  |
| P9-01 | Release             | Release readiness                                        | Team    | Not Started | Final smoke test, docs check, deployment checklist sign-off.                                                                  |
| P9-02 | Release             | Hypercare                                                | Team    | Not Started | Post-release monitoring, bug triage, and stabilization window.                                                                |
