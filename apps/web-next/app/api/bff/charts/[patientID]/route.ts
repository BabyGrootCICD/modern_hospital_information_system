import { NextResponse } from "next/server";

export async function GET(
  _request: Request,
  { params }: { params: Promise<{ patientID: string }> }
) {
  const { patientID } = await params;
  const chartsBase = process.env.CHARTS_SERVICE_BASE_URL || "http://localhost:8082";

  const response = await fetch(`${chartsBase}/v1/charts/${patientID}`, {
    cache: "no-store"
  });
  const payload = await response.json();
  return NextResponse.json(payload, { status: response.status });
}
