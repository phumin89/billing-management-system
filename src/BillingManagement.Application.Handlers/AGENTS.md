# Application Handler Instructions

Applies to command/query handlers and pipeline behaviors.

- Mirror the feature folders and namespaces from `BillingManagement.Application` so a message
  and its handler remain easy to locate across the assembly boundary.
- Handlers orchestrate one use case. Keep business invariants in Domain, message validation in
  Application, persistence behind Application.Abstractions ports, and HTTP mapping in Api.
- Depend only on Application, Application.Abstractions, Domain, and required framework
  abstractions. Never reference Api, Client, Contracts, Infrastructure, or EF Core.
- Keep `Mediator.Abstractions` here, but keep `Mediator.SourceGenerator` and `AddMediator`
  configuration in the executable Api composition root. Application must remain usable without
  handler implementations.
- Keep handlers block-bodied, flat, focused, cancellation-aware, and explicit about expected
  failures through command/query result contracts.
- Command handlers implement `ICommandHandler<TCommand>` and return only `CommandResult`.
  Validation behavior keeps Mediator's generator-compatible `<TCommand, TResponse>` shape; the
  command abstraction guarantees that its response is `CommandResult`, so keep any required
  response cast confined to that behavior and covered by end-to-end dispatch tests.
- Do not add forwarding services around `ISender`, generic base handlers, reflection-based
  dispatch, remote messaging, or one project per handler without a demonstrated need.
