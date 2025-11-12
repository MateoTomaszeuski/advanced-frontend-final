import { describe, it, expect, beforeEach, vi } from 'vitest';
import { renderHook, act, waitFor } from '@testing-library/react';
import { useAgent } from '../useAgent';
import { useAgentStore } from '../../stores/useAgentStore';
import { conversationApi, agentApi } from '../../services/api';
import toast from 'react-hot-toast';

vi.mock('../../services/api');
vi.mock('react-hot-toast');

describe('useAgent', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    useAgentStore.setState({
      status: 'idle',
      currentTask: null,
      progress: 0,
      currentConversation: null,
      conversations: [],
      recentActions: [],
    });
  });

  describe('createConversation', () => {
    it('should create conversation successfully', async () => {
      const mockConversation = {
        id: 1,
        userId: 1,
        title: 'Test Conversation',
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      };

      vi.mocked(conversationApi.create).mockResolvedValueOnce(mockConversation);

      const { result } = renderHook(() => useAgent());

      let conversation;
      await act(async () => {
        conversation = await result.current.createConversation('Test Conversation');
      });

      expect(conversation).toEqual(mockConversation);
      expect(conversationApi.create).toHaveBeenCalledWith({ title: 'Test Conversation' });
      expect(toast.success).toHaveBeenCalledWith(
        'Conversation created',
        expect.objectContaining({
          duration: 4000,
          style: expect.any(Object),
          iconTheme: expect.any(Object)
        })
      );

      const state = useAgentStore.getState();
      expect(state.conversations).toContainEqual(mockConversation);
      expect(state.currentConversation).toEqual(mockConversation);
    });

    it('should handle conversation creation error', async () => {
      const error = new Error('Failed to create');
      vi.mocked(conversationApi.create).mockRejectedValueOnce(error);

      const { result } = renderHook(() => useAgent());

      await expect(
        act(async () => {
          await result.current.createConversation('Test');
        })
      ).rejects.toThrow('Failed to create');

      expect(toast.error).toHaveBeenCalledWith(
        'Failed to create',
        expect.objectContaining({
          duration: Infinity,
          style: expect.any(Object),
          iconTheme: expect.any(Object)
        })
      );
    });
  });

  describe('createSmartPlaylist', () => {
    it('should create smart playlist successfully', async () => {
      const mockAction = {
        id: 1,
        conversationId: 1,
        actionType: 'CreateSmartPlaylist',
        status: 'Completed' as const,
        createdAt: '2024-01-01T00:00:00Z',
      };

      const mockResponse = {
        actionId: 1,
        actionType: 'CreateSmartPlaylist',
        status: 'Completed',
        result: {
          playlistId: 'pl123',
          playlistName: 'My Playlist',
          playlistUri: 'spotify:playlist:pl123',
          trackCount: 25,
          tracks: [],
        },
      };

      vi.mocked(agentApi.createSmartPlaylist).mockResolvedValueOnce(mockResponse);
      vi.mocked(agentApi.getAction).mockResolvedValueOnce(mockAction);

      const { result } = renderHook(() => useAgent());

      await act(async () => {
        await result.current.createSmartPlaylist({
          conversationId: 1,
          prompt: 'Create a rock playlist',
        });
      });

      expect(result.current.isLoading).toBe(false);
      expect(agentApi.createSmartPlaylist).toHaveBeenCalled();
      expect(toast.success).toHaveBeenCalledWith(
        'Playlist "My Playlist" created with 25 tracks!',
        expect.objectContaining({
          duration: 4000,
          style: expect.any(Object),
          iconTheme: expect.any(Object)
        })
      );

      const state = useAgentStore.getState();
      expect(state.status).toBe('idle');
      expect(state.recentActions).toContainEqual(mockAction);
    });

    it('should handle failed playlist creation', async () => {
      const mockResponse = {
        actionId: 1,
        actionType: 'CreateSmartPlaylist',
        status: 'Failed',
        errorMessage: 'Invalid prompt',
      };

      vi.mocked(agentApi.createSmartPlaylist).mockResolvedValueOnce(mockResponse);

      const { result } = renderHook(() => useAgent());

      await expect(
        act(async () => {
          await result.current.createSmartPlaylist({
            conversationId: 1,
            prompt: 'Invalid',
          });
        })
      ).rejects.toThrow('Invalid prompt');

      expect(toast.error).toHaveBeenCalledWith(
        'Invalid prompt',
        expect.objectContaining({
          duration: Infinity,
          style: expect.any(Object),
          iconTheme: expect.any(Object)
        })
      );

      const state = useAgentStore.getState();
      expect(state.status).toBe('error');
    });

    it('should update progress during creation', async () => {
      const mockResponse = {
        actionId: 1,
        actionType: 'CreateSmartPlaylist',
        status: 'Completed',
        result: {
          playlistId: 'pl123',
          playlistName: 'Test',
          playlistUri: 'spotify:playlist:pl123',
          trackCount: 10,
          tracks: [],
        },
      };

      vi.mocked(agentApi.createSmartPlaylist).mockResolvedValueOnce(mockResponse);
      vi.mocked(agentApi.getAction).mockResolvedValueOnce({
        id: 1,
        conversationId: 1,
        actionType: 'CreateSmartPlaylist',
        status: 'Completed' as const,
        createdAt: '2024-01-01T00:00:00Z',
      });

      const { result } = renderHook(() => useAgent());

      await act(async () => {
        await result.current.createSmartPlaylist({
          conversationId: 1,
          prompt: 'Test',
        });
      });

      const state = useAgentStore.getState();
      expect(state.progress).toBe(100);
    });
  });

  describe('discoverNewMusic', () => {
    it('should discover new music successfully', async () => {
      const mockAction = {
        id: 2,
        conversationId: 1,
        actionType: 'DiscoverNewMusic',
        status: 'Completed' as const,
        createdAt: '2024-01-01T00:00:00Z',
      };

      const mockResponse = {
        actionId: 2,
        actionType: 'DiscoverNewMusic',
        status: 'Completed',
        result: {
          playlistId: 'pl456',
          playlistName: 'Discovered Tracks',
          playlistUri: 'spotify:playlist:pl456',
          trackCount: 15,
          tracks: [],
        },
      };

      vi.mocked(agentApi.discoverNewMusic).mockResolvedValueOnce(mockResponse);
      vi.mocked(agentApi.getAction).mockResolvedValueOnce(mockAction);

      const { result } = renderHook(() => useAgent());

      await act(async () => {
        await result.current.discoverNewMusic({
          conversationId: 1,
          limit: 10,
        });
      });

      expect(toast.success).toHaveBeenCalledWith(
        'Discovered 15 new tracks in "Discovered Tracks"!',
        expect.objectContaining({
          duration: 4000,
          style: expect.any(Object),
          iconTheme: expect.any(Object)
        })
      );

      const state = useAgentStore.getState();
      expect(state.status).toBe('idle');
    });

    it('should handle discovery error', async () => {
      const error = new Error('Discovery failed');
      vi.mocked(agentApi.discoverNewMusic).mockRejectedValueOnce(error);

      const { result } = renderHook(() => useAgent());

      await expect(
        act(async () => {
          await result.current.discoverNewMusic({
            conversationId: 1,
            limit: 10,
          });
        })
      ).rejects.toThrow('Discovery failed');

      expect(toast.error).toHaveBeenCalledWith(
        'Discovery failed',
        expect.objectContaining({
          duration: Infinity,
          style: expect.any(Object),
          iconTheme: expect.any(Object)
        })
      );
    });
  });

  describe('scanDuplicates', () => {
    it('should scan for duplicates successfully', async () => {
      const mockResponse = {
        totalDuplicateTracks: 5,
        totalDuplicateGroups: 2,
        duplicateGroups: [],
      };

      vi.mocked(agentApi.scanDuplicates).mockResolvedValueOnce(mockResponse);

      const { result } = renderHook(() => useAgent());

      await act(async () => {
        await result.current.scanDuplicates(1, 'playlist123');
      });

      expect(toast.success).toHaveBeenCalledWith(
        'Found 5 duplicates in 2 groups',
        expect.objectContaining({
          duration: 4000,
          style: expect.any(Object),
          iconTheme: expect.any(Object)
        })
      );
    });

    it('should handle no duplicates found', async () => {
      const mockResponse = {
        totalDuplicateTracks: 0,
        totalDuplicateGroups: 0,
        duplicateGroups: [],
      };

      vi.mocked(agentApi.scanDuplicates).mockResolvedValueOnce(mockResponse);

      const { result } = renderHook(() => useAgent());

      await act(async () => {
        await result.current.scanDuplicates(1, 'playlist123');
      });

      expect(toast.success).toHaveBeenCalledWith(
        'No duplicates found in this playlist',
        expect.objectContaining({
          duration: 4000,
          style: expect.any(Object),
          iconTheme: expect.any(Object)
        })
      );
    });
  });

  describe('confirmRemoveDuplicates', () => {
    it('should remove duplicates successfully', async () => {
      const mockAction = {
        id: 3,
        conversationId: 1,
        actionType: 'RemoveDuplicates',
        status: 'Completed' as const,
        createdAt: '2024-01-01T00:00:00Z',
      };

      const mockResponse = {
        actionId: 3,
        actionType: 'RemoveDuplicates',
        status: 'Completed',
      };

      vi.mocked(agentApi.confirmRemoveDuplicates).mockResolvedValueOnce(mockResponse);
      vi.mocked(agentApi.getAction).mockResolvedValueOnce(mockAction);

      const { result } = renderHook(() => useAgent());

      await act(async () => {
        await result.current.confirmRemoveDuplicates(1, 'playlist123', ['uri1', 'uri2']);
      });

      expect(toast.success).toHaveBeenCalledWith(
        'Removed 2 duplicate tracks!',
        expect.objectContaining({
          duration: 4000,
          style: expect.any(Object),
          iconTheme: expect.any(Object)
        })
      );
    });
  });

  describe('suggestMusic', () => {
    it('should suggest music successfully', async () => {
      const mockResponse = {
        playlistName: 'Test Playlist',
        suggestionCount: 5,
        suggestions: [],
      };

      vi.mocked(agentApi.suggestMusic).mockResolvedValueOnce(mockResponse);

      const { result } = renderHook(() => useAgent());

      await act(async () => {
        await result.current.suggestMusic(1, 'playlist123', 'more energetic songs');
      });

      expect(toast.success).toHaveBeenCalledWith(
        'Generated 5 suggestions for "Test Playlist"',
        expect.objectContaining({
          duration: 4000,
          style: expect.any(Object),
          iconTheme: expect.any(Object)
        })
      );
    });

    it('should use default limit when not provided', async () => {
      const mockResponse = {
        playlistName: 'Test',
        suggestionCount: 10,
        suggestions: [],
      };

      vi.mocked(agentApi.suggestMusic).mockResolvedValueOnce(mockResponse);

      const { result } = renderHook(() => useAgent());

      await act(async () => {
        await result.current.suggestMusic(1, 'playlist123', 'context');
      });

      expect(agentApi.suggestMusic).toHaveBeenCalledWith({
        conversationId: 1,
        playlistId: 'playlist123',
        context: 'context',
        limit: 10,
      });
    });
  });

  describe('loading state', () => {
    it('should set loading state during operations', async () => {
      const mockResponse = {
        actionId: 1,
        actionType: 'CreateSmartPlaylist',
        status: 'Completed',
        result: {
          playlistId: 'pl123',
          playlistName: 'Test',
          playlistUri: 'spotify:playlist:pl123',
          trackCount: 10,
          tracks: [],
        },
      };

      vi.mocked(agentApi.createSmartPlaylist).mockImplementation(async () => {
        await new Promise(resolve => setTimeout(resolve, 100));
        return mockResponse;
      });

      vi.mocked(agentApi.getAction).mockResolvedValueOnce({
        id: 1,
        conversationId: 1,
        actionType: 'CreateSmartPlaylist',
        status: 'Completed' as const,
        createdAt: '2024-01-01T00:00:00Z',
      });

      const { result } = renderHook(() => useAgent());

      expect(result.current.isLoading).toBe(false);

      const promise = act(async () => {
        await result.current.createSmartPlaylist({
          conversationId: 1,
          prompt: 'Test',
        });
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      await promise;
    });
  });
});
