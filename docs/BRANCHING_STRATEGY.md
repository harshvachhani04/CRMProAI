# Branching Strategy & Git Workflow

This document establishes the branching model, commit conventions, and release gating rules for **CRMPro AI**.

---

## 1. Branch Hierarchy

```
main (Production/Stable baseline)
  │
  ├── develop (Integration branch for current release)
  │     │
  │     ├── phase/P000-repo-baseline
  │     ├── phase/P001-adrs
  │     ├── phase/P002-local-dev-env
  │     └── ...
  │
  └── hotfix/* (Emergency fixes directly to main)
```

### Branch Rules
- **`main`**: Contains code that has passed all micro-phase exit gates, unit tests, integration tests, and architecture tests. Every commit on `main` is deployable.
- **`develop`**: Primary branch for accumulating micro-phases for the current milestone.
- **`phase/Pxxx-<slug>`**: Short-lived feature branch created for each specific micro-phase (e.g., `phase/P001-adrs`, `phase/P003-create-solution`). Merged only when its exit gate is verified.
- **No Direct Commits to `main`**: All code must arrive via pull request or verified branch merge.

---

## 2. Commit Message Standards (Conventional Commits)

Commit messages must follow the Conventional Commits specification with explicit reference to the micro-phase ID:

```
<type>(<phase-id>): <short imperative description>

[optional body describing changes, reasoning, and test evidence]

[optional footer]
```

### Types
- `feat`: A new user-facing or API feature (e.g., `feat(P027): implement lead creation API with validation`)
- `fix`: A bug fix (e.g., `fix(P010): correct tenant resolution claim parsing`)
- `docs`: Documentation only changes (e.g., `docs(P001): record ADR 001 to 008`)
- `test`: Adding or updating tests (e.g., `test(P011): add cross-tenant query isolation tests`)
- `refactor`: Code change that neither fixes a bug nor adds a feature (within phase scope)
- `chore`: Maintenance, dependencies, build scripts (e.g., `chore(P000): setup repo baseline`)

### Examples
```bash
git commit -m "feat(P009): implement Tenant aggregate root and tenant context abstraction"
git commit -m "test(P016): add permission matrix authorization unit tests"
git commit -m "docs(P000): document branching strategy and coding standards"
```

---

## 3. Micro-Phase Verification & Merge Loop

Before merging any `phase/Pxxx` branch into `develop` or `main`:

1. **Compilation Check**: `dotnet build` and `ng build` must succeed with zero errors.
2. **Automated Tests**: All unit tests, architecture tests, and integration tests applicable to the phase must pass.
3. **Tenant & Permission Checks**: Verification that no cross-tenant leakage or privilege escalation is possible.
4. **Documentation**: Phase status in `docs/PHASE_TRACKER.md` updated and exit gate confirmed.
5. **Clean Diffs**: No temporary files, debug logs, or plain-text secrets committed.
