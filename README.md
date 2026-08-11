# Billing Management System

Billing Management System is a small Blazor WebAssembly and ASP.NET Core app for managing owner company details, customers, quotations, and invoices.

## Current MVP

- Owner company profile for document headers.
- Customer records for quotation/invoice selection.
- Quotations with line items and totals.
- Invoices created from quotations.
- HTML/print-style PDF download for quotations and invoices.

## Billing Workflow

1. Create the owner company profile used for seller details on billing documents.
2. Create a customer.
3. Create a quotation with one or more line items, prices, and tax rates.
4. Open the quotation and create its invoice.
5. Open either document and select **Print / Save PDF**.

Quotations snapshot seller details, customer details, currency, line items, prices, and tax
at creation time. Invoices copy that snapshot from their quotation, so later profile or
customer edits do not rewrite historical documents. One invoice can be created from each
quotation, and document numbers must be unique within their document type.

Out of scope for the current MVP: payments, dashboard, multi-role permissions, multi-company tenancy, receipt/credit/debit notes, advanced tax/currency workflows, and generic audit/versioning.

## Architecture

- `BillingManagement.Client`: Blazor WebAssembly UI.
- `BillingManagement.Api`: ASP.NET Core API with controllers and OpenAPI.
- `BillingManagement.Contracts`: shared request/response DTOs.
- `BillingManagement.Application.Abstractions`: CQRS interfaces, strict command/query results,
  and application ports.
- `BillingManagement.Application`: commands, queries, validators, results, and application services.
- `BillingManagement.Application.Handlers`: command/query handlers and the command validation pipeline.
- `BillingManagement.Domain`: business entities and rules.
- `BillingManagement.Infrastructure`: EF Core and SQL Server persistence.
- `BillingManagement.Migrator`: applies EF Core migrations.

### CQRS and Mediator

The API dispatches in-process commands and queries through
[Mediator](https://github.com/martinothamar/Mediator). The executable API owns source generation
and scans both the Application message assembly and Application.Handlers assembly.

- Every command implements the non-generic `ICommand` and returns `CommandResult`.
- `CommandResult` publicly exposes only `Success` and `Errors` and never carries a payload.
- Expected command failures use one `CommandErrorType` with one or more messages.
- Command validation failures are returned as HTTP 400 ProblemDetails under the `general` key.
- Queries implement `IQuery<TResult>`, where the result implements `IQueryResult`.

Customer writes use `ICustomerStore` with domain aggregates. Customer reads use
`ICustomerQueries` with read projections, direct lookup, filtering, and bounded pagination.
The customer list endpoint accepts `searchText`, `pageNumber`, and `pageSize` and returns
`X-Page-Number`, `X-Page-Size`, and `X-Total-Count` response headers.

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

The repository uses `.gitattributes` and `.editorconfig` together so .NET files are checked out
with CRLF consistently on Windows and Linux runners. Shell scripts and workflow YAML remain LF.
