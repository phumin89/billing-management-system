# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users

Small-business owners and office staff who prepare customer-facing billing documents and need a straightforward record of customers, quotations, and invoices.

## Product Purpose

Billing Management System turns owner-company and customer details into consistent quotations and invoices. Success means a user can set up their business once, create an accurate quotation, convert it into an invoice, and print either document without historical details changing later.

## Positioning

The product is a focused billing-document workspace: quotations snapshot seller, customer, currency, line-item, price, and tax details, and invoices inherit that snapshot instead of silently changing when profile or customer records are edited.

## Operating Context

Users maintain one owner company profile, manage customer records, draft quotations with line items and tax rates, create one invoice from each quotation, download either document as PDF, and track an invoice until it is paid or cancelled.

## Capabilities and Constraints

- Blazor WebAssembly client with an ASP.NET Core API, SQL Server, and EF Core persistence.
- One owner company profile supplies seller details for new billing documents.
- Customer records supply buyer details for new quotations.
- Quotations contain one or more line items, prices, tax rates, totals, currency, and unique document numbers.
- An invoice is created from a quotation and preserves that quotation's snapshot.
- Full invoice payment recording is supported. Partial payments, receipts, credit and debit notes, multi-company tenancy, roles, advanced tax or currency workflows, and generic audit history are outside the current MVP.
- Existing routes and document behavior must remain stable during visual redesign.

## Evidence on Hand

The repository contains working customer, company-profile, quotation, invoice, print, API, persistence, migration, and automated-test implementations. It does not contain approved brand assets, customer claims, testimonials, or production analytics; future work must not fabricate them.

## Product Principles

- Keep the document workflow obvious from customer setup through invoice creation.
- Make financial details easy to scan and verify before printing.
- Preserve historical truth through immutable document snapshots.
- Prefer calm, explicit operations over dashboard noise or speculative features.
- Keep the interface usable on desktop and mobile with visible keyboard focus.
