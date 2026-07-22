# Test And QA Instructions

Applies to unit and integration tests.

- Test behavior and invariants, not private implementation details.
- Use clear condition/action/result names and Arrange/Act/Assert structure.
- Unit-test Domain rules, validators, and handlers with real collaborators when cheap.
- Integration-test HTTP contracts, EF mappings, migrations, and SQL Server behavior at
  meaningful boundaries.
- Mock external or hard boundaries, not simple domain objects.
- Tests must be deterministic with explicit data. Do not rely on a developer's local SQL
  Server; use the repository's container/test database strategy.
- Bug fixes start with a regression test that fails for the reported reason.
- Keep generated EF migration files out of style-noise enforcement where configured, but
  verify their schema intent.
- QA verifies independently and does not modify production code unless explicitly routed.
- Reuse passing CI/developer evidence from the same commit; independently retest the card's
  highest-risk behavior instead of repeating the entire matrix.
- UI acceptance requires a real browser and screenshot evidence; DOM metrics alone are not
  visual QA. Capture only the states and viewports affected by the diff.
- Report PASS/FAIL first, then up to eight decisive bullets, blockers, and next owner.
