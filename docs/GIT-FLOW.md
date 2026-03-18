# GitFlow for DayCareManagement

This document defines the practical branch, merge, and release process for this repository.

## Branch Model

- `main`: production-ready history only.
- `develop`: integration branch for completed features.
- `feature/*`: short-lived branches for scoped work (branched from `develop`).
- `release/*`: stabilization branches for a planned release (branched from `develop`).
- `hotfix/*`: urgent production fixes (branched from `main`).

## Commit Message Convention

Use Conventional Commits:

```text
<type>(optional-scope): <short description>
```

Common types:

- `feat`: new behavior
- `fix`: bug fix
- `docs`: documentation updates
- `refactor`: internal structure changes without behavior change
- `test`: tests added/updated
- `chore`: maintenance and tooling

Examples:

- `feat(application): add student enrollment use case`
- `fix(webapi): validate null classroom assignment`
- `docs(gitflow): add release branch instructions`

## Pull Request Rules

- `feature/*` PR target: `develop`
- `release/*` PR target: `main` (after validation), then back-merge `main` into `develop`
- `hotfix/*` PR target: `main`, then back-merge `main` into `develop`
- Avoid direct pushes to `main` and `develop`.

## Release Tagging

- Tag releases on `main` using semantic version tags: `vX.Y.Z`.
- Create tags after the release PR is merged.
- Example: `v1.2.0`, `v1.2.1`.

```bash
git checkout main
git pull origin main
git tag vX.Y.Z
git push origin vX.Y.Z
```

## Commands: Start a Feature Branch

```bash
git checkout develop
git pull origin develop
git checkout -b feature/<short-topic>
```

When complete:

```bash
git push -u origin feature/<short-topic>
# Open PR: feature/<short-topic> -> develop
```

## Commands: Start a Release Branch

```bash
git checkout develop
git pull origin develop
git checkout -b release/vX.Y.Z
git push -u origin release/vX.Y.Z
```

Then:

1. Stabilize and validate release branch.
2. Open PR: `release/vX.Y.Z` -> `main`.
3. After merge, create and push tag `vX.Y.Z`.
4. Back-merge `main` into `develop`.

Back-merge commands:

```bash
git checkout develop
git pull origin develop
git merge origin/main
git push origin develop
```

## Commands: Start a Hotfix Branch

```bash
git checkout main
git pull origin main
git checkout -b hotfix/<short-topic>
git push -u origin hotfix/<short-topic>
```

Then:

1. Apply and validate the urgent fix.
2. Open PR: `hotfix/<short-topic>` -> `main`.
3. After merge, tag patch release `vX.Y.Z`.
4. Back-merge `main` into `develop`.
