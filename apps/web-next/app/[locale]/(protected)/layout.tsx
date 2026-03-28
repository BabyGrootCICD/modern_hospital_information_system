import { redirect } from "next/navigation";
import type { ReactNode } from "react";
import { getSession } from "@/lib/auth";
import { normalizeLocale } from "@/lib/i18n";

export default async function ProtectedLayout({
  children,
  params
}: {
  children: ReactNode;
  params: Promise<{ locale: string }>;
}) {
  const { locale: rawLocale } = await params;
  const locale = normalizeLocale(rawLocale);
  const session = await getSession();

  if (!session) {
    redirect(`/${locale}/login`);
  }

  return <>{children}</>;
}
