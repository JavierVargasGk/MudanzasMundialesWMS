import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

interface JwtPayload {
  exp?: number;
  role?: string;
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string;
  clientId?: string;
}

function decodeJwtPayload(token: string): JwtPayload | null {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return null;
    const base64Url = parts[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    return JSON.parse(jsonPayload);
  } catch {
    return null;
  }
}

const ROLE_ROUTES: Record<string, string[]> = {
  Administrador: ['/dashboard', '/inventory', '/operations', '/clients'],
  OperadorBodega: ['/dashboard', '/inventory', '/operations'],
  Analista: ['/dashboard', '/inventory'],
};

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;
  const token = request.cookies.get('accessToken')?.value;
  const isPublicRoute = pathname === '/login';

  // 1. Unauthenticated handling
  if (!token) {
    if (!isPublicRoute) {
      const loginUrl = new URL('/login', request.url);
      loginUrl.searchParams.set('from', pathname);
      return NextResponse.redirect(loginUrl);
    }
    return NextResponse.next();
  }

  const payload = decodeJwtPayload(token);
  
  // 2. Expiration check (exp in seconds vs Date.now in ms)
  const isExpired = !payload || !payload.exp || payload.exp * 1000 < Date.now();

  if (isExpired) {
    if (isPublicRoute) {
      const response = NextResponse.next();
      response.cookies.delete('accessToken');
      response.cookies.delete('refreshToken');
      return response;
    }

    const loginUrl = new URL('/login', request.url);
    loginUrl.searchParams.set('reason', 'expired');
    const response = NextResponse.redirect(loginUrl);
    response.cookies.delete('accessToken');
    response.cookies.delete('refreshToken');
    return response;
  }

  // 3. Prevent logged-in users from accessing /login
  if (isPublicRoute) {
    return NextResponse.redirect(new URL('/dashboard', request.url));
  }

  // 4. Role Authorization Check
  const userRole =
    payload.role ||
    payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
    '';

  const allowedRoutes = ROLE_ROUTES[userRole] || [];
  const isAuthorized = allowedRoutes.some((route) => pathname.startsWith(route));

  if (!isAuthorized) {
    if (pathname === '/dashboard') {
      const response = NextResponse.redirect(new URL('/login', request.url));
      response.cookies.delete('accessToken');
      return response;
    }
    return NextResponse.redirect(new URL('/dashboard', request.url));
  }

  const requestHeaders = new Headers(request.headers);
  if (payload.clientId) {
    requestHeaders.set('x-tenant-id', payload.clientId);
  }

  return NextResponse.next({
    request: {
      headers: requestHeaders,
    },
  });
}

export const config = {
  matcher: ['/((?!api|_next/static|_next/image|favicon.ico).*)'],
};