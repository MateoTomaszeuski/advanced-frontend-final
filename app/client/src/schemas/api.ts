import { z } from 'zod';

export const UserSchema = z.object({
  id: z.number(),
  email: z.string().email(),
  displayName: z.string().optional(),
  createdAt: z.string(),
  updatedAt: z.string(),
});

export const ConversationSchema = z.object({
  id: z.number(),
  title: z.string(),
  createdAt: z.string(),
  updatedAt: z.string(),
  actionCount: z.number().optional(),
});

export const AgentActionSchema = z.object({
  id: z.number(),
  actionType: z.string(),
  status: z.enum(['Processing', 'Completed', 'Failed', 'AwaitingApproval']),
  inputPrompt: z.string().optional(),
  parameters: z.unknown().optional(),
  result: z.unknown().optional(),
  errorMessage: z.string().optional(),
  createdAt: z.string(),
  completedAt: z.string().optional(),
});

export const PlaylistPreferencesSchema = z.object({
  maxTracks: z.number().min(1).max(100).optional(),
  genres: z.array(z.string()).optional(),
  mood: z.string().optional(),
  minEnergy: z.number().min(0).max(100).optional(),
  maxEnergy: z.number().min(0).max(100).optional(),
  minTempo: z.number().min(0).max(300).optional(),
  maxTempo: z.number().min(0).max(300).optional(),
});

export const CreateSmartPlaylistRequestSchema = z.object({
  conversationId: z.number(),
  prompt: z.string().min(1, 'Prompt is required'),
  preferences: PlaylistPreferencesSchema.optional(),
});

export const DiscoverNewMusicRequestSchema = z.object({
  conversationId: z.number(),
  limit: z.number().min(1).max(50).default(10),
  sourcePlaylistIds: z.array(z.string()).optional(),
});

export const SpotifyTrackSchema = z.object({
  id: z.string(),
  name: z.string(),
  artists: z.string(),
  uri: z.string(),
});

export const AgentActionResultSchema = z.object({
  playlistId: z.string(),
  playlistName: z.string(),
  playlistUri: z.string(),
  trackCount: z.number(),
  tracks: z.array(SpotifyTrackSchema),
});

export const AgentActionResponseSchema = z.object({
  actionId: z.number(),
  actionType: z.string(),
  status: z.string(),
  result: AgentActionResultSchema.optional(),
  errorMessage: z.string().optional(),
});

export const SpotifyConnectionStatusSchema = z.object({
  isConnected: z.boolean(),
  isTokenValid: z.boolean(),
  tokenExpiry: z.string().optional(),
});

export const ConnectSpotifyRequestSchema = z.object({
  accessToken: z.string().min(1),
  refreshToken: z.string().optional(),
  expiresIn: z.number().min(1),
});

export const DuplicateTrackSchema = z.object({
  id: z.string(),
  uri: z.string(),
  albumName: z.string(),
  releaseDate: z.string().optional(),
  popularity: z.number(),
  isRecommendedToKeep: z.boolean(),
});

export const DuplicateGroupSchema = z.object({
  trackName: z.string(),
  artists: z.array(z.string()),
  duplicates: z.array(DuplicateTrackSchema),
});

export const RemoveDuplicatesResponseSchema = z.object({
  playlistId: z.string(),
  playlistName: z.string(),
  totalDuplicateGroups: z.number(),
  totalDuplicateTracks: z.number(),
  duplicateGroups: z.array(DuplicateGroupSchema),
});

export const ScanDuplicatesRequestSchema = z.object({
  conversationId: z.number(),
  playlistId: z.string().min(1, 'Playlist ID is required'),
});

export const ConfirmRemoveDuplicatesRequestSchema = z.object({
  conversationId: z.number(),
  playlistId: z.string().min(1, 'Playlist ID is required'),
  trackUrisToRemove: z.array(z.string()),
});

export const SuggestedTrackSchema = z.object({
  id: z.string(),
  name: z.string(),
  artists: z.array(z.string()),
  uri: z.string(),
  reason: z.string(),
  popularity: z.number(),
});

export const SuggestMusicResponseSchema = z.object({
  playlistId: z.string(),
  playlistName: z.string(),
  context: z.string(),
  suggestionCount: z.number(),
  suggestions: z.array(SuggestedTrackSchema),
});

export const SuggestMusicRequestSchema = z.object({
  conversationId: z.number(),
  playlistId: z.string().min(1, 'Playlist ID is required'),
  context: z.string().min(1, 'Context is required'),
});

export const SpotifyPlaylistSchema = z.object({
  id: z.string(),
  name: z.string(),
  description: z.string().optional(),
  uri: z.string(),
  totalTracks: z.number(),
});

export type User = z.infer<typeof UserSchema>;
export type Conversation = z.infer<typeof ConversationSchema>;
export type AgentAction = z.infer<typeof AgentActionSchema>;
export type PlaylistPreferences = z.infer<typeof PlaylistPreferencesSchema>;
export type CreateSmartPlaylistRequest = z.infer<typeof CreateSmartPlaylistRequestSchema>;
export type DiscoverNewMusicRequest = z.infer<typeof DiscoverNewMusicRequestSchema>;
export type SpotifyTrack = z.infer<typeof SpotifyTrackSchema>;
export type AgentActionResult = z.infer<typeof AgentActionResultSchema>;
export type AgentActionResponse = z.infer<typeof AgentActionResponseSchema>;
export type SpotifyConnectionStatus = z.infer<typeof SpotifyConnectionStatusSchema>;
export type ConnectSpotifyRequest = z.infer<typeof ConnectSpotifyRequestSchema>;
export type DuplicateTrack = z.infer<typeof DuplicateTrackSchema>;
export type DuplicateGroup = z.infer<typeof DuplicateGroupSchema>;
export type RemoveDuplicatesResponse = z.infer<typeof RemoveDuplicatesResponseSchema>;
export type ScanDuplicatesRequest = z.infer<typeof ScanDuplicatesRequestSchema>;
export type ConfirmRemoveDuplicatesRequest = z.infer<typeof ConfirmRemoveDuplicatesRequestSchema>;
export type SuggestedTrack = z.infer<typeof SuggestedTrackSchema>;
export type SuggestMusicResponse = z.infer<typeof SuggestMusicResponseSchema>;
export type SuggestMusicRequest = z.infer<typeof SuggestMusicRequestSchema>;
export type SpotifyPlaylist = z.infer<typeof SpotifyPlaylistSchema>;
