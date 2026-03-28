import { NextResponse } from "next/server";

type MigrationSummary = {
  source: string;
  generated_at: string;
  migrated_endpoints: number;
  pending_endpoints: number;
};

export async function GET() {
  const upstream = process.env.LEGACY_REPORT_SUMMARY_URL;
  if (upstream) {
    try {
      const response = await fetch(upstream, { cache: "no-store" });
      if (response.ok) {
        const data = (await response.json()) as MigrationSummary;
        return NextResponse.json({ ...data, source: "legacy-upstream" });
      }
    } catch {
      // fall through to static baseline response
    }
  }

  return NextResponse.json({
    source: "bff-baseline",
    generated_at: new Date().toISOString(),
    migrated_endpoints: 14,
    pending_endpoints: 92
  } satisfies MigrationSummary);
}
