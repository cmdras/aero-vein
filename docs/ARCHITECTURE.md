# AeroVein — Architecture

> **Status:** SKELETON — sections marked 🔲 are unresolved decisions we fill in together.
> This document captures *why the system is shaped the way it is* and *where the
> boundaries are*. It is a map, not the territory — keep it to a few pages and link out
> for detail. North star for *what* we're building: [STORIES.md](./STORIES.md);
> background: [PITCH.md](../PITCH.md).
>
> **Method:** volatility-based decomposition (Juval Löwy, *Righting Software*).
> We decompose along **axes of volatility**, not function. The STORIES milestones are
> *use cases* — each is a call-chain across the boxes below, never a box of its own.
>
> **Legend:** ✅ decided & stable · 🔲 decision needed · 🔗 lives elsewhere (don't restate)

---

## 1. What this system is

✅ The **dispatch-and-operations brain** for a same-day medical drone courier service —
the system dispatchers use to receive transport requests, assign drones, and keep every
time-critical payload on track to land while it still matters. Single-city pilot in
**Mechelen**.

Two things are *stood in for*, by design (see §3 for why this shapes everything):
- **Drones & flight control are out of scope** → a **simulator** stands in for the fleet.
- **Auth is out of scope** → a **role switcher** stands in for the four personas
  (clinician, dispatcher, receiving staff, fleet manager).

Why it exists and who it's for: see [STORIES.md](./STORIES.md). This document does not
restate the requirements — it explains the *structure* that serves them.

---

## 2. The shape (components & boundaries)

✅ Löwy layers. Read top-to-bottom: clients call managers, managers orchestrate engines
and resource-access, nothing calls upward, **engines never call each other**.

```
  CLIENTS      Clinician · Dispatcher · Receiving · FleetMgr   (+ role switcher = auth stand-in)
                 │   render PUSHED state, issue actions — compute no business logic
                 ▼
  MANAGERS     MissionManager                         FleetManager
               (request → delivery + custody;          (drone readiness lifecycle:
                owns the human-approval gate)           in-service/charging/grounded/maint)
                 │   orchestrate the use-case sequence
                 ▼
  ENGINES      PlanningEngine     RiskEngine     PayloadPolicy
               (feasible options   (ETA vs        (per-type custody,
                + ETA)              deadline)      cold-chain, viability)
                 │   pure-ish rules/computation; mutually independent
                 ▼
  RESOURCE     Fleet/TelemetryAccess · ConditionsAccess · CustodyEventStore · MissionStore · SiteAccess
  ACCESS         │   encapsulate HOW we reach a resource — not the resource itself
                 ▼
  RESOURCES    Simulation engine  ·  weather / no-fly-zone source  ·  DB
  UTILITIES    live-update transport · server clock · notifications · auth stand-in
```

✅ **Stack.** bun + React (TypeScript) frontend · .NET (C#) backend · Postgres, as a monorepo
(per the `app-scaffold` skill). **bun is frontend-only** — toolchain, dev server, and React
runtime; it runs no business logic. Everything below CLIENTS is .NET. Client/server contracts
(§5) are generated from the API via OpenAPI codegen, not hand-mirrored.

✅ **Event store: Marten.** Postgres is reached through **Marten** (Postgres-native event store
+ projections) for the event-sourced core (§4): append-only streams, rebuildable projections,
and per-stream concurrency are first-class — exactly §4's model, so we don't hand-roll it.
**Discipline:** Marten lives *behind* the ResourceAccess seam (`CustodyEventStore`,
`MissionStore`); its `IDocumentSession` never leaks into managers or engines (§3.4).

### The anti-design we rejected (the foil)
A functional decomposition falls straight out of the milestones — and that's the trap:

> `IntakeService → AssignmentService → TrackingService → ReplanningService → DeliveryService` (+ `FleetService`)

This is the food-app `Ordering → Billing → Shipping` chain in scrubs. It forces a rigid
call order, and the `AssignmentService` must know about requests, drones, weather,
deadlines, *and* what tracking needs next → it bloats into a monolith; or the dispatcher
screen orchestrates the chain → a bloated client with business logic in the UI. The
milestones tell us what to **demo**, not what to **build**.

---

## 3. Volatilities & the boundaries that encapsulate them

*The heart of this document. For each: what varies, and what boundary contains the blast
radius.* Two **independent** axes — change over time, and change across context.

### Axis 1 — over time (Mechelen as the pilot matures)

#### 3.1 Delivery planning — *highest volatility*
✅ **DECIDED.** Start simple (range + battery + free + healthy) → multi-leg, battery-swap,
predictive battery models, ground fallback. **One `PlanningEngine`**, because "assign a
drone" was a *solution masquerading as the requirement* **get this payload there in time**.

- **One engine, two callers.** Initial dispatch (M2) calls it from a clean slate; reactive
  replanning (M4) calls it from a disrupted, in-flight state. Same rules.
- **Contract** (shape to confirm, 🔲 exact signature):

  ```
  IReadOnlyList<Option> OptionsFor(Mission mission, FleetState fleet,
                                   Conditions conditions, Constraints constraints)
  ```

  Returns *ranked feasible options* including ETA. Range, battery, weight, and cold-chain
  capability are **inputs/constraints — never baked into the algorithm.**

#### 3.2 Source of drone reality — *the critical seam*
✅ **DECIDED.** Simulator today → real drone telemetry later, both behind
**`Fleet/TelemetryAccess`**. **No business component may know whether a drone is simulated
or real** — position, battery, and status all arrive through this one boundary. STORIES'
"the simulation stands in for the real fleet" *is* a volatility declaration.

#### 3.3 What invalidates a plan
✅ Weather, a new no-fly zone, a drone draining faster than expected — all reach the system
through **`ConditionsAccess`** (weather + NFZ source). The **`RiskEngine`** continuously
evaluates plan validity against live conditions and telemetry. New disruption *types* are
added behind this seam without touching planning.

#### 3.4 Persistence
✅ Behind ResourceAccess. Business logic never couples to the storage tech (the masterclass
EF-Core "hot path" warning) — this is why **Marten stays behind `CustodyEventStore`/`MissionStore`**
and its session type never crosses into managers/engines. 🔗 schema detail lives with migrations, not here.

#### 3.5 Notification channel
✅ In-app today → SMS / pager / hospital-system integration later. A **utility**; the
business logic emits domain notifications, not channel-specific messages.

### Axis 2 — across context (freeze time)

#### 3.6 Payload type
✅ **DECIDED — genuine process volatility, not just different numbers.** Blood, organ, lab
sample, and pharma differ in *handling and custody process* (cold-chain rules, viability,
required custody steps), and these will keep changing. Encapsulated in the **`PayloadPolicy`**
engine. **Wiring discipline:** the **MissionManager reads PayloadPolicy and passes the
result as constraints into the PlanningEngine** (e.g. "organ → cold-chain required" becomes
a feasibility filter). PayloadPolicy and PlanningEngine never call each other.

#### 3.7 Geography / city
✅ Sites, landing pads, no-fly zones, and weather source vary by locale. Seams are
*positioned* (locale data + ConditionsAccess + site/pad data) but **Mechelen only** for the
pilot — multi-city is not built (§7).

#### 3.8 Drone model capability
✅ Range / battery curve / capacity / cold-chain capability are **data behind
Fleet/TelemetryAccess**, fed to planning as constraints — not branches in code.

### Deliberately NOT abstracted (variability, not volatility)

- **3.9 Auth → role switcher.** ✅ We do **not** build real auth or a provider-agnostic
  layer; that volatility isn't real yet. The four personas are **clients**, not
  components — there is no "Clinician service." (Revisit when a real deployment/security
  story exists, §7.)
- **3.10 Deadline / temperature / weight numbers.** ✅ Config/data tables, resolved with
  lookups — *variability*, handled in code. No component. (The per-type *process* that
  consumes them is §3.6.)

---

## 4. Data & event model

✅ **DECIDED — chain of custody is an immutable, append-only event log (the facts); the
live operational view is a derivation of it.** "Who handled what, when it moved, whether
temperature stayed in range" (PITCH) is exactly an event stream, and provable custody
(story M5) demands the facts never be mutated in place.

- **`CustodyEvent`** — append-only: `(missionId, type, at, actor, location, tempC, …)`.
  Created · approved-for-flight · launched · position/telemetry · inbound · landed ·
  confirmed-received. **This is the truth.**
- **Live mission/fleet state** — a **rebuildable projection** of the event log + latest
  telemetry. The dashboard reads the projection; it is never the source of truth.
- **Single writer per stream**, so the projection can't drift from the events.
- **Maps onto Marten:** custody history (`GET /missions/{id}/custody`) is a **live aggregation**
  (`AggregateStreamAsync`, computed on read — no stored copy to drift); the waiting list (M1) is an
  **Inline projection** (read-your-writes by construction — see [[adr-0009]]); the live ops board
  (M3) upgrades to an **async projection** run by the projection daemon (rebuildable from zero on
  demand). Per-stream optimistic concurrency enforces the single writer.

Stable entities (sketch — 🔲 column-level detail lives with migrations):

```
Mission ( id, payloadType, weight, originSite, destSite, latestArrival, status )
Drone   ( id, model, status, … )          -- live battery/position come via TelemetryAccess
Site    ( id, name, pads[] )               -- locale data (Mechelen)
CustodyEvent ( id, missionId, type, at, actor, location, tempC )   -- immutable facts
```

🔗 Beyond the boundaries (indexes, projection storage), detail lives with the schema.

---

## 5. Surface (traced from STORIES.md)

✅ Shaped by **use case, not table** — every story traced through the layers. The dashboard
(M3) receives **pushed state and computes nothing**; "live ETA vs. deadline" is the
RiskEngine's output rendered, not a client calculation.

| Story (milestone) | Call-chain | Surface |
|---|---|---|
| Submit request (M1) | Client → MissionManager → MissionStore | `POST /requests` |
| Cancel un-flown request (M1) | Client → MissionManager (guard: not yet launched) | `POST /requests/{id}/cancel` |
| Urgent-first waiting list (M1) | Client ← MissionManager projection (sorted by time-to-deadline) | `GET /requests?waiting` |
| Switch persona (M1) | Client only (auth stand-in utility) | *no business endpoint* |
| Fleet readiness (M2) | Client ← FleetManager ← Fleet/TelemetryAccess | `GET /drones` |
| Propose drone(s) (M2) | MissionManager → PayloadPolicy → PlanningEngine | `GET /requests/{id}/options` |
| Approve / override (M2) | MissionManager (approval gate) → commit | `POST /requests/{id}/assign` |
| Live position/battery/ETA (M3) | TelemetryAccess → MissionManager → live-update push | *push channel* |
| One live operations screen (M3) | projection → live-update push | `GET /board` + push |
| Flag at-risk flight (M4) | ConditionsAccess → RiskEngine trips → push | *push channel* |
| Propose a fix (M4) | MissionManager → PlanningEngine `OptionsFor(in-flight)` | `GET /missions/{id}/options` |
| Approve / reject replan (M4) | MissionManager (approval gate) → recommit | `POST /missions/{id}/replan` |
| Inbound + which pad (M5) | MissionManager → notifications utility | `POST` event + push |
| Confirm arrival + temp (M5) | Client → MissionManager → append CustodyEvent | `POST /missions/{id}/receive` |
| Full ordered custody history (M5) | CustodyEventStore (the log itself) | `GET /missions/{id}/custody` |
| Pull drone for maintenance (M5) | FleetManager (state transition) | `POST /drones/{id}/maintenance` |

**Validates the design (needs no new endpoint):** persona switch (§3.9); "resume / single
screen" is free because state is a server-side projection, not client memory.

🔲 **Live-update transport** — SSE vs WebSocket vs polling. The boundary (a "push state to
clients" utility) is fixed; the mechanism is open.

🔗 Per-endpoint request/response schemas → API docs, not here.

---

## 6. Cross-cutting constraints

### 6.1 Time is the central constraint
✅ Deadlines are hard (a missed window can waste an organ). The **server owns the clock**;
time-to-deadline and ETA are computed **server-side, never on the client**. The dashboard
displays them.

### 6.2 The simulator is clock-driven, and lives behind the fleet seam
✅ The simulation advances drone position and battery on a clock **tick**, and is reached
**only** through `Fleet/TelemetryAccess` (§3.2). State flows
`sim → TelemetryAccess → MissionManager → live-update utility → clients`. Swapping in real
telemetry replaces the resource behind the seam and touches no business logic.

### 6.3 A human approves every flight
✅ This is a **MissionManager invariant**, not a component: any state transition that
commits a drone (initial assign M2, replan M4) blocks on dispatcher approval. Encapsulated
in one place — never scattered across the UI.

---

## 7. Out of scope / known debt

✅ Deliberate omissions, not oversights. Each rides a boundary we already built, so adding
it later is local, not structural.

| Not built yet | Why deferred | Trigger to build it |
|---|---|---|
| **Real drone telemetry / flight control** | Simulator stands in (§3.2); `Fleet/TelemetryAccess` seam is ready. | Real drones in the loop. |
| **Multi-city** | Mechelen pilot; locale/conditions/site seams *positioned* (§3.7). | A second city. |
| **Real auth / providers** | Role switcher (§3.9); clients already shielded. | A real deployment or security story. |
| **Billing, contracts, regulatory certification, hospital-record integration** | Explicitly out of scope in PITCH — we build the ops brain first. | A story for any of them. |
| **Disruption types beyond weather/NFZ/battery** | RiskEngine + ConditionsAccess (§3.3) accept new types without touching planning. | A new real-world disruption source. |
| **Planning beyond single-leg** (battery-swap, multi-leg, ground fallback) | `PlanningEngine` contract (§3.1) takes constraints; algorithm can grow behind it. | A feasibility gap the simple algorithm can't close. |

---

## 8. Pointers

- North star (requirements): [STORIES.md](./STORIES.md)
- Background (the problem & the pitch): [PITCH.md](../PITCH.md)
- Method reference: Löwy, *Righting Software* (volatility-based decomposition)
- Decision log: *(future — `docs/adr/` when rationale outgrows §3–§4)*
