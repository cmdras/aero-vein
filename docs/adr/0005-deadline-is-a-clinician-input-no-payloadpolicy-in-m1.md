# 5. The deadline is a clinician input; PayloadPolicy stays out of M1

Date: 2026-06-25

## Status

Accepted

## Context

STORIES says the clinician supplies "the latest useful arrival time." PITCH adds that
payload types have very different deadlines "and the system has to respect that." We had to
decide whether `latestArrival` is an absolute instant the human owns or a value derived from
payload type, and whether M1 validates it against per-type viability — which would mean
wiring the `PayloadPolicy` engine (§3.6) into intake.

## Decision

`latestArrival` is an **absolute UTC instant supplied by the clinician** — an input, not a
computed value. M1 validation is **thin**: reject only a missing/non-future instant. The
server clock (§6.1) owns "now"; time-to-deadline and the urgent-first ordering are computed
server-side.

`PayloadPolicy` is **kept out of M1 entirely**. "Respecting different deadlines" is satisfied
by honoring whatever instant the clinician sets and surfacing urgency. Enforcing per-type
viability windows is a planning concern (can we actually get this payload there in time?)
that belongs in `PayloadPolicy` feeding the `PlanningEngine` in M2.

## Consequences

- M1 wires no engine it does not need, keeping the §3.6 discipline (MissionManager reads
  PayloadPolicy → passes constraints to PlanningEngine) clean for when M2 introduces it.
- Intake stays a thin write path; the deadline is captured as truth and used only for sorting.
- Risk accepted: M1 can record a deadline that is physically infeasible for the payload type.
  That is fine — nothing flies in M1, and M2's planning is where feasibility is judged.
