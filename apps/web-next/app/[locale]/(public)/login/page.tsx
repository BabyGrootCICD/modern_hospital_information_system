import { dictionary, normalizeLocale } from "@/lib/i18n";

export default async function LoginPage({
  params
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: rawLocale } = await params;
  const locale = normalizeLocale(rawLocale);
  const t = dictionary(locale);

  return (
    <section className="card">
      <h2>{t.login}</h2>
      <form method="post" action="/api/auth/login" className="form">
        <input type="hidden" name="locale" value={locale} />
        <label>
          {t.username}
          <input name="username" defaultValue="user01" required />
        </label>
        <label>
          {t.role}
          <select name="role" defaultValue="doctor">
            <option value="doctor">doctor</option>
            <option value="admin">admin</option>
            <option value="auditor">auditor</option>
          </select>
        </label>
        <button type="submit">{t.login}</button>
      </form>
    </section>
  );
}
