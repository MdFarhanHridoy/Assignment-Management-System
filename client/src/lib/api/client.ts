import { ApiError } from '../types';
import { API_URL, ROUTES } from '../constants';
import { getToken, clearToken } from '../auth/token';

class ApiClient {
  private baseUrl: string;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  private buildUrl(path: string): string {
    return `${this.baseUrl}${path}`;
  }

  private async request<T>(path: string, options?: RequestInit): Promise<T> {
    const token = getToken();
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      ...((options?.headers as Record<string, string> | undefined) ?? {}),
    };
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    let response: Response;
    try {
      response = await fetch(this.buildUrl(path), {
        ...options,
        headers,
      });
    } catch {
      throw {
        message: 'Network error: unable to reach the server. Please check your connection.',
      } as ApiError;
    }

    if (response.status === 401) {
      clearToken();
      if (typeof window !== 'undefined') {
        window.location.href = ROUTES.LOGIN;
      }
    }

    if (!response.ok) {
      const fallback: ApiError = {
        message: `Request failed with status ${response.status}.`,
      };
      try {
        const text = await response.text();
        if (text) {
          fallback.message = (JSON.parse(text) as ApiError).message ?? fallback.message;
          fallback.errors = (JSON.parse(text) as ApiError).errors ?? fallback.errors;
        }
      } catch {
        // response was not valid JSON; keep the fallback error
      }
      throw fallback;
    }

    if (response.status === 204) {
      return undefined as unknown as T;
    }

    const text = await response.text();
    if (!text) {
      return undefined as unknown as T;
    }
    return JSON.parse(text) as T;
  }

  async get<T>(path: string): Promise<T> {
    return this.request<T>(path, { method: 'GET' });
  }

  async post<T>(path: string, body?: unknown): Promise<T> {
    return this.request<T>(path, {
      method: 'POST',
      body: body === undefined ? undefined : JSON.stringify(body),
    });
  }

  async put<T>(path: string, body: unknown): Promise<T> {
    return this.request<T>(path, {
      method: 'PUT',
      body: JSON.stringify(body),
    });
  }

  async del(path: string): Promise<void> {
    await this.request<void>(path, { method: 'DELETE' });
  }
}

export const apiClient = new ApiClient(API_URL);
