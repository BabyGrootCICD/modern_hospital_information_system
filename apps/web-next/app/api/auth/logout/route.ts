import { NextResponse } from "next/server";

export async function POST(request: Request) {
  const formData = await request.formData();
  const locale = String(formData.get("locale") || "en");
  const response = NextResponse.redirect(new URL(`/${locale}/login`, request.url));
  response.cookies.delete("shis_session");
  return response;
}
