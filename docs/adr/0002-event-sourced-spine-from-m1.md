# 2. Event-sourced spine stood up in Milestone 1

Date: 2026-06-25

## Status

Accepted; refined by [[adr-0009]]

## Context

ARCHITECTURE §4 commits to an event-sourced core: chain of custody is an immutable,
append-only `CustodyEvent` log (the facts), and live operational state is a rebuildable
Marten projection derived from it. Milestone 1 ("Request intake + the spine") is the first
slice. We had to decide how much of that machinery M1 builds now versus retrofits later:
full event-sourced spine, state-only CRUD with events deferred, or a hybrid (event write
model but read via live aggregation instead of an async projection).

## Decision

M1 stands up the **full event-sourced spine**. Submitting a request appends an event to a
Marten stream through the `CustodyEventStore`/`MissionStore` seams; the urgent-first waiting
list is a real Marten projection; cancel appends a further event. The `MissionManager`
orchestrates through the ResourceAccess seams even while thin.

## Consequences

- "The projection can't drift from the events" (§4) holds from request #1, because events
  are the source of truth from the start — no later retrofit.
- The append→project→read loop is proven end-to-end on the smallest possible slice, while
  there are few moving parts to get it wrong.
- M3's live board reuses the same async-projection mechanism, so M1 de-risks it.
- Cost: Marten configuration and Postgres wiring are paid up front in M1 rather than spread
  out. Accepted as a one-time cost on the right slice.
