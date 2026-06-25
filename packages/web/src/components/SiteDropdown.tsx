import { useEffect, useState } from 'react';
import { api } from '#/api/client';
import type { components } from '#/api/schema';

type Site = components['schemas']['Site'];

type Props = {
  label: string;
  value: string;
  onChange: (id: string) => void;
};

export function SiteDropdown({ label, value, onChange }: Props) {
  const [sites, setSites] = useState<Site[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.GET('/api/sites').then(({ data }) => {
      if (data) setSites(data);
      setLoading(false);
    });
  }, []);

  return (
    <label className="flex flex-col gap-1 text-sm">
      <span className="font-medium text-foreground">{label}</span>
      <select
        value={value}
        onChange={(e) => onChange(e.target.value)}
        disabled={loading}
        className="rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground shadow-sm disabled:opacity-50"
      >
        <option value="">{loading ? 'Loading…' : 'Select a site'}</option>
        {sites.map((s) => (
          <option key={s.id} value={s.id}>
            {s.name}
          </option>
        ))}
      </select>
    </label>
  );
}
