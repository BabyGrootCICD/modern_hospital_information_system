import { cookies } from "next/headers";

export type SessionRole = "doctor" | "admin" | "auditor";

export type Session = {
  username: string;
  role: SessionRole;
};

export async function getSession(): Promise<Session | null> {
  const cookieStore = await cookies();
  const raw = cookieStore.get("shis_session")?.value;
  if (!raw) {
    return null;
  }

  const [username, role] = raw.split(":");
  if (!username || !role) {
    return null;
  }

  if (role !== "doctor" && role !== "admin" && role !== "auditor") {
    return null;
  }

  return { username, role };
}
