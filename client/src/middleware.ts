import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';
import { TOKEN_KEY } from '@/lib/constants';

const PUBLIC_ROUTES = ['/login'];

const ROUTE_ROLE_PREFIX: Record<string, string> = {
  '/admin': 'Admin',
  '/teacher': 'Teacher',
  '/student': 'Student',
};

const ROLE_DASHBOARD: Record<string, string> = {
  Admin: '/admin/dashboard',
  Teacher: '/teacher/dashboard',
  Student: '/student/dashboard',
};

function decodeJwt(token: string): { role?: string } | null {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return null;
    const payload = parts[1];
    // Base64url decode
    const jsonStr = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
    return JSON.parse(jsonStr);
  } catch {
    return null;
  }
}

function getRoleFromToken(token: string): string | null {
  const payload = decodeJwt(token);
  if (!payload) return null;
  const role = payload.role;
  if (Array.isArray(role)) {
    const first = role.find((r) => typeof r === 'string');
    return typeof first === 'string' ? first : null;
  }
  return typeof role === 'string' ? role : null;
}

function getRequiredRole(pathname: string): string | null {
  for (const prefix of Object.keys(ROUTE_ROLE_PREFIX)) {
    if (pathname === prefix || pathname.startsWith(`${prefix}/`)) {
      return ROUTE_ROLE_PREFIX[prefix];
    }
  }
  return null;
}

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;
  const token = request.cookies.get(TOKEN_KEY)?.value;

  // Public routes (e.g. /login)
  if (PUBLIC_ROUTES.includes(pathname)) {
    if (token) {
      const role = getRoleFromToken(token);
      if (role && ROLE_DASHBOARD[role]) {
        return NextResponse.redirect(new URL(ROLE_DASHBOARD[role], request.url));
      }
    }
    return NextResponse.next();
  }

  const requiredRole = getRequiredRole(pathname);

  // Non-role routes (e.g. "/") are handled client-side; let them through.
  if (!requiredRole) {
    return NextResponse.next();
  }

  // Protected role route: require a valid token.
  if (!token) {
    return NextResponse.redirect(new URL('/login', request.url));
  }

  const role = getRoleFromToken(token);
  if (!role || !ROLE_DASHBOARD[role]) {
    return NextResponse.redirect(new URL('/login', request.url));
  }

  // Role mismatch: send the user to their own dashboard.
  if (role !== requiredRole) {
    return NextResponse.redirect(new URL(ROLE_DASHBOARD[role], request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: ['/((?!_next/static|_next/image|favicon.ico).*)'],
};
