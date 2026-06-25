import { createFileRoute } from '@tanstack/react-router';
import { Sparkles } from 'lucide-react';

export const Route = createFileRoute('/')({
  component: Home,
});

function Home() {
  return (
    <main className="flex flex-1 flex-col items-center justify-center gap-8 text-center">
      <div className="flex flex-col items-center gap-4">
        <span className="flex size-16 items-center justify-center rounded-2xl bg-primary/10 text-primary">
          <Sparkles className="size-8" />
        </span>
        <div>
          <h1 className="text-3xl font-semibold tracking-tight">AeroVein</h1>
          <p className="mt-2 text-muted-foreground">Scaffolded. Build the first milestone from here.</p>
        </div>
      </div>
    </main>
  );
}
