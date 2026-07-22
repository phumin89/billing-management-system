# API Instructions

Applies to the ASP.NET Core API host.

- API owns routing, authentication/authorization, model binding, HTTP status codes,
  ProblemDetails, OpenAPI, CORS, and request logging.
- Controllers stay thin: map contracts to commands/queries, dispatch, and translate results.
  No business rules or EF queries in controllers.
- Use centralized ProblemDetails/error mapping and preserve field-level validation errors.
- Register services explicitly. Constructor injection is the default.
- Keep `Program.cs` readable; extract registration extensions only after unrelated concerns
  accumulate.
- Keep middleware order conventional and deliberate.
- Prefer built-in ASP.NET Core capabilities before adding packages.
- CORS must include the actual separately hosted client origins used by Docker/local dev.
- API or service-wiring changes require focused tests, Docker API build/start, and a real
  HTTP smoke check. Contract changes alone do not require browser QA unless client behavior changes.
