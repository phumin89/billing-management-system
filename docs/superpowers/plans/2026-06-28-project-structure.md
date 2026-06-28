# Project Structure Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Scaffold the billing management solution structure agreed in `AGENTS.md` and the project structure spec.

**Architecture:** Use a local service-oriented modular architecture. The Blazor WebAssembly client, ASP.NET Core API, SQL Server database, and migrator are runtime boundaries; application/domain/infrastructure projects are compile-time boundaries.

**Tech Stack:** .NET 10, ASP.NET Core Web API controllers, Blazor WebAssembly, EF Core SQL Server, xUnit, Docker Compose.

## Global Constraints

- Target `net10.0`.
- Keep command/query handlers in-process with the API.
- Do not add MediatR or messaging.
- Do not add behavior beyond scaffold wiring.
- TDD is not required for generated code, pure configuration, Docker files, or documentation-only edits.
- Verify with `dotnet build` and `dotnet test`.

---

## File Structure

- Create `BillingManagement.sln`.
- Create projects under `src/`.
- Create test projects under `tests/`.
- Create Docker files for `client`, `backend`, `database`, and `migrator` local development.
- Keep generated template structure recognizable.

### Task 1: Baseline Documentation

**Files:**
- Create: `docs/superpowers/specs/2026-06-28-project-structure-design.md`
- Create: `docs/superpowers/plans/2026-06-28-project-structure.md`
- Modify: `AGENTS.md`

**Interfaces:**
- Produces the project architecture baseline.

- [ ] **Step 1: Verify docs exist**

Run: `Test-Path AGENTS.md; Test-Path docs/superpowers/specs/2026-06-28-project-structure-design.md; Test-Path docs/superpowers/plans/2026-06-28-project-structure.md`

Expected: all values are `True`.

- [ ] **Step 2: Commit baseline docs**

Run:

```powershell
git add AGENTS.md docs/superpowers/specs/2026-06-28-project-structure-design.md docs/superpowers/plans/2026-06-28-project-structure.md
git commit -m "docs: define project architecture"
```

Expected: commit succeeds.

### Task 2: Scaffold Solution And Projects

**Files:**
- Create: `BillingManagement.sln`
- Create: `src/BillingManagement.Client/BillingManagement.Client.csproj`
- Create: `src/BillingManagement.Api/BillingManagement.Api.csproj`
- Create: `src/BillingManagement.Contracts/BillingManagement.Contracts.csproj`
- Create: `src/BillingManagement.Application.Abstractions/BillingManagement.Application.Abstractions.csproj`
- Create: `src/BillingManagement.Application/BillingManagement.Application.csproj`
- Create: `src/BillingManagement.Domain/BillingManagement.Domain.csproj`
- Create: `src/BillingManagement.Infrastructure/BillingManagement.Infrastructure.csproj`
- Create: `src/BillingManagement.Migrator/BillingManagement.Migrator.csproj`
- Create: `tests/BillingManagement.UnitTests/BillingManagement.UnitTests.csproj`
- Create: `tests/BillingManagement.IntegrationTests/BillingManagement.IntegrationTests.csproj`

**Interfaces:**
- Produces the solution and project shell.

- [ ] **Step 1: Generate projects**

Run the corresponding `dotnet new` commands with `--framework net10.0`.

- [ ] **Step 2: Add projects to solution**

Run `dotnet sln add` for every project.

- [ ] **Step 3: Add project references**

Wire references according to `AGENTS.md`.

- [ ] **Step 4: Build**

Run: `dotnet build`

Expected: build succeeds.

### Task 3: Add Minimal CQRS Abstractions

**Files:**
- Create: `src/BillingManagement.Application.Abstractions/Commands/ICommandHandler.cs`
- Create: `src/BillingManagement.Application.Abstractions/Commands/ICommandDispatcher.cs`
- Create: `src/BillingManagement.Application.Abstractions/Queries/IQueryHandler.cs`
- Create: `src/BillingManagement.Application.Abstractions/Queries/IQueryDispatcher.cs`

**Interfaces:**
- Produces CQRS interfaces referenced by future application handlers.

- [ ] **Step 1: Add interface files**

Use generic async interfaces with `CancellationToken`.

- [ ] **Step 2: Build**

Run: `dotnet build`

Expected: build succeeds.

### Task 4: Add Docker Development Shell

**Files:**
- Create: `docker-compose.yml`
- Create: `src/BillingManagement.Client/Dockerfile`
- Create: `src/BillingManagement.Api/Dockerfile`
- Create: `src/BillingManagement.Migrator/Dockerfile`
- Create: `.dockerignore`

**Interfaces:**
- Produces local Docker service definitions for client, backend, database, and migrator.

- [ ] **Step 1: Add Docker files**

Use development-friendly multi-stage Dockerfiles.

- [ ] **Step 2: Validate Compose config**

Run: `docker compose config`

Expected: config renders without errors.

### Task 5: Final Verification

**Files:**
- Modify as needed only to fix build or configuration errors.

**Interfaces:**
- Produces a verified scaffold.

- [ ] **Step 1: Build solution**

Run: `dotnet build`

Expected: build succeeds.

- [ ] **Step 2: Run tests**

Run: `dotnet test`

Expected: test run succeeds.

- [ ] **Step 3: Check git status**

Run: `git status --short`

Expected: only intended scaffold files are changed.

