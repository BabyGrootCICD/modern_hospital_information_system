import Link from "next/link";
import type { ReactNode } from "react";
import { dictionary, normalizeLocale } from "@/lib/i18n";

export default async function LocaleLayout({
  children,
  params
}: {
  children: ReactNode;
  params: Promise<{ locale: string }>;
}) {
  const { locale: rawLocale } = await params;
  const locale = normalizeLocale(rawLocale);
  const t = dictionary(locale);

  return (
    <main>
      <header className="topbar">
        <div>
          <h1>{t.title}</h1>
          <p>{t.subtitle}</p>
        </div>
        <nav className="topnav">
          <Link href={`/${locale}/dashboard`}>{t.dashboard}</Link>
          <Link href={`/${locale}/admin`}>{t.admin}</Link>
          <Link href={`/${locale}/reports`}>{t.reports}</Link>
          <Link href={`/${locale === "en" ? "zh" : "en"}/login`}>{locale === "en" ? "中文" : "EN"}</Link>
        </nav>
      </header>
      {children}
    </main>
  );
}
