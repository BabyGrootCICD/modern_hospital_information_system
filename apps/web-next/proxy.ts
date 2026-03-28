import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

const locales = new Set(["en", "zh"]);
const protectedRoutes = new Set(["dashboard", "admin", "reports"]);

function getPathSegments(pathname: string): string[] {
  return pathname.split("/").filter(Boolean);
}

function isIgnoredPath(pathname: string): boolean {
  if (pathname.startsWith("/api") || pathname.startsWith("/_next")) {
    return true;
  }
  const segments = getPathSegments(pathname);
  const last = segments[segments.length - 1] ?? "";
  return last.includes(".");
}

function hasLocale(pathname: string): boolean {
  const [locale] = getPathSegments(pathname);
  return locale !== undefined && locales.has(locale);
}

function isProtected(pathname: string): boolean {
  const [locale, route] = getPathSegments(pathname);
  return locale !== undefined && locales.has(locale) && route !== undefined && protectedRoutes.has(route);
}

export function proxy(request: NextRequest) {
  const { pathname } = request.nextUrl;

  if (isIgnoredPath(pathname)) {
    return NextResponse.next();
  }

  if (!hasLocale(pathname)) {
    const url = request.nextUrl.clone();
    url.pathname = `/en${pathname === "/" ? "/login" : pathname}`;
    return NextResponse.redirect(url);
  }

  if (isProtected(pathname) && !request.cookies.get("shis_session")) {
    const [locale] = getPathSegments(pathname);
    const url = request.nextUrl.clone();
    url.pathname = `/${locale ?? "en"}/login`;
    return NextResponse.redirect(url);
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/((?!_next/static|_next/image|favicon.ico).*)"]
};
