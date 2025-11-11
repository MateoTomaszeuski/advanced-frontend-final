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
  PlaylistAnalytics,
  AudioFeaturesStats,
  UserActivityStats,
  ActionTypeStats,
  DuplicateStats,
  AppAnalytics,
} from '../schemas/api';

export interface CreateConversationRequest {
  title: string;
}
