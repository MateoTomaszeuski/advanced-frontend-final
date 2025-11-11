namespace API.DTOs.Spotify;

public record PlaylistAnalyticsResponse(
    string PlaylistId,
    string PlaylistName,
    int TotalTracks,
    AudioFeaturesStats AudioFeatures,
    Dictionary<string, int> Genres,
    Dictionary<int, int> Keys,
    Dictionary<int, int> Modes,
    Dictionary<string, int> DecadeDistribution
);

public record AudioFeaturesStats(
    float AvgDanceability,
    float AvgEnergy,
    float AvgValence,
    float AvgAcousticness,
    float AvgInstrumentalness,
    float AvgLiveness,
    float AvgSpeechiness,
    float AvgTempo,
    float AvgLoudness,
    float MinTempo,
    float MaxTempo,
    float MinEnergy,
    float MaxEnergy
);
