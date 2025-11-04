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

  getAction: (actionId: number) =>
    apiClient.get<AgentAction>(`/api/agent/actions/${actionId}`),
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
};

export const testApi = {
  me: () => apiClient.get('/api/test/me'),
  ping: () => apiClient.get('/api/test/ping'),
};
