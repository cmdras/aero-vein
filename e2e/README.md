# E2E specs

End-to-end browser tests — the **per-milestone gate** in
[docs/WORKFLOW.md §5](../docs/WORKFLOW.md). Playwright is one substrate used in two modes:

- **Authoring time** — the agent drives a real browser through a milestone's journey via the
  **Playwright MCP** (accessibility-tree based; cheap, no vision model) and crystallises what it
  verified into `e2e/*.spec.ts`.
- **Run time** — CI runs **`playwright test`** headless against a deployed Render preview. These
  specs are the merge gate: deterministic, parallel, no model, no token cost (WORKFLOW.md
  principle 2).

## Running

```bash
# Local — boots the web dev server (:3000) automatically, then runs the specs.
bun run test:e2e

# Against a deployed Render preview (how CI runs it) — no local server started.
E2E_BASE_URL=https://<preview-url> bun run test:e2e
```

First-time setup downloads the browser binary (one-off, needs network + `~/.cache` write, so
run it outside the sandbox):

```bash
bunx playwright install chromium
```

## Conventions

- **`data-testid`** on interactive components — Playwright's `testIdAttribute`, pinned in
  `playwright.config.ts`. Use `page.getByTestId(...)` so generated locators don't churn when
  copy or layout changes.
- **Mobile-first viewport.** The single project runs at a phone viewport (`Pixel 5`), matching
  [ARCHITECTURE §1](../docs/ARCHITECTURE.md). Add a desktop project only when a story needs it.
- Keep `secrets` out of specs and the agent transcript — use the MCP `secrets` config.

## The auth seam (pending — M1)

CI can't perform the real Microsoft login (account, MFA, consent). The suite must start
**already authenticated** and test the post-login app — not the Microsoft hop itself
(WORKFLOW.md §5 "auth exception", §6). The seam is **not yet chosen**; the two candidates:

1. **Captured `storageState`** — log in once, save cookies/state to a file, load it in
   `use.storageState`.
2. **Test-only session-seeding endpoint** — a guarded endpoint that mints a session cookie
   directly, called from a Playwright `*.setup.ts` project that the test projects depend on.

When M1 lands and the seam is picked, wire it as a setup project, e.g.:

```ts
// playwright.config.ts
projects: [
  { name: 'setup', testMatch: /.*\.setup\.ts/ },
  { name: 'mobile-chrome', use: { ...devices['Pixel 5'], storageState: 'e2e/.auth/user.json' }, dependencies: ['setup'] },
]
```
