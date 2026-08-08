import { TOKEN_KEY } from '../constants';

export function getToken(): string | null {
  if (typeof window === 'undefined') return null;
  try {
    return window.localStorage.getItem(TOKEN_KEY);
  } catch {
    return null;
  }
}

export function setToken(token: string): void {
  if (typeof window === 'undefined') return;
  try {
    window.localStorage.setItem(TOKEN_KEY, token);
  } catch {
    // ignore storage errors (e.g. private mode / quota)
  }
  // Also set a cookie so Next.js Edge middleware can read the token for route protection.
  document.cookie = `${TOKEN_KEY}=${token}; path=/; max-age=7200; SameSite=Lax`;
}

export function clearToken(): void {
  if (typeof window === 'undefined') return;
  try {
    window.localStorage.removeItem(TOKEN_KEY);
  } catch {
    // ignore storage errors
  }
  // Clear the cookie used by middleware.
  document.cookie = `${TOKEN_KEY}=; path=/; max-age=0; SameSite=Lax`;
}
