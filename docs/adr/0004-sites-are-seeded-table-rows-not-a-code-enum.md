# 4. Sites are seeded table rows behind a seam, not a code enum

Date: 2026-06-25

## Status

Accepted

## Context

A request needs `originSite` and `destSite`. The Mechelen pilot is a contained set of known
hospital/lab sites (PITCH), so M1 needs a fixed catalog to populate the intake dropdown and
to validate origin/dest. No M1 story asks to *manage* sites. The open question was whether
adding site CRUD later (e.g. "onboard a partnered location") would be costly if we start with
a fixed catalog — i.e. is the simple choice a trap.

## Decision

The site catalog is **seed data in a Postgres table** (`id`, `name`; pads deferred to M5)
reached through a `SiteAccess` resource seam (§3.7). The initial Mechelen sites are seeded
rows. Sites are explicitly **not** modelled as a shared client/server code enum or in-memory
constant. M1 builds no site-management UI or write endpoints — read/list only.

## Consequences

- Adding CRUD later is **additive, not structural**: new `POST/PATCH/DELETE /sites` endpoints
  plus a small admin screen over the same rows. M1 intake is untouched — it references sites
  by id and a new row simply appears in the dropdown. Simulating "onboard a partner" is a
  one-row insert.
- Referential integrity is free now: validate that origin/dest exist and differ.
- Had we used a code enum, "add a location" would mean a code change + redeploy with no table
  to write into — the trap we avoided.
- This rides the `SiteAccess` boundary §7 already anticipates, so the deferral is honest debt.
