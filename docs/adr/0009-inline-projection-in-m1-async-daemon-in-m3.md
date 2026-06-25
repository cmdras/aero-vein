# 9. Waiting-list projection is Inline in M1; async daemon arrives in M3

Date: 2026-06-25

## Status

Accepted (refines [[adr-0002]])

## Context

[[adr-0002]] stood up the event-sourced spine in M1 and loosely described the waiting list as
an "async projection." The hard requirement for M1 is **read-your-writes**: a clinician
submits, then the waiting list must immediately show the new mission. Marten offers three
projection lifecycles — Inline (updates in the same transaction as the append; strong
consistency), Async (background projection daemon; eventual consistency, needs
`QueryForNonStaleData<T>(timeout)` for read-your-writes), and Live (aggregated on read).

## Decision

The M1 waiting-list projection uses the **Inline** lifecycle. The **async projection daemon**
is introduced in **M3** for the live operations board, where write throughput, decoupling, and
push genuinely demand it. This refines — does not reverse — ADR-0002: the event-sourced spine,
events-as-truth, and rebuildable projections all still hold; only the lifecycle detail changes.

## Consequences

- Read-your-writes is correct by construction: the submit→list demo works with no
  `QueryForNonStaleData` waits and no eventual-consistency races to test.
- No projection daemon operational surface in M1; its justification is an M3 concern.
- Cheap pivot: lifecycle is a registration-time choice (`ProjectionLifecycle.Inline` →
  `.Async`); flipping it in M3 is a one-line change and the projection rebuilds either way.
- Trade-off accepted: the daemon is exercised later (M3) rather than de-risked in M1.
