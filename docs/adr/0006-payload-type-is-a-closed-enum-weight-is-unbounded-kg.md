# 6. Payload type is a closed enum; weight is unbounded kilograms

Date: 2026-06-25

## Status

Accepted

## Context

Intake captures `payloadType` and `weight`. §3.6 names four payload types — blood, organ,
lab sample, pharma — as *genuine process volatility* encapsulated in `PayloadPolicy`. We had
to decide whether types are a code enum or reference-data rows (as sites are, [[adr-0004]]),
and whether weight is bounded at intake.

## Decision

`payloadType` is a **closed code enum** `{ Blood, Organ, LabSample, Pharma }`, validated for
membership at intake. It is deliberately **not** a table: unlike a site (a data row that
appears without code), a new payload type is a new custody/cold-chain *process* that
inevitably touches `PayloadPolicy` code, so the enum is the honest shape. The OpenAPI
contract generates a matching client type.

`weight` is a **positive decimal in kilograms**, validated `> 0` at intake with **no upper
cap**. Capacity-vs-weight feasibility is a `PlanningEngine` constraint from M2 (§3.8), the
same boundary reasoning as the deadline ([[adr-0005]]).

## Consequences

- The enum-vs-table split mirrors the volatility: site sets change as data, type sets change
  as code. Modeling types as data would falsely imply you can add one without touching policy.
- Intake stays thin: validate enum membership and `weight > 0`; meaning and feasibility arrive
  with PayloadPolicy/PlanningEngine in M2.
- Risk accepted: M1 can record a weight no drone can carry. Fine — nothing flies in M1.
