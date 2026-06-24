# Project Pitch: AeroVein — Medical Courier Dispatch Platform

## The problem

Time-critical medical payloads — blood units, transplant organs, lab samples, urgent pharmaceuticals — move between hospitals far too slowly. Today this happens by car: a courier is called, sits in traffic, and a kidney with a 24-hour viability clock loses two of those hours to a congested ring road. Coordination happens over phone calls and spreadsheets. Nobody has a single, live picture of what is in transit, what is at risk, and what to do when something goes wrong.

We are launching a same-day medical courier service that flies these payloads between hospitals using autonomous drones. Our first deployment is a single-city pilot in **Mechelen** — a contained, well-defined area where we can prove the operation works before expanding to other cities. The drones are being handled separately. **What we need built is the platform that runs the operation** — the system our dispatchers use to receive requests, assign drones, and keep every payload on track to arrive while it still matters.

## What we're delivering to our customers

Hospitals and labs send us a transport request. We commit to a delivery window and fly it. Our promise is speed, reliability, and an unbroken chain of custody — the receiving hospital knows exactly when the payload arrives, on which pad, and that it stayed within safe temperature the whole way.

## What the platform must do

**Take in requests.** A clinician submits a transport request: what the payload is, where it starts, where it needs to go, and the latest moment it can still arrive and be useful. Different payload types have very different deadlines, and the system has to respect that.

**Help dispatchers assign the right drone.** Our dispatchers shouldn't be solving a logistics puzzle in their heads. The system should propose the best drone for each job — one with the range and battery to make it in time, that's free and healthy — and let a dispatcher approve or override it. A human always stays accountable for the decision, but the software does the heavy thinking.

**Show the whole operation live.** Dispatchers need a single screen showing every drone in the air, every payload in motion, and every request waiting — with the most urgent deadlines front and center. When something needs a human, it should be impossible to miss.

**React when reality changes.** Weather grounds part of the fleet. A temporary no-fly zone appears over an event. A drone's battery drains faster than expected. When a plan stops being valid, the system has to flag the affected payload, propose what to do instead, and let the dispatcher act — all while the deadline keeps counting down. This is the core of the service: getting the payload there anyway.

**Manage the fleet.** Fleet managers need to see each drone's status and battery, and take a drone out of service for maintenance so it won't be assigned.

**Close the loop on delivery.** The receiving hospital is told when a drone is inbound and which pad to clear, then confirms the payload arrived and stayed in temperature range. Every step is recorded.

## What our users need from it

- **Clinicians** want to submit a request in seconds and get a clear, trustworthy ETA back — then be able to cancel if the patient situation changes.
- **Dispatchers** want to triage by urgency, trust the system's suggestions, and never be blindsided by a problem they could have caught.
- **Receiving hospital staff** want enough warning to prep the landing pad and a simple way to confirm receipt.
- **Fleet managers** want an honest, real-time view of what the fleet can actually do right now.

## Requirements and constraints

- **Deadlines are hard, not soft.** A missed window can mean a wasted organ. The system must treat time as the central constraint, not a nice-to-have.
- **A human approves every flight.** These are medical payloads. The platform advises; a dispatcher commits.
- **Chain of custody must hold.** Who handled what, when it moved, and whether temperature stayed in range must be captured for every payload, end to end.
- **Physical limits are real.** Drone range, battery, payload weight, cold-chain requirements, landing-pad availability, weather, and no-fly zones all genuinely constrain what's possible and must be reflected honestly.
- **It has to be usable under pressure.** Dispatchers will be working time-critical, high-stakes situations. The interface must make the urgent thing obvious and the right action easy.

## What success looks like

A dispatcher sits in front of one screen and runs the whole Mechelen operation calmly. Requests come in and are matched to drones in seconds. Payloads fly. When a storm rolls in and grounds three routes at once, nobody panics — the affected payloads light up, the system offers a fix for each, the dispatcher approves them, and every payload still lands while it counts. That's the product.

## Out of scope for now

The drones themselves and their flight control, billing and contracts, regulatory certification, and integration with hospital record systems. We're building the **dispatch and operations brain** first.
