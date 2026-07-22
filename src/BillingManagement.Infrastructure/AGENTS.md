# Infrastructure Instructions

Applies to EF Core, SQL Server, repositories, and migrations.

- Infrastructure implements Application boundaries and references Domain; never Api or Client.
- Persistence models/entities must not leak as public API contracts or UI models.
- Register `DbContext` as scoped. Use `IDbContextFactory` only when the execution model needs it.
- Keep connection strings in configuration, user secrets, or deployment settings. Never
  commit real credentials.
- Keep mappings explicit: required/nullability, lengths, indexes, constraints, concurrency,
  and normalization must agree with domain/application rules.
- Reads that expect a singleton or stable order must be deterministic.
- Use explicit transactions for multi-step writes that must commit atomically.
- Migrations must be intentional, reviewable, safe for existing data, and paired with the
  model snapshot. Do not silently destroy or rewrite data.
- Database or migration changes require focused SQL integration tests, Docker SQL Server,
  migrator execution, and schema/history verification. Verify fresh and existing database
  paths when the migration behavior differs.
- Do not weaken a database invariant merely to make an application test pass.
