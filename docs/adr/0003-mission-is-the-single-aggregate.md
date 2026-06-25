# 3. Mission is the single aggregate; "Request" is its waiting stage

Date: 2026-06-25

## Status

Accepted

## Context

STORIES speaks entirely in "request" (submit, cancel, waiting list), while ARCHITECTURE §4
models a single `Mission` entity with a `status` spanning `waiting → … → delivered`, and §5
splits the surface into `/requests` (M1) and `/missions/{id}` (M3+). We had to decide
whether a Request and a Mission are one aggregate or two — i.e. whether assignment spawns a
distinct Mission stream from a prior Request stream.

## Decision

There is **one aggregate, the Mission**, with **one Marten event stream** per mission keyed
by `missionId` from its birth event. "Request" is the ubiquitous-language name for a Mission
in its **waiting stage** (submitted, not yet assigned/flown), not a separate entity. The
`/requests` vs `/missions/{id}` URL split is ergonomic surface naming over the same id.

The stream's birth event is **`MissionRequested`**, named from the mission's point of view
rather than `RequestSubmitted`, so the custody stream reads as one coherent story.

## Consequences

- Custody history (§4) is a single unbroken stream per mission — no id boundary to reconcile
  and no cross-stream drift, which two aggregates would have introduced.
- The `status` field carries the lifecycle; `waiting` is just its first value.
- Surface naming can stay user-facing (`/requests`) without implying a second entity.
- Term recorded in CONTEXT.md: Mission, Request, MissionRequested.
