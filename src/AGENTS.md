# Source Instructions

Applies to all projects under `src/`. Read a project-specific `AGENTS.md` when present.

## Layer Ownership

- `Contracts`: stable transport DTOs only. No handlers, domain behavior, EF attributes,
  or infrastructure dependencies.
- `Application.Abstractions`: small CQRS and boundary interfaces. Never reference Infrastructure.
- `Application`: commands, queries, handlers, validators, and use-case results organized
  by feature. Never reference Api, Client, or EF implementation details.
- `Domain`: entities, value objects, domain services, errors, and invariants. It must not
  reference other solution projects or framework-specific HTTP/SQL/UI concerns.
- `Infrastructure`: EF Core, SQL Server, repositories, migrations, and external adapters.
- `Migrator`: start, migrate, exit. It references Infrastructure and remains usable from
  command line and Docker Compose.

## CQRS

- Commands mutate; queries read without side effects.
- Messages never execute themselves. Dispatchers route to in-process handlers.
- Controllers translate HTTP to commands/queries; handlers orchestrate use cases.
- Validation belongs near its command/query. Domain invariants remain in Domain.
- Application defines required boundaries; Infrastructure implements them.
- Do not add MediatR, messaging, event sourcing, separate read stores, or remote handler
  services without a demonstrated need.
- Keep abstractions concrete and purposeful. Avoid generic base handlers/controllers/helpers.

## Domain And Documents

- Make invalid states difficult to create with factories, constructors, or value objects
  when rules justify them.
- Issued billing documents must snapshot printed customer, address, tax, item, price,
  discount, tax, currency, and exchange-rate data. Historical documents must not change
  when live customer/product records change.
- Do not build generic versioning or revision history before a concrete query/display need.
- Use soft delete, audit timestamps, and concurrency tokens only where their behavior is useful.

## C# And Changes

- One top-level class, record, struct, interface, or enum per file.
- Keep methods focused; use guard clauses and explicit expected-failure results.
- Constructor injection is the default for external dependencies.
- Keep framework-specific annotations out of Domain.
- TDD is required for behavior changes and bug fixes. Pure generated code, docs, and
  mechanical configuration are exempt.
- Run formatter, focused tests, and build once per changed revision.
