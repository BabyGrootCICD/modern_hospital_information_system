import { NextResponse } from "next/server";

type CreateJobBody = {
  mode: "export" | "import";
  hospital: string;
  payload_ref?: string;
};

export async function POST(request: Request) {
  const body = (await request.json()) as CreateJobBody;
  const syncBase = process.env.SYNC_SERVICE_BASE_URL || "http://localhost:8085";

  const response = await fetch(`${syncBase}/v1/sync/jobs`, {
    method: "POST",
    headers: { "content-type": "application/json" },
    body: JSON.stringify(body),
    cache: "no-store"
  });

  const payload = await response.json();
  return NextResponse.json(payload, { status: response.status });
}
