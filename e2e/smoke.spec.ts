import { expect, test } from '@playwright/test';

// Placeholder smoke spec so `bun run test:e2e` is green from day one. Real milestone
// journeys (M2 decks, M3 study loop — docs/WORKFLOW.md §2) get authored here via the
// Playwright MCP and replayed by this runner. Replace this once M0 ships a real page.
test('app shell loads', async ({ page }) => {
  const response = await page.goto('/');
  expect(response?.ok()).toBeTruthy();
  await expect(page.locator('body')).toBeVisible();
});
