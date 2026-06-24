# 1. Record architecture decisions

Date: 2026-06-24

## Status

Accepted

## Context

We need to record the architectural decisions made on this project, so a newcomer can tell
a deliberate choice from an accident and understand the reasoning behind hard-to-reverse calls.

## Decision

We will use Architecture Decision Records, as described by Michael Nygard, stored as numbered
markdown files in `docs/adr/`. An ADR is written only when a decision is genuinely
hard-to-reverse — routine choices stay in the code. See `docs/agents/domain.md` for how the
agent skills consume these.

## Consequences

Decisions are captured close to the code, versioned with it, and reviewable in PRs. The
`grill-with-docs` skill appends ADRs inline as decisions crystallise during a milestone.
