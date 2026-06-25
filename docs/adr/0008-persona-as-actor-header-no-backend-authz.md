# 8. Persona travels as an actor header; no backend authorization in M1

Date: 2026-06-25

## Status

Accepted

## Context

§3.9 holds that personas are clients, not components, and that we build no real auth (persona
switch has "no business endpoint", §5). Yet §4's `CustodyEvent` carries an `actor` — events
must record who acted. We had to decide how persona reaches the backend and whether it gates
any action.

## Decision

- **Wire format:** the client sends the current persona on each request as an `X-Actor`
  header (e.g. `X-Actor: clinician`). The `MissionManager` stamps it as `actor` on every
  appended event (`MissionRequested`, `MissionCancelled`). The header *is* the auth stand-in's
  wire format — the seam through which real auth would later inject a verified identity.
- **Authorization:** **none in the backend**. The role switcher renders persona-appropriate
  screens (clinician → submit form; dispatcher → waiting list); the API accepts any action
  from any persona. Authorization arrives only with real auth (§7's trigger).

## Consequences

- Custody events get a real `actor` value now, with a place ready for verified identity later.
- We do not build the auth volatility §3.9 says isn't real yet; gating stays presentation, not
  a backend rule, so there is no fake security shape to tear out.
- The `X-Actor` header is trusted/unauthenticated by design in M1 — acceptable because this is
  a pilot stand-in, and the same seam is where real authentication slots in.
