using System.Text.Json;
using API.DTOs.Agent;
using API.Interfaces;
using API.Models;
using Microsoft.Extensions.Logging;

namespace API.Services.Helpers;

public class ToolExecutionHelper {
    private readonly IAgentService _agentService;
    private readonly ILogger _logger;

    public ToolExecutionHelper(IAgentService agentService, ILogger logger) {
        _agentService = agentService;
        _logger = logger;
    }

    public async Task<object> ExecuteCreateSmartPlaylistAsync(
        User user,
        int conversationId,
        Dictionary<string, object> arguments) {
        try {
            var prompt = arguments.GetValueOrDefault("prompt")?.ToString() ?? "";

            int? maxTracks = arguments.ContainsKey("maxTracks") ?
                ((JsonElement)arguments["maxTracks"]).GetInt32() : null;
            int? minEnergy = arguments.ContainsKey("minEnergy") ?
                ((JsonElement)arguments["minEnergy"]).GetInt32() : null;
            int? maxEnergy = arguments.ContainsKey("maxEnergy") ?
                ((JsonElement)arguments["maxEnergy"]).GetInt32() : null;
            int? minTempo = arguments.ContainsKey("minTempo") ?
                ((JsonElement)arguments["minTempo"]).GetInt32() : null;
            int? maxTempo = arguments.ContainsKey("maxTempo") ?
                ((JsonElement)arguments["maxTempo"]).GetInt32() : null;
            string[]? genres = arguments.ContainsKey("genres") ?
                ((JsonElement)arguments["genres"]).EnumerateArray().Select(g => g.GetString()!).ToArray() : null;
            string? mood = arguments.ContainsKey("mood") ? arguments["mood"]?.ToString() : null;

            var preferences = new PlaylistPreferences(maxTracks, genres, mood, minEnergy, maxEnergy, minTempo, maxTempo);
            var request = new CreateSmartPlaylistRequest(prompt, preferences);
            var result = await _agentService.CreateSmartPlaylistAsync(user, request, conversationId);

            return new {
                success = result.Status == "Completed",
                message = result.Status == "Completed" ? "Created playlist successfully" : result.ErrorMessage,
                result = result.Result
            };
        } catch (Exception ex) {
            _logger.LogError(ex, "Error executing create_smart_playlist");
            return new { success = false, message = ex.Message };
        }
    }

    public async Task<object> ExecuteDiscoverNewMusicAsync(
        User user,
        int conversationId,
        Dictionary<string, object> arguments) {
        try {
            var limit = arguments.ContainsKey("limit") ?
                ((JsonElement)arguments["limit"]).GetInt32() : 10;
            string[]? sourcePlaylistIds = arguments.ContainsKey("sourcePlaylistIds") ?
                ((JsonElement)arguments["sourcePlaylistIds"]).EnumerateArray().Select(p => p.GetString()!).ToArray() : null;

            var request = new DiscoverNewMusicRequest(limit, sourcePlaylistIds);
            var result = await _agentService.DiscoverNewMusicAsync(user, request, conversationId);

            return new {
                success = result.Status == "Completed",
                message = result.Status == "Completed" ? "Discovered new music successfully" : result.ErrorMessage,
                result = result.Result
            };
        } catch (Exception ex) {
            _logger.LogError(ex, "Error executing discover_new_music");
            return new { success = false, message = ex.Message };
        }
    }
}