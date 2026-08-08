'use client';
import { useState, useCallback, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { authApi } from '@/lib/api/endpoints';
import { getToken, setToken, clearToken } from '@/lib/auth/token';
import { UserDto, AuthResponse, ApiError } from '@/lib/types';
import { ROUTES } from '@/lib/constants';

interface UseAuthReturn {
  user: UserDto | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<{ success: boolean; error?: string }>;
  logout: () => void;
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

export function useAuth(): UseAuthReturn {
  const router = useRouter();
  const [user, setUser] = useState<UserDto | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);

  useEffect(() => {
    let mounted = true;
    const token = getToken();
    if (!token) {
      setIsLoading(false);
      return;
    }
    authApi
      .me()
      .then((currentUser) => {
        if (mounted) setUser(currentUser);
      })
      .catch(() => {
        if (mounted) setUser(null);
      })
      .finally(() => {
        if (mounted) setIsLoading(false);
      });
    return () => {
      mounted = false;
    };
  }, []);

  const login = useCallback(
    async (
      email: string,
      password: string,
    ): Promise<{ success: boolean; error?: string }> => {
      try {
        const response: AuthResponse = await authApi.login({ email, password });
        setToken(response.token);
        setUser(response.user);
        return { success: true };
      } catch (err) {
        return {
          success: false,
          error: extractErrorMessage(err, 'Login failed. Please try again.'),
        };
      }
    },
    [],
  );

  const logout = useCallback(() => {
    clearToken();
    setUser(null);
    router.push(ROUTES.LOGIN);
  }, [router]);

  return {
    user,
    isAuthenticated: user !== null,
    isLoading,
    login,
    logout,
  };
}
