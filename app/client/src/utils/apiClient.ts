import { config } from '../config';

class ApiClient {
  private baseUrl: string;

  constructor() {
    this.baseUrl = config.api.baseUrl;
  }

  private async getAuthToken(): Promise<string | null> {
    const oidcStorage = sessionStorage.getItem(
      `oidc.user:${config.keycloak.authority}:${config.keycloak.clientId}`
    );

    if (!oidcStorage) {
      return null;
    }

    try {
      const user = JSON.parse(oidcStorage);
      return user.access_token;
    } catch (error) {
      console.error('Failed to parse OIDC storage:', error);
      return null;
    }
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const token = await this.getAuthToken();

    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      ...(options.headers as Record<string, string>),
    };

    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    const url = `${this.baseUrl}${endpoint}`;

    try {
      const response = await fetch(url, {
        ...options,
        headers,
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => null);
        
        if (response.status === 401) {
          throw new Error('Your session has expired. Please sign in again.');
        }

        if (response.status === 403) {
          throw new Error('You do not have permission to perform this action.');
        }

        if (response.status === 404) {
          throw new Error('The requested resource was not found.');
        }

        if (response.status === 500) {
          throw new Error('A server error occurred. Please try again later.');
        }

        if (response.status === 400) {
          const message = errorData?.error || errorData?.message || 'Invalid request. Please check your input and try again.';
          throw new Error(message);
        }

        const message = errorData?.error || errorData?.message || 'An unexpected error occurred. Please try again.';
        throw new Error(message);
      }

      if (response.status === 204) {
        return {} as T;
      }

      return response.json();
    } catch (error) {
      if (error instanceof Error) {
        throw error;
      }
      
      if (typeof error === 'string') {
        throw new Error(error);
      }
      
      throw new Error('Network error. Please check your connection and try again.');
    }
  }

  async get<T>(endpoint: string): Promise<T> {
    return this.request<T>(endpoint, { method: 'GET' });
  }

  async post<T>(endpoint: string, data?: unknown): Promise<T> {
    return this.request<T>(endpoint, {
      method: 'POST',
      body: data ? JSON.stringify(data) : undefined,
    });
  }

  async put<T>(endpoint: string, data?: unknown): Promise<T> {
    return this.request<T>(endpoint, {
      method: 'PUT',
      body: data ? JSON.stringify(data) : undefined,
    });
  }

  async delete<T>(endpoint: string): Promise<T> {
    return this.request<T>(endpoint, { method: 'DELETE' });
  }
}

export const apiClient = new ApiClient();
