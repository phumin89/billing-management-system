# AGENTS.md

Repository-wide instructions for the Billing Management System. Global coding
rules still apply. Before changing files below a scoped directory, read its
nearest `AGENTS.md` as well.

## Scoped Instructions

| Scope | Additional instructions |
| --- | --- |
| `src/**` | `src/AGENTS.md` |
| `src/BillingManagement.Application.Handlers/**` | `src/BillingManagement.Application.Handlers/AGENTS.md` |
| `src/BillingManagement.Client/**` | `src/BillingManagement.Client/AGENTS.md` |
| `src/BillingManagement.Api/**` | `src/BillingManagement.Api/AGENTS.md` |
| `src/BillingManagement.Infrastructure/**` | `src/BillingManagement.Infrastructure/AGENTS.md` |
| `tests/**` | `tests/AGENTS.md` |

Do not create `AGENTS.override.md`; scoped rules should extend repository rules.

## Engineering Practice

- Prefer correct, simple, explicit, readable code over clever or speculative architecture.
- Keep the happy path flat with guard clauses and early returns. Avoid `else` after a
  terminating statement and review code that reaches three indentation levels.
- Use block-bodied methods. Expression-bodied properties and simple lambdas remain acceptable.
- Keep methods focused, at one level of abstraction, and normally below 20 lines. Extract
  named operations when a block or LINQ pipeline mixes multiple concerns.
- Use intent-revealing domain names. Avoid vague `data`, `info`, `manager`, `processor`,
  `helper`, `util`, and `common` names unless their tiny local scope makes intent obvious.
- Prefer parameter objects when four or more values form one concept. Avoid boolean flags,
  output parameters, hidden side effects, and long primitive parameter lists.
- Commands change state; queries answer questions. Do not combine both responsibilities.
- Every application command implements the non-generic `ICommand` and returns the single
  concrete `CommandResult`. Its only public instance properties are `Success` and `Errors`;
  do not add payloads, self-referential result generics, or hidden validation state.
- Domain objects hide state and protect business rules. Requests, responses, records, and
  persistence projections are plain data structures and must not masquerade as domain objects.
- Use small, capability-focused interfaces. Add abstractions only for real boundaries or
  demonstrated variation; prefer small duplication over the wrong abstraction.
- Validate expected input at system boundaries and represent expected business failures with
  result types. Reserve exceptions for exceptional or invalid internal states; never swallow them.
- Keep controllers thin, business rules in Domain/Application, and framework/database details
  in outer adapters. Dependencies must point inward.
- Use readable LINQ. Split long pipelines into named stages when they combine filtering,
  ordering, projection, paging, mutation, side effects, or error handling.
- Tests follow Arrange/Act/Assert, describe behavior, and remain fast, independent, repeatable,
  and self-validating. Use TDD for behavior changes and tricky rules, not mechanical wiring.
- Improve touched code safely, but do not mix the requested change with unrelated broad cleanup.
- Follow `.editorconfig` line endings and never leave mixed endings in one file. .NET source and
  project files use CRLF; do not impose CRLF on shell scripts or other Unix-executed files.

## Product And Stack

- Blazor WebAssembly SPA, ASP.NET Core API, SQL Server, EF Core, and Docker Compose.
- Use the latest stable project SDK; new projects default to `net10.0` unless pinned.
- Prefer built-in DI, configuration, logging, validation, health checks, and ProblemDetails.
- Keep the application modular and in-process. Do not split handlers or persistence into
  network services without a concrete deployment need.
- Do not create projects beyond the current solution shape without approval.

```text
src/
  BillingManagement.Client
  BillingManagement.Api
  BillingManagement.Contracts
  BillingManagement.Application.Abstractions
  BillingManagement.Application
  BillingManagement.Application.Handlers
  BillingManagement.Domain
  BillingManagement.Infrastructure
  BillingManagement.Migrator
tests/
  BillingManagement.UnitTests
  BillingManagement.IntegrationTests
```

Dependency direction:

```text
Client -> Contracts
Api -> Contracts, Application.Abstractions, Application, Application.Handlers, Infrastructure
Application.Abstractions -> Domain
Application -> Application.Abstractions, Domain
Application.Handlers -> Application, Application.Abstractions, Domain
Infrastructure -> Application.Abstractions, Domain
Migrator -> Infrastructure
```

Forbidden: `Domain` referencing another project; `Client` referencing Application,
Application.Handlers, Domain, Infrastructure, or EF Core; Contracts referencing Application,
Application.Handlers, or Infrastructure;
Application referencing Application.Handlers, Api, or Client; Application.Handlers referencing
Api, Client, or Infrastructure; Infrastructure referencing Api or Client.

## Delivery Workflow

- Work one Trello card at a time. One card has one active owner.
- Trello numbers come from `Card Numbers by Reenhanced`. The title must contain the
  visible number as `#BMS-123 Task name` before Developer routing.
- Never infer a number from the card URL, sequence, or nearby cards. Missing number is
  a hard stop: comment once, leave/move the card blocked, then stop.
- Before implementation, use the main local checkout and switch to a local branch that
  starts with the exact card number, for example `#BMS-123-short-description`.
- Do not use Codex worktrees unless the user explicitly approves one.
- Preserve user changes and unrelated dirty files. Never reset or revert them.
- Keep the diff limited to the card. Avoid unrelated cleanup, renames, formatting,
  dependency updates, or generated-file churn.
- When implementation is complete, push and open a ready-for-review PR, never a draft.
  Do not start another implementation card before handoff.
- Default to autonomous approval inside accepted scope. Ask only for genuine product
  decisions, blockers, destructive actions, secrets, paid installs, or scope expansion.

## Verification Routing

- C# changes: `dotnet format BillingManagement.slnx --verify-no-changes` plus focused
  tests and build.
- Runtime, API, database, migration, or service-wiring changes: Docker Compose evidence.
- UI behavior: real browser check. Visual changes require relevant desktop and mobile
  screenshots; DOM-only evidence is insufficient.
- Documentation/instruction-only changes: validate structure, scope, links, and diff;
  do not run build, Docker, or browser without a runtime reason.
- Reuse evidence from the same revision. Do not rerun expensive checks without new code.
- If a required tool is unavailable, report the exact verification gap.

## Repository Conventions

- Use one top-level C# type per `.cs` file. Private tiny nested helpers are the exception.
- Preserve architecture boundaries and existing conventions.
- Never commit secrets, local databases, build output, or IDE noise.
- Do not introduce a library without explaining the concrete need.
- Update the relevant scoped instruction file when architecture intentionally changes.

## Roles And Handoffs

- PM owns priority, routing, approval, and cross-role handoff; it does not implement
  feature code unless explicitly requested.
- BA owns scope, business rules, acceptance criteria, and necessary ticket splitting.
- Designer owns UX flow, screen behavior, visual states, and handoff notes.
- Developer implements only an approved card routed to Developer.
- QA independently verifies and accepts/rejects; it does not fix code unless routed.
- Every completion or blocker requires one concise Trello comment and PM callback:
  status, card/PR link, decisive output, verification, blocker/gap, next owner.
- Do not include routine model, skill, or mode confirmations in callbacks.

## Credit Discipline

- Prefer the main session for small or sequential work. Use at most one agent unless
  independent parallel work materially saves time.
- Never forward full conversation history when a compact card brief is enough.
- Use the cheapest capable model; reserve frontier/high-reasoning models for genuinely
  complex implementation or debugging.
- One verification owner and one verification pass per unchanged revision.
- Stop after one repeated tooling failure and report the blocker; do not poll or retry
  indefinitely.
- Avoid broad Trello reads. Prefer direct card URLs and cached list/card identifiers.
- Trello comments: at most 10 lines unless writing a BA specification.
- QA result: PASS/FAIL plus at most eight high-signal bullets; detailed logs only for
  failures or disputed evidence.
- Keep PM callbacks and command summaries short. Do not repeat Trello content in chat.
- Stop when meaningful progress is blocked; do not fill time with backlog grooming.
