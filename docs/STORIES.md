# AeroVein — User Stories

> The dispatch-and-operations brain for a same-day medical drone courier service.
> Single-city pilot in **Mechelen**. Drones and flight control are out of scope — a
> simulation engine stands in for the real fleet. Auth is out of scope — a role switcher
> stands in for the four personas (clinician, dispatcher, receiving staff, fleet manager).

Stories are grouped into **milestones** that are also **vertical slices** — each one is
shippable and demonstrable on its own.

## Milestone 1 — Request intake + the spine

- As a **clinician**, I want to submit a transport request (payload type, weight, origin site, destination site, and the latest useful arrival time) so that AeroVein can move it.
- As a **clinician**, I want to cancel a request that hasn't flown yet so that we don't dispatch a drone for a situation that has changed.
- As a **dispatcher**, I want one list of all waiting requests with the most urgent deadlines first so that I always know what needs attention next.
- As a **user**, I want to switch between personas so that I can exercise the whole operation in the demo without real accounts.

## Milestone 2 — The fleet + the assignment brain

- As a **fleet manager**, I want to see every drone with its status, battery, and location so that I have an honest picture of fleet readiness.
- As a **dispatcher**, I want the system to propose the best drone(s) for a waiting request so that I'm not solving a logistics puzzle in my head.
- As a **dispatcher**, I want to approve the proposed drone — or override it with another feasible one — so that a human stays accountable for every flight.

## Milestone 3 — The live view

- As a **dispatcher**, I want each active flight to keep its position, battery, and ETA current as it progresses so that what I see always matches what's actually happening.
- As a **dispatcher**, I want a single screen showing every drone in the air, every payload in motion, and every request waiting — updating live — so that I can run the operation from one place.
- As a **dispatcher**, I want every active flight to show live time-to-deadline against its live ETA so that I can see at a glance whether it will make it.

## Milestone 4 — Disruptions + reactive replanning

- As a **dispatcher**, I want any flight put at risk by changing conditions (weather, a new no-fly zone, a drone draining faster than expected) to be flagged immediately and impossible to miss so that I never lose a payload to something I could have caught.
- As a **dispatcher**, I want the system to propose a concrete fix for each at-risk flight so that I can act fast instead of starting from scratch.
- As a **dispatcher**, I want to approve or reject a proposed replan so that a human stays accountable even under pressure.

## Milestone 5 — Closing the loop + chain of custody

- As **receiving staff**, I want to be told when a drone is inbound and which pad to clear so that the pad is ready when it lands.
- As **receiving staff**, I want to confirm the payload arrived and stayed within its temperature range so that the chain of custody is closed cleanly.
- As a **dispatcher**, I want to see the complete, ordered history of any payload so that custody is provable end to end.
- As a **fleet manager**, I want to pull a drone for maintenance so that it won't be assigned to new flights.
