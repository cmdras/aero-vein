# 7. M1 status set is Waiting|Cancelled; cancel is an idempotent manager invariant

Date: 2026-06-25

## Status

Accepted

## Context

§4 gives `Mission.status` a full lifecycle, but M1 reaches only the first slice. STORIES lets
a clinician cancel "a request that hasn't flown yet." We had to fix the M1 status set, where
the cancel guard lives, and how illegal/repeat cancels behave.

## Decision

- **Status set (M1):** exactly `Waiting | Cancelled`. No speculative later-lifecycle values
  are shipped; the enum grows with the milestones that produce the states.
- **Cancel guard:** a `MissionManager` invariant, not UI logic (§6.3). In M1 "hasn't flown
  yet" collapses to `status == Waiting`; later milestones widen it to "any pre-launch status."
- **Happy path:** cancelling a `Waiting` mission appends a `MissionCancelled` event, flips
  status to `Cancelled`, and drops it from the waiting projection.
- **Idempotent:** re-cancelling an already-`Cancelled` mission returns **200** and appends
  **no** second event — the action is treated as a no-op against the final state.
- **Unknown id:** **404**.

## Consequences

- The transition rule sits in one place (the manager), matching the human-approval-gate
  pattern, ready to widen rather than be rewritten when more statuses exist.
- Idempotency makes the cancel endpoint safe to retry and tolerant of a stale client (the
  button after a double-click) without manufacturing duplicate custody events.
- The custody stream stays clean: at most one `MissionCancelled` per mission.
