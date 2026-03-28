import { NextResponse } from "next/server";

type ServiceHealth = {
  name: string;
  status: "ok" | "down";
  url: string;
};

async function probe(name: string, url: string): Promise<ServiceHealth> {
  try {
    const response = await fetch(url, { cache: "no-store" });
    return { name, status: response.ok ? "ok" : "down", url };
  } catch {
    return { name, status: "down", url };
  }
}

export async function GET() {
  const checks = await Promise.all([
    probe("identity-access-service", process.env.IDENTITY_SERVICE_HEALTH_URL || "http://localhost:8081/healthz"),
    probe("patient-chart-service", process.env.PATIENT_SERVICE_HEALTH_URL || "http://localhost:8082/healthz"),
    probe("order-clinical-service", process.env.ORDER_SERVICE_HEALTH_URL || "http://localhost:8083/healthz"),
    probe("audit-integrity-service", process.env.AUDIT_SERVICE_HEALTH_URL || "http://localhost:8091/healthz")
  ]);

  const overall = checks.every((x) => x.status === "ok") ? "ok" : "degraded";
  return NextResponse.json({ overall, checks });
}
