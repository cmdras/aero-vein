import { useEffect, useState } from 'react';
import { api } from '#/api/client';
import type { components } from '#/api/schema';

type Site = components['schemas']['Site'];

type SitesResult = {
  sites: Site[];
  loading: boolean;
  error: string | null;
};

/**
 * Module-level cached promise so `/api/sites` is fetched exactly once and shared
 * across every consumer. This dedupes React StrictMode's dev double-mount and the
 * multiple <SiteDropdown> instances on the home page without needing a provider.
 */
let sitesPromise: Promise<Site[]> | null = null;

function loadSites(): Promise<Site[]> {
  if (!sitesPromise) {
    sitesPromise = api
      .GET('/api/sites')
      .then(({ data, error }) => {
        if (error || !data) {
          throw new Error('Failed to load sites');
        }
        return data;
      })
      .catch((err) => {
        // Reset the cache so a later mount can retry instead of being stuck on a
        // permanently-rejected promise.
        sitesPromise = null;
        throw err;
      });
  }
  return sitesPromise;
}

export function useSites(): SitesResult {
  const [sites, setSites] = useState<Site[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    loadSites()
      .then((data) => {
        if (cancelled) return;
        setSites(data);
        setError(null);
        setLoading(false);
      })
      .catch(() => {
        if (cancelled) return;
        setError('Could not load sites. Please try again.');
        setLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, []);

  return { sites, loading, error };
}
