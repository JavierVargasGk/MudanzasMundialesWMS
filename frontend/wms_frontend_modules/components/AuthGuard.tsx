'use client';

import { useEffect, ReactNode } from 'react';
import { usePathname } from 'next/navigation';

interface AuthGuardProps {
  children: ReactNode;
}

export function AuthGuard({ children }: AuthGuardProps): ReactNode {
  const pathname = usePathname();

  useEffect(() => {
    if (pathname === '/login') return;

    const checkAuth = async () => {
      try {
        const res = await fetch('http://localhost:5080/api/auth/me', {
          credentials: 'include',
        });

        if (res.status === 401) {
          window.location.href = '/login?reason=expired';
        }
      } catch {
      }
    };

    checkAuth();
    const interval = setInterval(checkAuth, 10000);

    return () => clearInterval(interval);
  }, [pathname]);

  return children;
}