export type {
  User,
  Conversation,
  AgentAction,
  PlaylistPreferences,
  CreateSmartPlaylistRequest,
  DiscoverNewMusicRequest,
  SpotifyTrack,
  AgentActionResult,
  AgentActionResponse,
  SpotifyConnectionStatus,
  ConnectSpotifyRequest,
} from '../schemas/api';

export interface CreateConversationRequest {
  title: string;
}
