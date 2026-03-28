import type { Metadata } from "next";
import "./styles.css";

export const metadata: Metadata = {
  title: "SHIS Modern Web",
  description: "Next.js frontend for SHIS modernization"
};

export default function RootLayout({
  children
}: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  );
}
