# Billing Management System

Billing Management System is a small Blazor WebAssembly and ASP.NET Core app for managing owner company details, customers, quotations, and invoices.

## Current MVP

- Owner company profile for document headers.
- Customer records for quotation/invoice selection.
- Quotations with line items and totals.
- Invoices created from quotations.
- HTML/print-style PDF download for quotations and invoices.

Out of scope for the current MVP: payments, dashboard, multi-role permissions, multi-company tenancy, receipt/credit/debit notes, advanced tax/currency workflows, and generic audit/versioning.

## Architecture

- `BillingManagement.Client`: Blazor WebAssembly UI.
- `BillingManagement.Api`: ASP.NET Core API with controllers and OpenAPI.
- `BillingManagement.Contracts`: shared request/response DTOs.
- `BillingManagement.Application.Abstractions`: CQRS interfaces and app contracts.
- `BillingManagement.Application`: commands, queries, handlers, validation.
- `BillingManagement.Domain`: business entities and rules.
- `BillingManagement.Infrastructure`: EF Core and SQL Server persistence.
- `BillingManagement.Migrator`: applies EF Core migrations.

## Local Docker Setup

Prerequisites: Docker Desktop.

```powershell
docker compose up --build
```

Run migrations when needed:

```powershell
docker compose --profile tools run --rm migrator
```

Local URLs:

- Client: http://localhost:5080
- API: http://localhost:5081
- SQL Server: localhost,14333
- OpenAPI JSON in development: http://localhost:5081/openapi/v1.json

Default local SQL password comes from `BMS_DB_PASSWORD`; Docker Compose has a dev fallback. Do not use the fallback for production.

## Development Workflow

- Work from Trello cards with visible `#BMS-xxxx` in the title.
- Branch names start with the card number, for example `#BMS-0062-readme`.
- Keep changes small and scoped to one card.
- Open a PR for review when implementation is done.
- Run relevant build/tests before PR.
- Use Docker Compose verification for runtime, UI, API, DB, migration, or service wiring changes.

## CI

GitHub Actions runs on PRs and pushes to `master`:

- restore solution
- format gate
- frontend build
- backend/full solution build
- unit tests
- integration tests

Frontend unit tests are not configured yet.
