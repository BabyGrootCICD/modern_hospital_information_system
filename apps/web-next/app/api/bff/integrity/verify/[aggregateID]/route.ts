import { NextResponse } from "next/server";

export async function GET(
  _request: Request,
  { params }: { params: Promise<{ aggregateID: string }> }
) {
  const { aggregateID } = await params;
  const baseUrl = process.env.INTEGRITY_SERVICE_BASE_URL || "http://localhost:8091";

  const response = await fetch(`${baseUrl}/v1/integrity/verify/${aggregateID}`, {
    cache: "no-store"
  });
  const body = await response.json();
  return NextResponse.json(body, { status: response.status });
}
