import { getSession } from "@/lib/auth";
import { dictionary, normalizeLocale } from "@/lib/i18n";

export default async function DashboardPage({
  params
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: rawLocale } = await params;
  const locale = normalizeLocale(rawLocale);
  const t = dictionary(locale);
  const session = await getSession();

  return (
    <section className="card">
      <h2>{t.dashboard}</h2>
      <p>Signed in as: {session?.username}</p>
      <p>Role: {session?.role}</p>
      <form method="post" action="/api/auth/logout">
        <input type="hidden" name="locale" value={locale} />
        <button type="submit">{t.logout}</button>
      </form>
    </section>
  );
}
