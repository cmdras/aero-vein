# AeroVein — Context

> Domain glossary and shared language for AeroVein. This complements (does not
> restate) `docs/ARCHITECTURE.md` (why the system is shaped this way) and `docs/STORIES.md`
> (what we're building). Terms are added **lazily** — when a word actually gets resolved in
> a `grill-with-docs` session, not upfront. See `docs/agents/domain.md`.

## Ubiquitous language

### Mission
The single aggregate at the heart of the system: one payload's journey from request to
delivery, with an immutable custody history. One Marten event stream per Mission, born at
submission and keyed by `missionId` for its whole life. A Mission carries `payloadType`,
`weight`, `originSite`, `destSite`, `latestArrival`, and `status`. See [[adr-0003]].

### Request
Not a separate entity — it is **a Mission in its waiting stage** (submitted, not yet
assigned or flown). "Submit a request" / "cancel a request" / "waiting requests" all act on
a Mission; the `/requests` surface is ergonomic naming over the same id. See [[adr-0003]].

### Site
A known origin/destination in the Mechelen pilot (hospital or lab). Reference data: `id`,
`name` (pads deferred to M5). Sites are a **seeded catalog of table rows** reached through a
`SiteAccess` seam — not a code enum — so the intake form references them by id and a future
"onboard a partner location" feature is additive CRUD over the same rows. See [[adr-0004]].

### Payload type
One of a **closed set** — `Blood`, `Organ`, `LabSample`, `Pharma` (§3.6). Modelled as a code
enum, not a table, because each type implies a distinct custody/cold-chain *process* in
PayloadPolicy — adding one is a code change, unlike adding a [[site]]. M1 validates membership
and stores it; PayloadPolicy gives it meaning from M2. See [[adr-0006]].

### Weight
The payload weight in **kilograms**, a positive decimal supplied at intake. M1 validates
`> 0` only; capacity-vs-weight feasibility is a PlanningEngine constraint from M2 (§3.8), so
no upper cap is enforced at intake. See [[adr-0006]].

### Waiting list
The dispatcher's single list of all `Waiting` missions, sorted ascending by
[[time-to-deadline]] (most urgent first); ties (equal `latestArrival`) break by submission
time (`MissionRequested.at`) ascending, giving a stable, jitter-free order between polls. It
is a Marten projection over the custody streams, using the **Inline** lifecycle in M1
(read-your-writes by construction) — the async projection daemon arrives in M3 for the live
board. Read via `GET /requests?waiting`. See [[adr-0009]], [[adr-0010]].

### Actor
Who performed an action, recorded on every custody event (§4). In M1 it is the current
**persona** (clinician / dispatcher / receiving / fleet manager), sent by the client as an
`X-Actor` request header — the auth stand-in's wire format (§3.9). The `MissionManager`
stamps it onto appended events. No backend role enforcement in M1: the role switcher renders
persona-appropriate screens, but the API accepts any action from any persona. Real verified
identity later arrives through this same seam. See [[adr-0008]].

### Status (M1)
`Mission.status` in M1 is exactly `Waiting | Cancelled` — the only states M1 produces. §4's
fuller lifecycle (`assigned → … → delivered`) is added by later milestones; M1 ships no
speculative values. See [[adr-0007]].

### MissionCancelled
The event appended when a waiting mission is cancelled; flips status to `Cancelled` and drops
the mission from the waiting projection. Cancel is **idempotent**: re-cancelling an
already-cancelled mission returns 200 and appends no second event. See [[adr-0007]].

### Cancel guard
The `MissionManager` invariant that a mission is cancellable only while it "hasn't flown
yet". In M1 that collapses to `status == Waiting`; later milestones widen it to any
pre-launch status. Lives in the manager, not the UI (§6.3). See [[adr-0007]].

### Latest arrival
`latestArrival` — the latest useful arrival instant for a payload, **supplied by the
clinician** as an absolute UTC timestamp. It is an *input*, not a computed value. M1
validates only that it is in the future. Per-payload-type viability windows are a planning
concern (PayloadPolicy, M2+), deliberately not enforced at intake. See [[adr-0005]].

### Time-to-deadline
`latestArrival − now`, computed **server-side** against the server clock (§6.1) — never on
the client. The urgent-first waiting list sorts ascending by this value.

### MissionRequested
The birth event of a Mission's stream, appended when a clinician submits a request. Named
from the mission's point of view (not "RequestSubmitted") so the custody stream reads as one
coherent story. Payload: `payloadType`, `weight`, `originSite`, `destSite`, `latestArrival`,
plus the acting persona (see [[actor]]).
