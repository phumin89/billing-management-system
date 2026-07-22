# Client Instructions

Applies to the Blazor WebAssembly SPA.

- Reference Contracts only. Never reference Application, Domain, Infrastructure, or EF Core.
- Treat browser state and client validation as untrusted. Keep secrets and privileged
  business rules on the server.
- Call the API through typed HTTP clients or focused client services.
- Razor files are markup. Put non-trivial state, handlers, mapping, and API calls in a
  `.razor.cs` partial class.
- Use feature folders under `Pages/<Feature>` or `Components/<Feature>` when a feature
  contains more than one UI file. Preserve routes.
- Build reusable, focused components; do not put persistence or domain rules in components.
- Prefer built-in forms, validation, routing, and authorization patterns.
- Prefer component-scoped SCSS. Generated CSS must be deterministic in local build,
  Docker, and CI; do not rely on uncommitted local generation.
- Follow `DESIGN.md` and established tokens before inventing visual styles.
- UI changes require Docker client build and real browser verification. Visual changes
  need one relevant desktop and mobile screenshot, readable responsive layout, and a
  clean browser console.
- Preserve visible keyboard focus and reduced-motion behavior where motion exists.
