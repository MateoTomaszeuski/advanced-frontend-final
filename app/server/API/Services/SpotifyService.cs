using API.DTOs.Spotify;
using API.Interfaces;

namespace API.Services;

public class SpotifyService : ISpotifyService {
    private readonly ISpotifyAuthService _authService;
    private readonly ISpotifyUserService _userService;
    private readonly ISpotifyPlaylistService _playlistService;
    private readonly ISpotifyTrackService _trackService;

    public SpotifyService(
        ISpotifyAuthService authService,
        ISpotifyUserService userService,
        ISpotifyPlaylistService playlistService,
        ISpotifyTrackService trackService) {
        _authService = authService;
        _userService = userService;
        _playlistService = playlistService;
        _trackService = trackService;
    }

    public async Task<string> RefreshAccessTokenAsync(string refreshToken)
        => await _authService.RefreshAccessTokenAsync(refreshToken);

    public async Task<string> GetCurrentUserIdAsync(string accessToken)
        => await _userService.GetCurrentUserIdAsync(accessToken);

    public async Task<SpotifyUserProfile> GetCurrentUserProfileAsync(string accessToken)
        => await _userService.GetCurrentUserProfileAsync(accessToken);

    public async Task<SpotifyPlaylist> CreatePlaylistAsync(string accessToken, string userId, CreatePlaylistRequest request)
        => await _playlistService.CreatePlaylistAsync(accessToken, userId, request);

    public async Task<SpotifyPlaylist> GetPlaylistAsync(string accessToken, string playlistId)
        => await _playlistService.GetPlaylistAsync(accessToken, playlistId);

    public async Task<SpotifyPlaylist[]> GetUserPlaylistsAsync(string accessToken, int limit = 50)
        => await _playlistService.GetUserPlaylistsAsync(accessToken, limit);

    public async Task AddTracksToPlaylistAsync(string accessToken, string playlistId, string[] trackUris)
        => await _playlistService.AddTracksToPlaylistAsync(accessToken, playlistId, trackUris);

    public async Task RemoveTracksFromPlaylistAsync(string accessToken, string playlistId, string[] trackUris)
        => await _playlistService.RemoveTracksFromPlaylistAsync(accessToken, playlistId, trackUris);

    public async Task<SpotifyTrack[]> SearchTracksAsync(string accessToken, string query, int limit = 20)
        => await _trackService.SearchTracksAsync(accessToken, query, limit);

    public async Task<SpotifyTrack[]> GetRecommendationsAsync(string accessToken, string[] seedTracks, int limit = 20)
        => await _trackService.GetRecommendationsAsync(accessToken, seedTracks, limit);

    public async Task<AudioFeatures[]> GetAudioFeaturesAsync(string accessToken, string[] trackIds)
        => await _trackService.GetAudioFeaturesAsync(accessToken, trackIds);

    public async Task<SpotifyTrack[]> GetUserSavedTracksAsync(string accessToken, int limit = 50)
        => await _trackService.GetUserSavedTracksAsync(accessToken, limit);

    public async Task<SpotifyPlaylistTrack[]> GetPlaylistTracksAsync(string accessToken, string playlistId)
        => await _trackService.GetPlaylistTracksAsync(accessToken, playlistId);
}