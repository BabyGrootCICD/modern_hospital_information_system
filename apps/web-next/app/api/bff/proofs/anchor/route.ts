import { NextResponse } from "next/server";

export async function POST(request: Request) {
  const payload = await request.json();
  const baseUrl = process.env.PROOF_SERVICE_BASE_URL || "http://localhost:8092";

  const response = await fetch(`${baseUrl}/v1/proofs/anchor`, {
    method: "POST",
    headers: { "content-type": "application/json" },
    body: JSON.stringify(payload),
    cache: "no-store"
  });
  const body = await response.json();
  return NextResponse.json(body, { status: response.status });
}
