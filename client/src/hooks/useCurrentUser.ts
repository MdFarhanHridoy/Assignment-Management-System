'use client';
import { useState, useCallback, useEffect } from 'react';
import { authApi } from '@/lib/api/endpoints';
import { getToken } from '@/lib/auth/token';
import { UserDto, ApiError } from '@/lib/types';

interface UseCurrentUserReturn {
  user: UserDto | null;
  loading: boolean;
  error: string | null;
  refresh: () => Promise<void>;
}

function extractErrorMessage(err: unknown, fallback: string): string {
  if (err && typeof err === 'object' && 'message' in err) {
    const message = (err as ApiError).message;
    if (typeof message === 'string' && message.trim()) {
      return message;
    }
  }
  return fallback;
}

export function useCurrentUser(): UseCurrentUserReturn {
  const [user, setUser] = useState<UserDto | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const fetchUser = useCallback(async () => {
    const token = getToken();
    if (!token) {
      setUser(null);
      setError(null);
      setLoading(false);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const currentUser = await authApi.me();
      setUser(currentUser);
    } catch (err) {
      setUser(null);
      setError(extractErrorMessage(err, 'Failed to load user.'));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void fetchUser();
  }, [fetchUser]);

  const refresh = useCallback(async () => {
    await fetchUser();
  }, [fetchUser]);

  return { user, loading, error, refresh };
}
