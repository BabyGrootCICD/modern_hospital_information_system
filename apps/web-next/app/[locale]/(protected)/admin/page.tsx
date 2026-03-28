import { getSession } from "@/lib/auth";
import { normalizeLocale } from "@/lib/i18n";

export default async function AdminPage({
  params
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: rawLocale } = await params;
  const locale = normalizeLocale(rawLocale);
  const session = await getSession();

  if (session?.role !== "admin") {
    return (
      <section className="card">
        <h2>Admin</h2>
        <p>Read-only access denied for role: {session?.role}</p>
        <p>Login as admin to access migration administration widgets.</p>
      </section>
    );
  }

  return (
    <section className="card">
      <h2>Admin</h2>
      <p>Locale: {locale}</p>
      <ul>
        <li>Identity policy mapping status: ready</li>
        <li>Legacy route migration ratio: 12%</li>
        <li>Cutover gates: pending</li>
      </ul>
    </section>
  );
}
