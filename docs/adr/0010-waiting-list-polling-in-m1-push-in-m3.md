# 10. Waiting list is polled in M1; push arrives in M3

Date: 2026-06-25

## Status

Accepted

## Context

The Inline projection ([[adr-0009]]) keeps the server current the instant a write commits,
but a dispatcher's browser still needs to learn about another persona's submission. §5 leaves
the live-update transport (SSE vs WebSocket vs polling) an open decision scoped to M3. M1
needs *some* freshness mechanism without pulling that decision forward.

## Decision

In M1 the client **polls** `GET /requests?waiting` on a short interval (~5s) while the waiting
list is open, plus an immediate refetch after the acting user's own submit/cancel. The
interval is a **stopgap** that M3's push channel replaces — the endpoint stays, only the
data source swaps.

The waiting list sorts ascending by [[time-to-deadline]]; ties (equal `latestArrival`) break
by submission time (`MissionRequested.at`) ascending, giving a deterministic, stable order
that does not jitter between polls.

## Consequences

- M1 stays self-contained and demoable (two browser personas; submissions appear within
  seconds) without committing to the M3 transport choice.
- The acting user sees their own change immediately via the post-mutation refetch.
- Polling is wasteful at scale and laggy by up to the interval — both acceptable for the pilot
  and both eliminated by the M3 push channel.
