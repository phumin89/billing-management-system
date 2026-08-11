# API Instructions

Applies to the ASP.NET Core API host.

- API owns routing, authentication/authorization, model binding, HTTP status codes,
  ProblemDetails, OpenAPI, CORS, and request logging.
- Controllers stay thin: map contracts to commands/queries, dispatch, and translate results.
  No business rules or EF queries in controllers.
- Use centralized ProblemDetails/error mapping. Command validation is intentionally returned
  under the general validation key because `CommandResult` does not carry field metadata. Do not
  add a hidden field-error side channel; preserve field errors only for APIs whose explicit
  non-command result contract supports them.
- Register services explicitly. Constructor injection is the default.
- Reference Application for messages and Application.Handlers only for composition-root
  registration. Controllers dispatch through `ISender`; they never construct handlers.
- The executable Api owns `Mediator.SourceGenerator` and `AddMediator` configuration. Scan both
  the Application message assembly and Application.Handlers implementation assembly explicitly.
- Keep `Program.cs` readable; extract registration extensions only after unrelated concerns
  accumulate.
- Keep middleware order conventional and deliberate.
- Prefer built-in ASP.NET Core capabilities before adding packages.
- CORS must include the actual separately hosted client origins used by Docker/local dev.
- API or service-wiring changes require focused tests, Docker API build/start, and a real
  HTTP smoke check. Contract changes alone do not require browser QA unless client behavior changes.
