import type { Metadata } from 'next';
import { cookies } from 'next/headers';
import { AuthGuard } from '@/components/AuthGuard';
import { Sidebar } from '@/components/layout/Sidebar';
import { UserRole } from '@/types/auth';

// @ts-ignore - allow global CSS import without explicit type declarations
import './globals.css';

export const metadata: Metadata = {
  title: 'Mudanzas WMS',
  description: 'Logistics Management System',
};

export default async function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const cookieStore = await cookies();
  const token = cookieStore.get('accessToken')?.value;

  let role: UserRole = 'Operario';
  let userName = 'Usuario';
  let userEmail = 'usuario@empresa.com';

  if (token) {
    try {
      const payload = JSON.parse(
        Buffer.from(token.split('.')[1], 'base64').toString()
      );
      role =
        (payload[
          'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
        ] as UserRole) || 'Operario';
      userName =
        payload[
          'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'
        ] || 'Usuario';
      userEmail =
        payload[
          'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'
        ] || 'usuario@empresa.com';
    } catch {
    }
  }

  return (
    <html lang="es">
      <body className="bg-[#10151D] text-white flex min-h-screen">
        <AuthGuard>
          <div className="flex w-full min-h-screen">
            <Sidebar role={role} userName={userName} userEmail={userEmail} />
            <main className="flex-1 p-6 overflow-y-auto">{children}</main>
          </div>
        </AuthGuard>
      </body>
    </html>
  );
}