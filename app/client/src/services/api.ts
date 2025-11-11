import { apiClient } from '../utils/apiClient';
import type {
  Conversation,
  CreateConversationRequest,
  CreateSmartPlaylistRequest,
  DiscoverNewMusicRequest,
  AgentActionResponse,
  AgentAction,
  SpotifyConnectionStatus,
  ConnectSpotifyRequest,
  PlaylistAnalytics,
} from '../types/api';

export const conversationApi = {
  getAll: () => apiClient.get<Conversation[]>('/api/conversations'),

  getById: (id: number) => apiClient.get<Conversation>(`/api/conversations/${id}`),

  create: (data: CreateConversationRequest) =>
    apiClient.post<Conversation>('/api/conversations', data),

  delete: (id: number) => apiClient.delete(`/api/conversations/${id}`),
};

export const agentApi = {
  createSmartPlaylist: (data: CreateSmartPlaylistRequest) =>
    apiClient.post<AgentActionResponse>('/api/agent/create-smart-playlist', data),

  discoverNewMusic: (data: DiscoverNewMusicRequest) =>
    apiClient.post<AgentActionResponse>('/api/agent/discover-new-music', data),

  scanDuplicates: (data: { conversationId: number; playlistId: string }) =>
    apiClient.post('/api/agent/scan-duplicates', data),

  confirmRemoveDuplicates: (data: { conversationId: number; playlistId: string; trackUrisToRemove: string[] }) =>
    apiClient.post<AgentActionResponse>('/api/agent/confirm-remove-duplicates', data),

  suggestMusic: (data: { conversationId: number; playlistId: string; context: string; limit?: number }) =>
    apiClient.post('/api/agent/suggest-music', data),

  getAction: (actionId: number) =>
    apiClient.get<AgentAction>(`/api/agent/actions/${actionId}`),

  getRecentPlaylists: (limit?: number) =>
    apiClient.get(`/api/agent/recent-playlists${limit ? `?limit=${limit}` : ''}`),

  getHistory: (params?: { actionType?: string; status?: string; limit?: number }) => {
    const queryParams = new URLSearchParams();
    if (params?.actionType) queryParams.append('actionType', params.actionType);
    if (params?.status) queryParams.append('status', params.status);
    if (params?.limit) queryParams.append('limit', params.limit.toString());
    const queryString = queryParams.toString();
    return apiClient.get<AgentAction[]>(`/api/agent/history${queryString ? `?${queryString}` : ''}`);
  },

  getAnalytics: () =>
    apiClient.get('/api/agent/analytics'),
};

export const spotifyApi = {
  connect: (data: ConnectSpotifyRequest) =>
    apiClient.post('/api/spotify/connect', data),

  exchangeCode: (code: string, redirectUri: string) =>
    apiClient.post('/api/spotify/exchange-code', { code, redirectUri }),

  getStatus: () =>
    apiClient.get<SpotifyConnectionStatus>('/api/spotify/status'),

  getProfile: () =>
    apiClient.get('/api/spotify/profile'),

  disconnect: () => apiClient.post('/api/spotify/disconnect'),

  getPlaylists: () => apiClient.get('/api/spotify/playlists'),

  addTracksToPlaylist: (playlistId: string, trackUris: string[]) =>
    apiClient.post(`/api/spotify/playlists/${playlistId}/tracks`, { trackUris }),

  getPlaylistAnalytics: (playlistId: string) =>
    apiClient.get<PlaylistAnalytics>(`/api/spotify/playlists/${playlistId}/analytics`),
};

export const testApi = {
  me: () => apiClient.get('/api/test/me'),
  ping: () => apiClient.get('/api/test/ping'),
};
