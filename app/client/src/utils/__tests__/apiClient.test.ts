import { describe, it, expect, beforeEach, vi, afterEach } from 'vitest';
import { apiClient } from '../apiClient';

globalThis.fetch = vi.fn();
const mockFetch = globalThis.fetch as ReturnType<typeof vi.fn>;

describe('ApiClient', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    sessionStorage.clear();
  });

  afterEach(() => {
    sessionStorage.clear();
  });

  describe('Authentication', () => {
    it('should include auth token when available', async () => {
      const mockToken = 'test-token-123';
      sessionStorage.setItem(
        'oidc.user:undefined:undefined',
        JSON.stringify({ access_token: mockToken })
      );

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => ({ data: 'test' }),
      } as Response);

      await apiClient.get('/test');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({
          headers: expect.objectContaining({
            'Authorization': `Bearer ${mockToken}`,
          }),
        })
      );
    });

    it('should work without auth token', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => ({ data: 'test' }),
      } as Response);

      await apiClient.get('/test');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({
          headers: expect.not.objectContaining({
            'Authorization': expect.any(String),
          }),
        })
      );
    });
  });

  describe('GET requests', () => {
    it('should make GET request with correct method', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => ({ id: 1, name: 'Test' }),
      } as Response);

      const result = await apiClient.get('/users/1');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/users/1'),
        expect.objectContaining({
          method: 'GET',
        })
      );
      expect(result).toEqual({ id: 1, name: 'Test' });
    });
  });

  describe('POST requests', () => {
    it('should make POST request with data', async () => {
      const postData = { name: 'New User' };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => ({ id: 1, ...postData }),
      } as Response);

      const result = await apiClient.post('/users', postData);

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/users'),
        expect.objectContaining({
          method: 'POST',
          body: JSON.stringify(postData),
        })
      );
      expect(result).toEqual({ id: 1, ...postData });
    });

    it('should make POST request without data', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => ({ success: true }),
      } as Response);

      await apiClient.post('/trigger-action');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({
          method: 'POST',
          body: undefined,
        })
      );
    });
  });

  describe('PUT requests', () => {
    it('should make PUT request with data', async () => {
      const updateData = { name: 'Updated User' };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => ({ id: 1, ...updateData }),
      } as Response);

      const result = await apiClient.put('/users/1', updateData);

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/users/1'),
        expect.objectContaining({
          method: 'PUT',
          body: JSON.stringify(updateData),
        })
      );
      expect(result).toEqual({ id: 1, ...updateData });
    });
  });

  describe('DELETE requests', () => {
    it('should make DELETE request', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 204,
      } as Response);

      await apiClient.delete('/users/1');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.stringContaining('/users/1'),
        expect.objectContaining({
          method: 'DELETE',
        })
      );
    });
  });

  describe('Error handling', () => {
    it('should throw error on 401 Unauthorized', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 401,
        json: async () => ({}),
      } as Response);

      await expect(apiClient.get('/protected')).rejects.toThrow(
        'Unauthorized - please sign in again'
      );
    });

    it('should throw error on 403 Forbidden', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 403,
        json: async () => ({}),
      } as Response);

      await expect(apiClient.get('/admin')).rejects.toThrow(
        'Forbidden - you do not have access to this resource'
      );
    });

    it('should throw error with message from response', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 400,
        json: async () => ({ message: 'Invalid input data' }),
      } as Response);

      await expect(apiClient.post('/users', {})).rejects.toThrow(
        'Invalid input data'
      );
    });

    it('should throw error with error field from response', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 500,
        json: async () => ({ error: 'Internal server error' }),
      } as Response);

      await expect(apiClient.get('/error')).rejects.toThrow(
        'Internal server error'
      );
    });

    it('should throw generic error when no message available', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 500,
        json: async () => {
          throw new Error('Invalid JSON');
        },
      } as unknown as Response);

      await expect(apiClient.get('/error')).rejects.toThrow('HTTP 500');
    });
  });

  describe('Response handling', () => {
    it('should return empty object on 204 No Content', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        status: 204,
      } as Response);

      const result = await apiClient.delete('/users/1');

      expect(result).toEqual({});
    });

    it('should include Content-Type header', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => ({}),
      } as Response);

      await apiClient.get('/test');

      expect(mockFetch).toHaveBeenCalledWith(
        expect.any(String),
        expect.objectContaining({
          headers: expect.objectContaining({
            'Content-Type': 'application/json',
          }),
        })
      );
    });
  });
});
