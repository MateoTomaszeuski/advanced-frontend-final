namespace API.DTOs.Agent;

public record RemoveDuplicatesRequest(
    string PlaylistId
);

public record DuplicateGroup(
    string TrackName,
    string[] Artists,
    DuplicateTrack[] Duplicates
);

public record DuplicateTrack(
    string Id,
    string Uri,
    string AlbumName,
    DateTime? ReleaseDate,
    int Popularity,
    bool IsRecommendedToKeep
);

public record RemoveDuplicatesResponse(
    string PlaylistId,
    string PlaylistName,
    int TotalDuplicateGroups,
    int TotalDuplicateTracks,
    DuplicateGroup[] DuplicateGroups
);

public record ConfirmRemoveDuplicatesRequest(
    string PlaylistId,
    string[] TrackUrisToRemove
);
