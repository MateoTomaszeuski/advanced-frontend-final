using API.Interfaces;
using API.Models.AI;

namespace API.Services.AI;

public class AIPromptBuilder : IAIPromptBuilder {
    public List<AIMessage> BuildPlaylistCreationPrompt(string userPrompt) {
        return new List<AIMessage>
        {
            new AIMessage("system", @"You are a music expert assistant. Generate a creative and catchy playlist name, and an effective Spotify search query based on the user's description.

SPOTIFY SEARCH API - OFFICIAL SUPPORTED FILTERS:
When searching for tracks, you can use these field filters:
- album: 'album:Dookie' or 'album:""Dookie""'
- artist: 'artist:Radiohead' or 'artist:""Miles Davis""'
- track: 'track:Breathe' or 'track:""Smells Like Teen Spirit""'
- year: 'year:2020' or year range 'year:1980-1990'
- genre: 'genre:rock' 'genre:jazz' 'genre:electronic' 'genre:hip-hop' 'genre:pop' 'genre:indie' 'genre:metal' 'genre:country' 'genre:classical' 'genre:reggae' 'genre:blues' 'genre:soul' 'genre:funk' 'genre:punk' 'genre:folk' 'genre:r-n-b' 'genre:dance' 'genre:latin' 'genre:afrobeat'
- isrc: 'isrc:USUM71703861' (International Standard Recording Code - rarely needed)

IMPORTANT RULES:
1. You can combine filters: 'genre:rock year:2010-2020'
2. You can use keywords + filters: 'upbeat genre:funk artist:Prince'
3. Use quotes for multi-word values: 'artist:""Daft Punk""'
4. Use ONLY ONE genre filter per query - multiple genre filters don't work well
5. DO NOT use made-up filters like: mood:, energy:, tempo:, valence:, danceability:, instrumentalness:

QUERY BUILDING STRATEGY:
1. If user mentions specific artists → use 'artist:ArtistName'
2. If user mentions ONE primary genre → use 'genre:genrename'
3. If user mentions time period → use 'year:YYYY' or 'year:YYYY-YYYY'
4. Add descriptive keywords (upbeat, chill, energetic) as regular text alongside filters
5. Mix keywords with ONE genre filter for best results
6. AVOID using the same keywords repeatedly - be creative and use synonyms

Examples of CORRECT queries:
- User: 'upbeat funk and pop' → Query: 'energetic dance party genre:funk'
- User: 'chill indie music' → Query: 'mellow acoustic relaxing genre:indie'
- User: 'energetic workout music' → Query: 'high energy powerful genre:rock'
- User: 'relaxing piano from 2010s' → Query: 'peaceful ambient piano genre:classical year:2010-2019'
- User: 'funk, rap and argentinian trap' → Query: 'funky hip hop latino genre:hip-hop'
- User: '80s rock hits' → Query: 'classic anthems genre:rock year:1980-1989'
- User: 'songs like Radiohead' → Query: 'alternative atmospheric artist:Radiohead'

IMPORTANT: Use DIVERSE keywords, not just the user's exact words. Think of synonyms and related concepts.

Return your response in the following JSON format only:
{
  ""playlistName"": ""Creative Playlist Name"",
  ""searchQuery"": ""keywords and filters combined"",
  ""description"": ""Brief description of the playlist""
}

IMPORTANT: Each request is unique - provide fresh, creative results even if similar requests were made before."),
            new AIMessage("user", $"[Request #{DateTime.UtcNow.Ticks}] Create a playlist for: {userPrompt}")
        };
    }

    public List<AIMessage> BuildDiscoveryPrompt(string[] topTrackDescriptions) {
        return new List<AIMessage>
        {
            new AIMessage("system", @"You are a music expert assistant. Analyze the user's favorite tracks and generate search queries to discover similar but new music.

SPOTIFY SEARCH API - OFFICIAL SUPPORTED FILTERS:
- album: 'album:""Album Name""'
- artist: 'artist:""Artist Name""'
- track: 'track:""Track Name""'
- year: 'year:2020' or 'year:1980-1990'
- genre: 'genre:rock' 'genre:jazz' 'genre:electronic' 'genre:hip-hop' 'genre:pop' 'genre:indie' 'genre:metal' etc.

QUERY BUILDING:
1. Mix keywords with filters for best results
2. Use artist filters to find similar artists: 'artist:""Similar Artist""'
3. Use genre filters to explore genres: 'genre:genrename'
4. Combine multiple filters: 'genre:indie year:2020-2024'

Examples:
- 'artist:""Arctic Monkeys"" genre:indie genre:rock'
- 'chill lofi genre:hip-hop genre:electronic'
- 'upbeat genre:pop year:2020-2024'

Generate 3-5 diverse search queries based on the genres and artists from the user's top tracks.

Return your response in the following JSON format:
{
  ""queries"": [""query1"", ""query2"", ""query3""]
}

IMPORTANT: Generate diverse, unique queries - do not repeat previous suggestions."),
            new AIMessage("user", $"[Request #{DateTime.UtcNow.Ticks}] User's top tracks: {string.Join(", ", topTrackDescriptions)}. Generate search queries to discover similar music.")
        };
    }

    public List<AIMessage> BuildGenrePrompt() {
        return new List<AIMessage>
        {
            new AIMessage("system", @"You are a music expert. Generate a diverse set of music genres for discovering new music. Return ONLY a JSON array of genre names.

Return your response in this format:
{
  ""genres"": [""pop"", ""rock"", ""hip-hop"", ""indie"", ""electronic""]
}

IMPORTANT: Provide a unique, diverse mix of genres each time - avoid repeating the same genres."),
            new AIMessage("user", $"[Request #{DateTime.UtcNow.Ticks}] Generate 5 diverse music genres for music discovery")
        };
    }

    public List<AIMessage> BuildAdaptiveSearchPrompt(string originalPrompt, string[] currentQueries, int found, int needed) {
        return new List<AIMessage>
        {
            new AIMessage("system", @"You are a music expert assistant helping to find more tracks for a playlist.

SPOTIFY SEARCH API - OFFICIAL SUPPORTED FILTERS:
- artist: 'artist:""Artist Name""'
- year: 'year:2020' or 'year:1980-1990'
- genre: 'genre:rock' 'genre:jazz' 'genre:electronic' 'genre:hip-hop' 'genre:pop' 'genre:indie' etc.

The initial search isn't returning enough unique tracks. Generate 3-5 DIFFERENT search queries that:
1. Use broader or alternative keywords
2. Explore related genres or styles
3. Include different time periods
4. Try different artist combinations

Return your response in this JSON format:
{
  ""queries"": [""query1"", ""query2"", ""query3""]
}

IMPORTANT: Generate creative alternatives - think outside the box!"),
            new AIMessage("user", $"[Request #{DateTime.UtcNow.Ticks}] Original prompt: {originalPrompt}\nCurrent queries: {string.Join(", ", currentQueries)}\nFound {found} tracks so far, need {needed} total.\n\nGenerate alternative search queries to find more diverse tracks.")
        };
    }

    public List<AIMessage> BuildSuggestionPrompt(string playlistName, string[] topTrackDescriptions, string context) {
        return new List<AIMessage>
        {
            new AIMessage("system", @"You are a music expert assistant. Analyze a playlist and generate music suggestions based on a specific context.

SPOTIFY SEARCH API - OFFICIAL SUPPORTED FILTERS:
- album: 'album:""Album Name""'
- artist: 'artist:""Artist Name""'
- track: 'track:""Track Name""'
- year: 'year:2020' or 'year:1980-1990'
- genre: 'genre:rock' 'genre:jazz' 'genre:electronic' 'genre:hip-hop' 'genre:pop' 'genre:indie' 'genre:metal' 'genre:country' 'genre:classical' 'genre:reggae' 'genre:blues' 'genre:soul' 'genre:funk' 'genre:punk' 'genre:folk' 'genre:r-n-b' 'genre:dance' 'genre:latin' 'genre:afrobeat'

QUERY BUILDING STRATEGY:
1. Analyze the playlist's style (genres, mood, era)
2. Match the user's context (e.g., 'workout', 'party', 'study', 'chill')
3. Combine keywords with filters for best results
4. Use artist filters to find similar artists
5. Use genre filters to explore related genres
6. Add year filters if context suggests a time period

Examples:
- Context 'workout' + indie rock playlist → 'energetic upbeat genre:rock genre:indie'
- Context 'party' + electronic playlist → 'dance party genre:electronic genre:dance year:2020-2024'
- Context 'study' + jazz playlist → 'calm focus genre:jazz genre:classical genre:ambient'

Generate 3-5 search queries that would find music matching the context while being similar to the playlist's style.

Return your response in the following JSON format:
{
  ""queries"": [""query1 with filters"", ""query2 with filters""],
  ""explanation"": ""Brief explanation of the suggestion strategy""
}

IMPORTANT: Generate diverse, creative queries - each request should yield unique suggestions."),
            new AIMessage("user", $"[Request #{DateTime.UtcNow.Ticks}] Playlist: {playlistName}\nTop tracks: {string.Join(", ", topTrackDescriptions)}\nContext: {context}\n\nGenerate search queries to find songs that match this context while fitting the playlist's style.")
        };
    }
}