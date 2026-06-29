# AGENTS.md

Guidance for agents working in this repository.

## Project Intent

This repository is for a billing management system built with:

- Blazor WebAssembly SPA client hosted separately
- ASP.NET Core backend API with controllers and Swagger/OpenAPI
- SQL Server persistence
- CQRS-ready application structure
- Docker-based local development

Until the solution is scaffolded, treat these as the default architecture decisions.

## Default Stack

- Use the latest stable .NET SDK available for the project. Prefer `net10.0` for new work unless the user pins another target.
- Use ASP.NET Core's modern minimal hosting model with `WebApplicationBuilder`.
- Use Blazor WebAssembly as the primary UI framework.
- Use SQL Server as the primary relational database.
- Use EF Core for relational persistence unless the user explicitly chooses another data access approach.
- Use built-in dependency injection, configuration, logging, validation, health checks, and ProblemDetails before adding third-party infrastructure.

## Architecture Decision

Use a local service-oriented modular architecture.

The project should be separated enough to test the API with Swagger/Postman and run the Blazor client independently, but it should not split command handlers, query handlers, or database access into separate network services unless there is a concrete deployment or scaling need.

PONYTAIL rule: separate by useful boundary, not by class name.

## Intended Solution Shape

Use this solution layout:

```text
src/
  BillingManagement.Client/                     # Blazor WebAssembly SPA
  BillingManagement.Api/                        # ASP.NET Core API, controllers, Swagger
  BillingManagement.Contracts/                  # API request/response DTOs shared by Client and Api
  BillingManagement.Application.Abstractions/   # CQRS interfaces and application contracts
  BillingManagement.Application/                # Commands, queries, handlers, validators
  BillingManagement.Domain/                     # Entities, value objects, domain rules
  BillingManagement.Infrastructure/             # EF Core, SQL Server, external integrations
  BillingManagement.Migrator/                   # Console app for EF migrations and local DB setup
tests/
  BillingManagement.UnitTests/                  # Domain and application handler tests
  BillingManagement.IntegrationTests/           # API, EF Core, migrations, SQL Server tests
docker/
  sqlserver/                                    # SQL Server local development assets if needed
docker-compose.yml
BillingManagement.slnx
```

Do not create extra projects beyond this list without a clear reason.

## Runtime Shape

Expected local runtime:

```text
Browser
  -> BillingManagement.Client
  -> BillingManagement.Api
  -> BillingManagement.Application.Abstractions
  -> BillingManagement.Application
  -> BillingManagement.Domain
  -> BillingManagement.Infrastructure
  -> SQL Server

BillingManagement.Migrator
  -> BillingManagement.Infrastructure
  -> SQL Server
```

Docker should support local development with:

```text
client
api
sqlserver
migrator
```

The migrator should be runnable on demand. Do not make every normal app start depend on automatic destructive migrations.

## Project Responsibilities

### BillingManagement.Client

- Blazor WebAssembly SPA.
- Contains Razor components, pages, client-side routing, UI state, and browser-facing validation.
- Calls `BillingManagement.Api` over HTTP.
- May reference `BillingManagement.Contracts`.
- Must not reference `Application`, `Domain`, `Infrastructure`, or EF Core.
- Must not contain secrets or privileged business rules.

### BillingManagement.Api

- ASP.NET Core Web API host.
- Exposes controllers for Swagger/Postman and the Blazor client.
- Owns HTTP concerns: routing, authentication, authorization, model binding, status codes, ProblemDetails, Swagger/OpenAPI, CORS, and request logging.
- References `Contracts`, `Application.Abstractions`, `Application`, and `Infrastructure` for dependency registration.
- Keeps controllers thin. Controllers translate HTTP requests into commands or queries and return HTTP responses.
- Must not contain business rules or EF Core query logic.

### BillingManagement.Contracts

- Contains API request and response DTOs shared by `Client` and `Api`.
- Keep contracts stable, explicit, and separate from EF entities.
- Do not put handlers, domain behavior, EF Core attributes, or infrastructure dependencies here.

### BillingManagement.Application.Abstractions

- Contains CQRS interfaces and application-level abstractions.
- Good examples:
  - `ICommandHandler<TCommand, TResult>`
  - `IQueryHandler<TQuery, TResult>`
  - `ICommandDispatcher`
  - `IQueryDispatcher`
  - application clock/current user abstractions when needed
  - repository or unit-of-work abstractions only when they simplify testing or boundaries
- Must not reference `Infrastructure`.
- Keep this project small.

### BillingManagement.Application

- Contains commands, queries, handlers, validators, and use-case DTOs.
- Organize by feature/use case, not by technical buckets.
- References `Application.Abstractions` and `Domain`.
- May depend on abstractions implemented by `Infrastructure`.
- Must not reference `Api`, `Client`, or EF Core-specific implementation unless intentionally accepted.
- Handlers should be in-process with the API. Do not create separate command/query services for local-only use.

Recommended feature structure:

```text
BillingManagement.Application/
  Invoices/
    CreateInvoice/
      CreateInvoiceCommand.cs
      CreateInvoiceHandler.cs
      CreateInvoiceResult.cs
      CreateInvoiceValidator.cs
    GetInvoiceDetail/
      GetInvoiceDetailQuery.cs
      GetInvoiceDetailHandler.cs
      InvoiceDetailDto.cs
```

### BillingManagement.Domain

- Contains business concepts: entities, value objects, domain services, domain errors, and domain rules.
- Must not reference `Api`, `Client`, `Application`, or `Infrastructure`.
- Keep domain objects free from HTTP and EF Core concerns unless there is a deliberate, documented reason.

### BillingManagement.Infrastructure

- Contains EF Core `DbContext`, SQL Server configuration, migrations, repositories, and external service implementations.
- References `Application.Abstractions` and `Domain`.
- Implements interfaces needed by `Application`.
- Owns persistence details. Do not leak EF entities as API contracts.

### BillingManagement.Migrator

- Console app for local database migration and setup.
- References `Infrastructure`.
- Runs EF Core migrations against SQL Server.
- Should be usable from Docker Compose and from the command line.
- Keep it boring: start, migrate, exit.

## Dependency Rules

Allowed dependency direction:

```text
Client -> Contracts

Api -> Contracts
Api -> Application.Abstractions
Api -> Application
Api -> Infrastructure

Application -> Application.Abstractions
Application -> Domain

Infrastructure -> Application.Abstractions
Infrastructure -> Domain

Migrator -> Infrastructure
```

Forbidden:

```text
Domain -> anything else
Client -> Application
Client -> Infrastructure
Contracts -> Application or Infrastructure
Application.Abstractions -> Infrastructure
Application -> Api or Client
Infrastructure -> Api or Client
```

## Blazor Guidance

- Build the UI as reusable, focused Razor components.
- Keep data access and business rules out of components.
- Call the API through typed HTTP clients or small client services.
- Treat browser state and client-side code as untrusted.
- Keep secrets and privileged work on the server.
- Prefer built-in forms, validation, routing, authorization, and component patterns.

Use Blazor WebAssembly SPA as a separate client project. Do not put server-side database access in the client.

## ASP.NET Core Guidance

- Keep `Program.cs` readable. Move feature registrations into extension methods once unrelated concerns accumulate.
- Register services explicitly in DI.
- Use constructor injection by default.
- Keep middleware order conventional: exception handling, HTTPS, static files, routing, authentication, authorization, endpoints.
- Use centralized error handling and ProblemDetails-style responses for API failures.
- Do not put business logic in endpoints, controllers, Razor components, or EF Core configuration.

## SQL Server And EF Core Guidance

- Store connection strings in configuration, user secrets, or secure deployment configuration. Never commit real secrets.
- Register the EF Core `DbContext` as scoped with SQL Server.
- Use migrations intentionally and keep them reviewable.
- Keep entities out of public API contracts and UI view models.
- Prefer explicit transactions for multi-step writes that must succeed or fail together.
- Use `IDbContextFactory<TContext>` only when the execution model requires it, such as long-lived Blazor component scopes or background work.

## Billing History And Snapshot Guidance

Use document snapshots for billing history.

When a billing document is issued, copy the customer and pricing details needed to reproduce that document into the document itself. Do not rely only on current `Customer`, `Product`, or `Address` records for old invoices, receipts, quotations, credit notes, or tax documents.

Example:

```text
Customer
  Id
  Name
  CurrentBillingAddress

Invoice
  Id
  CustomerId
  CustomerNameSnapshot
  BillingAddressSnapshot
  TaxIdSnapshot
  IssuedAt
```

If the customer later changes address, old invoices must still print the old address from `BillingAddressSnapshot`.

Snapshot these values when they appear on issued documents:

- customer display name
- billing address
- tax id
- branch information
- contact name, email, or phone when printed on the document
- item description
- unit price
- discount
- tax rate
- currency and exchange rate

Do not build generic entity versioning first. Add `CustomerVersion`, `AddressVersion`, audit log tables, or full revision history only when there is a concrete need to query or display change history outside issued documents.

Soft delete, audit fields, and optimistic concurrency are still useful:

- use soft delete for recoverable user mistakes
- use `CreatedAt`, `UpdatedAt`, and `DeletedAt` on persisted records where useful
- use row version/concurrency tokens for records users can edit
- use snapshots for historical document correctness

## CQRS-Ready Guidance

Design application features around use cases:

- Commands mutate state and return only what the caller needs.
- Queries read state and avoid side effects.
- Commands and queries are message objects. They must not execute themselves or directly call their own handlers.
- Dispatchers route commands and queries to the matching handler.
- Handlers should live in the application layer.
- Validation belongs near the command or query it validates.
- Domain rules belong in the domain layer, not in UI or persistence code.
- Infrastructure implements persistence and external service abstractions defined by the application layer.

Prefer a lightweight in-process CQRS style first. Do not add MediatR, messaging, event sourcing, a separate read database, or separate command/query services unless the project has a concrete need.

CQRS abstractions and CQRS implementations may be separated:

```text
BillingManagement.Application.Abstractions/
  Commands/
    ICommandHandler.cs
    ICommandDispatcher.cs
  Queries/
    IQueryHandler.cs
    IQueryDispatcher.cs

BillingManagement.Application/
  Invoices/
    CreateInvoice/
      CreateInvoiceCommand.cs
      CreateInvoiceHandler.cs
    GetInvoiceDetail/
      GetInvoiceDetailQuery.cs
      GetInvoiceDetailHandler.cs
```

This is good encapsulation. Splitting handlers into separate deployed apps is not required for Swagger/Postman testing.

Expected CQRS flow:

```text
Controller
  -> ICommandDispatcher
  -> ICommandHandler<TCommand, TResult>
  -> Domain / Infrastructure

Controller
  -> IQueryDispatcher
  -> IQueryHandler<TQuery, TResult>
  -> Infrastructure / Domain
```

## Docker Guidance

- Use Docker Compose for local development.
- Compose should run SQL Server and, when useful, the API and client.
- Provide a development build path, not only production images.
- Keep Dockerfiles simple and close to the projects they build.
- Use environment variables for local connection strings.
- Do not commit real passwords or secrets.
- Prefer a named Docker volume for SQL Server data.

## Clean Code And SOLID Guidance

Apply Clean Code and SOLID only where they reduce real complexity. Do not add abstractions just to satisfy a pattern.

- Use clear names that describe the business action: `CreateInvoiceCommand`, `GetCustomerBalanceQuery`, `InvoiceLine`.
- Keep classes focused on one reason to change.
- Keep controllers thin. Put use-case logic in command/query handlers.
- Keep Razor components focused on UI and browser interaction.
- Keep methods short enough to read without scrolling when practical, but do not split code into meaningless one-line wrappers.
- Prefer explicit DTOs at boundaries: API contracts, commands, query results, and persistence entities should not be the same type by accident.
- Validate at trust boundaries: API requests, commands, and user input.
- Return clear results or failures from application handlers. Do not hide expected business failures as generic exceptions.
- Use dependency inversion at architecture boundaries: `Application` defines what it needs; `Infrastructure` implements it.
- Use interfaces for cross-project boundaries or multiple implementations. Do not create an interface for every class.
- Prefer composition over inheritance. Use inheritance only when the domain relationship is real.
- Keep domain rules in `Domain`; keep persistence details in `Infrastructure`; keep HTTP details in `Api`; keep UI details in `Client`.
- Make invalid states hard to create with constructors, factory methods, value objects, or validation where useful.
- Avoid global mutable state and service locator patterns.
- Avoid premature generic helpers, base handlers, base controllers, and shared utility folders until duplication is real.

## TDD Guidance

Use TDD for production behavior changes.

- Write a failing test before implementation for new features, bug fixes, refactors, and behavior changes.
- Verify the test fails for the expected reason before writing production code.
- Write the smallest implementation that makes the test pass.
- Refactor only after the test is green.
- For bugs, add a failing regression test that reproduces the bug before fixing it.
- Prefer focused unit tests for domain rules and command/query handlers.
- Prefer integration tests for API behavior, EF Core persistence, migrations, and SQL Server behavior.
- Use real code in tests. Mock only external boundaries or hard dependencies.
- Do not add test-only methods or weaken production design just to make testing easier.
- Generated code, pure configuration, Docker files, and trivial documentation-only edits do not need TDD.

## Testing Expectations

- Add unit tests for domain rules and command/query handlers.
- Add integration tests for EF Core persistence, migrations, and critical API flows.
- Prefer deterministic tests with explicit seed data.
- Do not rely on a developer's local SQL Server for automated tests unless explicitly agreed. Prefer containers or a test-specific database strategy.

## Development Rules

- Always use PONYTAIL: prefer the simplest working solution, avoid speculative abstractions, avoid unnecessary dependencies, and keep diffs small.
- Always use CAVEMAN Max: write the shortest direct status and implementation notes that still preserve necessary facts.
- All agents must use PONYTAIL and CAVEMAN Max to reduce overbuilding and token/credit usage.
- Before implementation work, create or switch to a feature branch that starts with the Trello `Card Number` custom field when present, for example `#BMS-123-short-description`. Do not infer, renumber, or replace the Trello card number from the card title, URL slug, or sequence gaps.
- Implementation work must happen in the main local repository checkout on a local ticket branch unless the user explicitly approves a Codex worktree. Do not use Codex worktrees for implementation by default, because the user reviews in Visual Studio.
- When implementation work finishes, push the card branch and create a ready-for-review pull request before starting another implementation card.
- Keep changes small and scoped to the requested work.
- Follow existing project conventions once files exist.
- Do not introduce new libraries without explaining the need.
- Do not commit generated secrets, local database files, build outputs, or IDE-specific noise.
- Update this file when the actual architecture intentionally diverges from these defaults.

## Agent Coordination Rules

- When an agent finishes assigned work, it must update the Trello card and send a completion message back to the Project Manager session.
- When an agent is blocked, it must update the Trello card and send a blocked message back to the Project Manager session instead of waiting silently.
- Completion or blocked messages must include the Trello card link, final status, important output, verification performed, blockers or gaps, and the recommended next owner.
