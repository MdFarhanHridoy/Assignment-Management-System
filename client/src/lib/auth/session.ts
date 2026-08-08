import { UserDto, UserRole } from '../types';
import { getToken } from './token';

function base64UrlDecode(input: string): string {
  const base64 = input.replace(/-/g, '+').replace(/_/g, '/');
  const padded = base64.padEnd(
    base64.length + ((4 - (base64.length % 4)) % 4),
    '=',
  );
  if (typeof atob === 'function') {
    const binary = atob(padded);
    const bytes = Uint8Array.from(binary, (c) => c.charCodeAt(0));
    return new TextDecoder('utf-8').decode(bytes);
  }
  return Buffer.from(padded, 'base64').toString('utf-8');
}

// Decode JWT payload (base64url) without verification — client-side display only
export function decodeJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return null;
    return JSON.parse(base64UrlDecode(parts[1])) as Record<string, unknown>;
  } catch {
    return null;
  }
}

function findClaim(payload: Record<string, unknown>, names: string[]): unknown {
  const lookup: Record<string, unknown> = {};
  for (const [key, value] of Object.entries(payload)) {
    lookup[key.toLowerCase()] = value;
    const suffix = key.split('/').pop()?.toLowerCase();
    if (suffix) lookup[suffix] = value;
  }
  for (const name of names) {
    const value = lookup[name.toLowerCase()];
    if (value !== undefined && value !== null) return value;
  }
  return undefined;
}

function normalizeRole(value: unknown): UserRole | null {
  let candidate: unknown = value;
  if (Array.isArray(candidate)) {
    candidate = candidate.find((r) => typeof r === 'string') ?? null;
  }
  return typeof candidate === 'string' ? (candidate as UserRole) : null;
}

export function getUserRole(): UserRole | null {
  const token = getToken();
  if (!token) return null;
  const payload = decodeJwtPayload(token);
  if (!payload) return null;
  return normalizeRole(findClaim(payload, ['role', 'roles']));
}

export function getSessionUser(): Pick<UserDto, 'id' | 'name' | 'email' | 'role'> | null {
  const token = getToken();
  if (!token) return null;
  const payload = decodeJwtPayload(token);
  if (!payload) return null;
  const id = findClaim(payload, ['sub', 'id', 'userId', 'nameidentifier']);
  const name = findClaim(payload, ['name', 'unique_name', 'given_name']);
  const email = findClaim(payload, ['email', 'emailaddress']);
  const role = normalizeRole(findClaim(payload, ['role', 'roles']));
  if (!id || !role) return null;
  return {
    id: String(id),
    name: name ? String(name) : '',
    email: email ? String(email) : '',
    role,
  };
}

export function isAuthenticated(): boolean {
  return getToken() !== null;
}
