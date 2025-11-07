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
  DuplicateTrack,
  DuplicateGroup,
  RemoveDuplicatesResponse,
  ScanDuplicatesRequest,
  ConfirmRemoveDuplicatesRequest,
  SuggestedTrack,
  SuggestMusicResponse,
  SuggestMusicRequest,
  SpotifyPlaylist,
} from '../schemas/api';

export interface CreateConversationRequest {
  title: string;
}
