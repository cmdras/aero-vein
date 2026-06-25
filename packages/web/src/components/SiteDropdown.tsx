import { useSites } from '#/api/useSites';

type Props = {
  label: string;
  value: string;
  onChange: (id: string) => void;
};

export function SiteDropdown({ label, value, onChange }: Props) {
  const { sites, loading, error } = useSites();

  const placeholder = loading ? 'Loading…' : error ? 'Failed to load sites' : 'Select a site';

  return (
    <label className="flex flex-col gap-1 text-sm">
      <span className="font-medium text-foreground">{label}</span>
      <select
        value={value}
        onChange={(e) => onChange(e.target.value)}
        disabled={loading || !!error}
        aria-invalid={!!error}
        className="rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground shadow-sm disabled:opacity-50 aria-invalid:border-red-500"
      >
        <option value="">{placeholder}</option>
        {sites.map((s) => (
          <option key={s.id} value={s.id}>
            {s.name}
          </option>
        ))}
      </select>
      {error && (
        <span role="alert" className="text-xs text-red-500">
          {error}
        </span>
      )}
    </label>
  );
}
