import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

const locales = new Set(["en", "zh"]);

function hasLocale(pathname: string): boolean {
  const segment = pathname.split("/")[1];
  return locales.has(segment);
}

function isProtected(pathname: string): boolean {
  return /\/(dashboard|admin|reports)/.test(pathname);
}

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

  if (pathname.startsWith("/api") || pathname.startsWith("/_next") || pathname.includes(".")) {
    return NextResponse.next();
  }

  if (!hasLocale(pathname)) {
    const url = request.nextUrl.clone();
    url.pathname = `/en${pathname === "/" ? "/login" : pathname}`;
    return NextResponse.redirect(url);
  }

  if (isProtected(pathname) && !request.cookies.get("shis_session")) {
    const locale = pathname.split("/")[1] || "en";
    const url = request.nextUrl.clone();
    url.pathname = `/${locale}/login`;
    return NextResponse.redirect(url);
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/((?!_next/static|_next/image|favicon.ico).*)"]
};
