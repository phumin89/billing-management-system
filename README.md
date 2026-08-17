# 🧾 Billing Management System

Billing Management System is a focused billing-document workspace for small
businesses. It helps an owner company maintain customer records, issue
quotations, convert accepted quotations into invoices, download printable PDFs,
and track invoice status from issuance through payment or cancellation.

The product is intentionally compact: it prioritizes clear document preparation,
stable historical snapshots, and a simple operational workflow over broad ERP or
accounting-suite features.

## ✨ Highlights

- Owner company profile used as the seller identity on new billing documents.
- Customer registry with searchable customer records.
- Quotation creation with line items, tax, currency, totals, and unique document
  numbers.
- One invoice created from each quotation, preserving the original quotation
  snapshot.
- Invoice dashboard, status filters, payment recording, cancellation, and recent
  activity.
- Server-generated PDF downloads for quotations and invoices.
- Docker Compose setup for the Blazor client, ASP.NET Core API, SQL Server, and
  EF Core migrator.
- CI checks for formatting, build, unit tests, and integration tests.

## 🔄 Billing Workflow

1. Create the owner company profile that appears on billing documents.
2. Add a customer record.
3. Create a quotation with line items, prices, and tax rates.
4. Review the quotation and create its invoice.
5. Download the quotation or invoice as a PDF.
6. Track the invoice until it is paid or cancelled.

Quotations snapshot seller details, customer details, currency, line items,
prices, and tax at creation time. Invoices copy that snapshot from their
quotation, so later edits to the owner profile or customer record do not rewrite
historical documents.

Current MVP constraints:

- One owner company profile.
- One invoice per quotation.
- Full-payment tracking only.
- No multi-company tenancy, roles, receipts, credit notes, debit notes, partial
  payments, advanced tax workflows, or generic audit/version history.

## 🧰 Technology Stack

- .NET 10
- Blazor WebAssembly
- ASP.NET Core Web API
- EF Core
- SQL Server 2022
- Docker Compose
- Mediator for in-process CQRS dispatch
- Prettier and `dotnet format` for formatting gates

## 🏗️ Architecture

The solution uses a layered architecture with dependencies pointing toward the
business domain. This keeps business rules independent from the UI, database,
and framework-specific concerns.

| Layer          | Responsibility                                    |
| -------------- | ------------------------------------------------- |
| Client         | Blazor WebAssembly user interface                 |
| API            | HTTP endpoints, request mapping, and PDF delivery |
| Application    | Use cases, commands, queries, and validation      |
| Domain         | Business entities, calculations, and invariants   |
| Infrastructure | EF Core persistence and SQL Server integration    |

The API dispatches commands and queries in process through
[Mediator](https://github.com/martinothamar/Mediator). Shared contracts keep the
client separated from internal application and persistence implementations.

- Commands implement the non-generic `ICommand` contract and return
  `CommandResult`.
- Expected command failures are represented with `CommandErrorType` and mapped to
  ProblemDetails responses.
- Queries implement `IQuery<TResult>`, where the result implements
  `IQueryResult`.
- Customer writes use domain aggregates through `ICustomerStore`.
- Customer reads use query projections through `ICustomerQueries`, including
  search, direct lookup, and bounded pagination.

## 🚀 Local Development

Prerequisites:

- Docker Desktop
- .NET 10 SDK
- Node.js and npm for formatting tools

Install frontend tooling:

```powershell
npm install
```

Run the full local stack:

```powershell
docker compose up --build
```

The Compose stack starts SQL Server, waits for it to become healthy, applies EF
Core migrations, starts the API after migration succeeds, and starts the client
after the API is ready.

Local URLs:

| Service       | URL                                   |
| ------------- | ------------------------------------- |
| Client        | http://localhost:5080                 |
| API           | http://localhost:5081                 |
| API liveness  | http://localhost:5081/health/live     |
| API readiness | http://localhost:5081/health/ready    |
| OpenAPI JSON  | http://localhost:5081/openapi/v1.json |
| SQL Server    | localhost,14333                       |

The development SQL password comes from `BMS_DB_PASSWORD`. Docker Compose
includes a local fallback for convenience; do not use the fallback password in
production.

## ✅ Verification

Run formatting checks:

```powershell
npm run check:format
```

Apply formatting:

```powershell
npm run format
```

Run the automated test suites:

```powershell
dotnet test BillingManagement.slnx
```

Build the solution:

```powershell
dotnet build BillingManagement.slnx
```

For changes that affect runtime wiring, database behavior, migrations, API
integration, or UI behavior, verify with Docker Compose as well.

## ⚙️ Continuous Integration

GitHub Actions runs on pull requests and pushes to `master`.

The pipeline checks:

- solution restore
- formatting gates
- frontend build
- full solution build
- unit tests
- integration tests

Blazor component and markup behavior are covered by the .NET test projects. The
repository does not use a separate JavaScript test runner because it does not
contain application JavaScript.

## 📦 Production Deployment

Create a `.env` file from `.env.example`, set a production SQL Server password,
and run:

```powershell
docker compose -f docker-compose.yml -f docker-compose.production.yml up -d --build
```

The production override removes direct host exposure for the API and SQL Server.
The client proxies `/api` requests to the internal API service.

Before upgrades, back up the named SQL Server and company-media volumes.
Authentication and multi-user authorization are outside the current MVP, so
production deployments should be placed behind an authenticated private network
or access gateway.

## 🤝 Development Workflow

- Keep changes small, reviewable, and tied to one product or engineering goal.
- Use descriptive branch names that include the card or task identifier when one
  exists.
- Open a pull request when implementation and local verification are complete.
- Keep formatting clean before review.
- Use Docker Compose evidence for runtime, UI, API, database, migration, or
  service-wiring changes.

## 🧹 Repository Hygiene

The repository uses `.gitattributes` and `.editorconfig` together so .NET files
are checked out with consistent CRLF line endings on Windows and Linux runners.
Shell scripts and workflow YAML remain LF.

Do not commit secrets, local databases, build output, IDE noise, or generated
artifacts that are not part of the intended product change.
