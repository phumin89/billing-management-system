# Project Structure Design

## Goal

Build a local billing management system with a separately hosted Blazor WebAssembly SPA, an ASP.NET Core API, SQL Server persistence, CQRS-ready boundaries, Docker-based development, Clean Code/SOLID guidance, and TDD for production behavior.

## Target Stack

- .NET 10 / ASP.NET Core 10
- Blazor WebAssembly SPA
- ASP.NET Core Web API with controllers and Swagger/OpenAPI
- SQL Server
- EF Core
- Docker Compose for local development
- xUnit for tests

## Architecture

Use a local service-oriented modular architecture, not full microservices.

The runtime services are:

```text
client    # Blazor WebAssembly SPA
backend   # ASP.NET Core API
database  # SQL Server
migrator  # EF Core migration runner, run on demand
```

The codebase is separated by useful boundaries:

```text
src/
  BillingManagement.Client/
  BillingManagement.Api/
  BillingManagement.Contracts/
  BillingManagement.Application.Abstractions/
  BillingManagement.Application/
  BillingManagement.Domain/
  BillingManagement.Infrastructure/
  BillingManagement.Migrator/
tests/
  BillingManagement.UnitTests/
  BillingManagement.IntegrationTests/
```

Command/query handlers stay in-process with the API. They are separated by project boundary for encapsulation, not deployed as separate services.

## Dependency Direction

Allowed:

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

## CQRS Design

Commands and queries are message objects. They do not execute themselves.

Expected flow:

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

CQRS abstractions live in `BillingManagement.Application.Abstractions`. Handler implementations live in `BillingManagement.Application`.

Do not add MediatR, messaging, event sourcing, a separate read database, or separate command/query services until there is a concrete need.

## Docker Design

Docker Compose should support local development with:

```text
client
backend
database
migrator
```

`database`, `backend`, and `client` are normal dev services. `migrator` runs on demand and exits.

## Quality Rules

- Always use PONYTAIL: simplest working solution, no speculative abstractions, no unnecessary dependencies.
- Always use CAVEMAN: direct, plain, terse implementation notes.
- Use Clean Code and SOLID only where they reduce real complexity.
- Use interfaces for cross-project boundaries or multiple implementations, not every class.
- Use TDD for production behavior changes.
- Generated code, pure configuration, Docker files, and documentation-only edits do not need TDD.

