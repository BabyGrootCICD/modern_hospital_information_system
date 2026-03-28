export const supportedLocales = ["en", "zh"] as const;
export type Locale = (typeof supportedLocales)[number];

type Dictionary = {
  title: string;
  subtitle: string;
  login: string;
  username: string;
  role: string;
  dashboard: string;
  admin: string;
  reports: string;
  logout: string;
};

const dictionaries: Record<Locale, Dictionary> = {
  en: {
    title: "SHIS Modern Portal",
    subtitle: "Phase 1 frontend shell",
    login: "Login",
    username: "Username",
    role: "Role",
    dashboard: "Dashboard",
    admin: "Admin",
    reports: "Reports",
    logout: "Logout"
  },
  zh: {
    title: "SHIS 現代化入口",
    subtitle: "第一階段前端骨架",
    login: "登入",
    username: "帳號",
    role: "角色",
    dashboard: "儀表板",
    admin: "管理",
    reports: "報表",
    logout: "登出"
  }
};

export function normalizeLocale(input: string): Locale {
  return supportedLocales.includes(input as Locale) ? (input as Locale) : "en";
}

export function dictionary(locale: string): Dictionary {
  return dictionaries[normalizeLocale(locale)];
}
