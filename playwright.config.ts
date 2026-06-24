import { defineConfig, devices } from '@playwright/test';

// E2E is the per-milestone gate (docs/WORKFLOW.md §5). The Playwright MCP authors these
// specs at authoring time; this runner replays them headlessly at run time — same locators,
// no model, no token cost.
//
// Target selection:
//   - CI runs against a deployed Render preview env (docs/WORKFLOW.md §4): set E2E_BASE_URL.
//   - Locally we start the web dev server (:3000) and run against it.
const baseURL = process.env.E2E_BASE_URL ?? 'http://localhost:3000';
const isRemoteTarget = Boolean(process.env.E2E_BASE_URL);

export default defineConfig({
  testDir: 'e2e',

  // Keep generated run artifacts (traces, screenshots, .last-run.json) alongside the specs
  // rather than at the repo root. Playwright excludes outputDir from test collection.
  outputDir: 'e2e/test-results',

  // One spec at a time can mutate shared user state; keep files parallel, tests serial-safe.
  fullyParallel: true,

  // Fail CI if a stray test.only is committed.
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,

  reporter: process.env.CI
    ? [['github'], ['html', { outputFolder: 'e2e/playwright-report', open: 'never' }]]
    : [['html', { outputFolder: 'e2e/playwright-report' }]],

  use: {
    baseURL,
    // Matches the data-testid convention in docs/WORKFLOW.md §5 so generated locators
    // don't churn. (This is Playwright's default, pinned here to make the contract explicit.)
    testIdAttribute: 'data-testid',
    trace: 'on-first-retry',
  },

  // Mobile-first app (docs/ARCHITECTURE.md §1) → exercise it at a phone viewport first.
  projects: [
    {
      name: 'mobile-chrome',
      use: { ...devices['Pixel 5'] },
    },
  ],

  // Locally, boot the web dev server before tests. When E2E_BASE_URL points at a deployed
  // preview, the app is already running there — don't start anything.
  webServer: isRemoteTarget
    ? undefined
    : {
        command: 'bun run dev',
        url: baseURL,
        reuseExistingServer: !process.env.CI,
        timeout: 120_000,
      },
});
