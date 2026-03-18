# GitHub Setup for DayCareManagement

Run all commands from the `DayCareManagement` folder.

## 1) Initialize local Git repository

```bash
git init
git add .
git commit -m "chore: initialize DayCareManagement M1 scaffold"
```

## 2) Create remote repository (GitHub CLI option)

If `gh` is installed and authenticated:

```bash
gh auth status
gh repo create DayCareManagement --private --source . --remote origin --push
```

Use `--public` instead of `--private` if you want a public repository.

## 3) Fallback manual remote creation (without GitHub CLI)

1. Create an empty repository named `DayCareManagement` in GitHub web UI.
2. Copy the repository URL, then run:

```bash
git branch -M main
git remote add origin <YOUR_GITHUB_REPO_URL>
git push -u origin main
```

## 4) Verify CI workflow

After pushing to `main`, confirm `.github/workflows/dotnet-ci.yml` runs under the GitHub Actions tab.

## 5) Enable GitFlow Branches

Create and push the `develop` branch:

```bash
git checkout main
git pull origin main
git checkout -b develop
git push -u origin develop
```

Recommended branch protections (set in GitHub repository settings):

- Protect `main`: no direct pushes, require pull request, require status checks, require at least 1 approval.
- Protect `develop`: no direct pushes, require pull request, require status checks.
- Require signed commits if your team policy mandates it.
- Restrict who can dismiss reviews for `main` (maintainers only).

Use `main` for production-ready code and `develop` for integration of completed feature branches.
