# 11. Domain invariants live in the manager; errors are 400 ProblemDetails

Date: 2026-06-25

## Status

Accepted

## Context

M1's validation splits into well-formedness (parseable input: required fields, positive
numeric weight, known `payloadType`, valid `latestArrival`) and domain invariants (origin and
destination both exist and differ; `latestArrival` is in the future). We had to decide where
each tier lives and what the wire error contract is.

## Decision

- **Placement.** Well-formedness is validated at the **API edge** (DTO/model validation in the
  controller) — cheap fast-fail needing no domain knowledge. **Domain invariants live in the
  `MissionManager`**, never in the controller or the React client, per §6.3 (business
  invariants live in the manager). M2's planning leans on the same manager.
- **Error contract.** Every error body is RFC 7807 **ProblemDetails** (ASP.NET Core native),
  carrying a stable machine-readable code so the client maps it to a field message.
- **Status codes.** **400** for all validation failures (malformed *and* business-rule). **404**
  for cancel of an unknown mission id. Cancel of an already-cancelled mission stays **200**
  ([[adr-0007]]).

## Consequences

- Invariants sit in one place, reusable by later milestones, not duplicated across UI/controller.
- A single 400 avoids the 400-vs-422 bikeshed; the body's error code does the disambiguation the
  client needs. Trade-off: clients must read the code, not just the status, to tell malformed
  from semantically-invalid.
