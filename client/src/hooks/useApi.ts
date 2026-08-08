'use client';
import { useState, useCallback, useEffect, useRef } from 'react';
import { ApiError } from '@/lib/types';

interface UseApiReturn<T> {
  data: T | null;
  loading: boolean;
  error: string | null;
  refetch: () => void;
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

export function useApi<T>(fetcher: () => Promise<T>, deps: unknown[] = []): UseApiReturn<T> {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [refetchCount, setRefetchCount] = useState<number>(0);

  const fetcherRef = useRef(fetcher);
  fetcherRef.current = fetcher;

  useEffect(() => {
    let mounted = true;
    setLoading(true);
    setError(null);
    fetcherRef
      .current()
      .then((result) => {
        if (mounted) setData(result);
      })
      .catch((err) => {
        if (mounted) {
          setData(null);
          setError(extractErrorMessage(err, 'Something went wrong.'));
        }
      })
      .finally(() => {
        if (mounted) setLoading(false);
      });
    return () => {
      mounted = false;
    };
  }, [...deps, refetchCount]);

  const refetch = useCallback(() => {
    setRefetchCount((count) => count + 1);
  }, []);

  return { data, loading, error, refetch };
}
