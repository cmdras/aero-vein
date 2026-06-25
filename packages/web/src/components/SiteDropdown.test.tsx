import { afterEach, beforeEach, expect, test, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';

const GET = vi.fn();

vi.mock('#/api/client', () => ({
  api: { GET },
}));

beforeEach(() => {
  GET.mockReset();
});

afterEach(() => {
  // The useSites hook caches the fetch promise at module level; reset it so each
  // test starts from a clean slate.
  vi.resetModules();
});

async function renderDropdown() {
  // Import after the mock + module reset so the fresh module-level cache is used.
  const { SiteDropdown } = await import('./SiteDropdown');
  return render(<SiteDropdown label="Origin site" value="" onChange={() => {}} />);
}

test('renders fetched sites once data resolves', async () => {
  GET.mockResolvedValue({
    data: [
      { id: '11111111-1111-1111-1111-111111111111', name: 'Mechelen North' },
      { id: '22222222-2222-2222-2222-222222222222', name: 'Mechelen South' },
    ],
    error: undefined,
  });

  await renderDropdown();

  await waitFor(() => expect(screen.getByText('Mechelen North')).toBeDefined());
  expect(screen.getByText('Mechelen South')).toBeDefined();
});

test('surfaces an error message when the fetch rejects', async () => {
  GET.mockRejectedValue(new Error('network down'));

  await renderDropdown();

  await waitFor(() => expect(screen.getByRole('alert')).toBeDefined());
  expect(screen.getByRole('combobox')).toHaveProperty('disabled', true);
});

test('surfaces an error when the API returns an error payload', async () => {
  GET.mockResolvedValue({ data: undefined, error: { message: 'boom' } });

  await renderDropdown();

  await waitFor(() => expect(screen.getByRole('alert')).toBeDefined());
});
