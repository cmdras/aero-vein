import { createFileRoute } from '@tanstack/react-router';
import { useState } from 'react';
import { SiteDropdown } from '#/components/SiteDropdown';

export const Route = createFileRoute('/')({
  component: Home,
});

function Home() {
  const [originSite, setOriginSite] = useState('');
  const [destSite, setDestSite] = useState('');

  return (
    <main className="flex flex-1 flex-col gap-8 pt-8">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">New transport request</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Select origin and destination sites from the Mechelen catalog.
        </p>
      </div>
      <div className="flex flex-col gap-4">
        <SiteDropdown label="Origin site" value={originSite} onChange={setOriginSite} />
        <SiteDropdown label="Destination site" value={destSite} onChange={setDestSite} />
      </div>
    </main>
  );
}
