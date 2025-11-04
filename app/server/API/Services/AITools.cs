using API.Models.AI;

namespace API.Services;

public static class AITools
{
    public static List<AITool> GetSpotifyTools()
    {
        return new List<AITool>
        {
            new AITool(
                "function",
                new AIToolFunction(
                    "create_smart_playlist",
                    "Create a smart playlist on Spotify based on natural language description. The AI will interpret the user's mood, genre preferences, and activity to create a curated playlist.",
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            prompt = new
                            {
                                type = "string",
                                description = "Natural language description of the desired playlist (e.g., 'energetic workout music', 'chill study vibes', 'party music')"
                            },
                            maxTracks = new
                            {
                                type = "integer",
                                description = "Maximum number of tracks to include in the playlist (1-100, default: 20)",
                                minimum = 1,
                                maximum = 100
                            },
                            genres = new
                            {
                                type = "array",
                                description = "Optional array of genre names to filter tracks",
                                items = new { type = "string" }
                            },
                            mood = new
                            {
                                type = "string",
                                description = "Optional mood descriptor (e.g., 'happy', 'sad', 'energetic', 'calm')"
                            },
                            minEnergy = new
                            {
                                type = "integer",
                                description = "Minimum energy level (0-100)",
                                minimum = 0,
                                maximum = 100
                            },
                            maxEnergy = new
                            {
                                type = "integer",
                                description = "Maximum energy level (0-100)",
                                minimum = 0,
                                maximum = 100
                            },
                            minTempo = new
                            {
                                type = "integer",
                                description = "Minimum tempo in BPM (0-300)",
                                minimum = 0,
                                maximum = 300
                            },
                            maxTempo = new
                            {
                                type = "integer",
                                description = "Maximum tempo in BPM (0-300)",
                                minimum = 0,
                                maximum = 300
                            }
                        },
                        required = new[] { "prompt" }
                    }
                )
            ),
            new AITool(
                "function",
                new AIToolFunction(
                    "discover_new_music",
                    "Discover new music recommendations based on the user's Spotify listening history. Creates a discovery playlist with tracks the user hasn't saved yet.",
                    new
                    {
                        type = "object",
                        properties = new
                        {
                            limit = new
                            {
                                type = "integer",
                                description = "Number of track recommendations to generate (1-50, default: 10)",
                                minimum = 1,
                                maximum = 50
                            },
                            sourcePlaylistIds = new
                            {
                                type = "array",
                                description = "Optional array of Spotify playlist IDs to use as seed for recommendations",
                                items = new { type = "string" }
                            }
                        },
                        required = new string[] { }
                    }
                )
            )
        };
    }

    public static string GetSystemPrompt()
    {
        return @"You are a helpful AI assistant with access to Spotify music tools. You can help users:

1. Create smart playlists based on their mood, activity, or preferences
2. Discover new music based on their listening history

When users ask about music, playlists, or want music recommendations, use the appropriate tools.
Always confirm what playlist was created and provide details about the tracks added.

Important:
- Before using tools, make sure the user has connected their Spotify account
- Provide friendly, conversational responses
- When creating playlists, ask clarifying questions if the user's request is vague
- Suggest specific moods, genres, or activities if the user needs ideas

Available tools:
- create_smart_playlist: Create a playlist based on natural language description
- discover_new_music: Find new tracks based on the user's listening history";
    }
}
