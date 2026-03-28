import { NextResponse } from "next/server";

const allowedRoles = new Set(["doctor", "admin", "auditor"]);

export async function POST(request: Request) {
  const formData = await request.formData();
  const username = String(formData.get("username") || "").trim();
  const role = String(formData.get("role") || "").trim();
  const locale = String(formData.get("locale") || "en").trim();

  if (!username || !allowedRoles.has(role)) {
    return NextResponse.json({ error: "Invalid credentials payload" }, { status: 400 });
  }

  const response = NextResponse.redirect(new URL(`/${locale}/dashboard`, request.url));
  response.cookies.set("shis_session", `${username}:${role}`, {
    httpOnly: true,
    sameSite: "lax",
    path: "/"
  });

  return response;
}
