import { normalizeLocale } from "@/lib/i18n";

type Summary = {
  source: string;
  generated_at: string;
  migrated_endpoints: number;
  pending_endpoints: number;
};

async function getSummary(): Promise<Summary> {
  const baseUrl = process.env.NEXT_PUBLIC_BASE_URL ?? "http://localhost:3001";
  const response = await fetch(`${baseUrl}/api/bff/reports/summary`, {
    cache: "no-store"
  });
  if (!response.ok) {
    return {
      source: "fallback",
      generated_at: new Date().toISOString(),
      migrated_endpoints: 0,
      pending_endpoints: 0
    };
  }
  return response.json();
}

export default async function ReportsPage({
  params
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: rawLocale } = await params;
  const locale = normalizeLocale(rawLocale);
  const summary = await getSummary();

  return (
    <section className="card">
      <h2>Reports</h2>
      <p>Locale: {locale}</p>
      <p>Source: {summary.source}</p>
      <p>Generated at: {summary.generated_at}</p>
      <p>Migrated endpoints: {summary.migrated_endpoints}</p>
      <p>Pending endpoints: {summary.pending_endpoints}</p>
    </section>
  );
}
