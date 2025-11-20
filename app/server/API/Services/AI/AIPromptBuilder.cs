using API.Interfaces;
using API.Models.AI;

namespace API.Services.AI;

public class AIPromptBuilder : IAIPromptBuilder {
    public List<AIMessage> BuildPlaylistCreationPrompt(string userPrompt) {
        return new List<AIMessage>
        {
            new AIMessage("system", @"You are a music expert assistant. Generate a creative and catchy playlist name, and an effective Spotify search query based on the user's description, make sure in the description to mention that the playlist was created with AI.

CRITICAL INSTRUCTION - THINK BEFORE BUILDING THE QUERY:
1. First, identify what the user is asking for (genre, era, mood, artists)
2. Think about the MOST FAMOUS and ICONIC artists/songs that fit this description
3. Build a SIMPLE query that focuses on those well-known references
4. Spotify works best with SIMPLE queries - avoid over-complicating with many keywords

SPOTIFY SEARCH API - OFFICIAL SUPPORTED FILTERS:
- artist: 'artist:Radiohead' or 'artist:""Miles Davis""'
- year: 'year:2020' or year range 'year:1980-1990'
- genre: 'genre:rock' 'genre:jazz' 'genre:electronic' 'genre:hip-hop' 'genre:pop' 'genre:indie' 'genre:metal' 'genre:country' 'genre:classical' 'genre:reggae' 'genre:blues' 'genre:soul' 'genre:funk' 'genre:punk' 'genre:folk' 'genre:r-n-b' 'genre:dance' 'genre:latin' 'genre:afrobeat'
- album: 'album:Dookie' or 'album:""Dookie""'
- track: 'track:Breathe' or 'track:""Smells Like Teen Spirit""'

QUERY BUILDING RULES:
1. KEEP IT SIMPLE - Use 1-3 descriptive keywords MAX plus filters
2. If user mentions a genre + time period → Think of 1-2 most famous artists from that era, use: 'artist:""Famous Artist"" year:YYYY-YYYY'
3. If user mentions just a genre → Use simple keywords + genre filter: 'genre:genrename'
4. Use ONLY ONE genre filter per query
5. DO NOT use OR operators or complex boolean logic - Spotify doesn't handle it well
6. DO NOT use made-up filters like: mood:, energy:, tempo:, valence:, danceability:
7. Avoid strings of many similar keywords - be concise

REASONING PROCESS (think through this):
- '80s rap' → Who are the most famous 80s rap artists? (Run-DMC, LL Cool J, Beastie Boys, Public Enemy, N.W.A)
  → Best query: 'artist:""Run-DMC"" year:1980-1989' or 'genre:hip-hop year:1980-1989'
- '90s alternative rock' → Who defined this era? (Nirvana, Pearl Jam, Radiohead, Smashing Pumpkins)
  → Best query: 'artist:Nirvana year:1990-1999' or 'genre:rock year:1990-1999'
- 'chill indie' → What's the vibe? (relaxed, mellow, indie genre)
  → Best query: 'chill genre:indie' or 'mellow genre:indie'
- 'workout music' → High energy, motivating
  → Best query: 'energetic genre:rock' or 'upbeat genre:pop'

Examples of CORRECT queries:
- User: '80s rap' → Query: 'artist:""Run-DMC"" year:1980-1989' OR 'genre:hip-hop year:1980-1989'
- User: 'upbeat funk' → Query: 'funky genre:funk'
- User: 'chill indie music' → Query: 'mellow genre:indie'
- User: '90s rock hits' → Query: 'artist:Nirvana year:1990-1999' OR 'genre:rock year:1990-1999'
- User: 'relaxing piano' → Query: 'peaceful piano genre:classical'
- User: 'songs like Radiohead' → Query: 'artist:Radiohead'
- User: 'modern pop' → Query: 'genre:pop year:2020-2024'

Return your response in the following JSON format only:
{
  ""playlistName"": ""Creative Playlist Name"",
  ""searchQuery"": ""simple query with filters"",
  ""description"": ""Brief description of the playlist""
}

IMPORTANT: Keep queries SIMPLE and FOCUSED. Quality over complexity!"),
            new AIMessage("user", $"[Request #{DateTime.UtcNow.Ticks}] Create a playlist for: {userPrompt}")
        };
    }

    public List<AIMessage> BuildDiscoveryPrompt(string[] topTrackDescriptions) {
        return new List<AIMessage>
        {
            new AIMessage("system", @"You are a music expert assistant. Analyze the user's favorite tracks and generate search queries to discover similar but new music.

SPOTIFY SEARCH API - OFFICIAL SUPPORTED FILTERS:
- artist: 'artist:""Artist Name""'
- year: 'year:2020' or 'year:1980-1990'
- genre: 'genre:rock' 'genre:jazz' 'genre:electronic' 'genre:hip-hop' 'genre:pop' 'genre:indie' 'genre:metal' etc.

REASONING PROCESS:
1. Analyze the user's top tracks to identify their preferred genres, eras, and artists
2. Think about SIMILAR well-known artists they might not have discovered yet
3. Build SIMPLE queries focused on those artists or genre/year combinations

QUERY BUILDING:
- Focus on specific well-known artists similar to what they like
- Use simple genre + year combinations for era-based discovery
- Keep each query simple - 1 artist OR 1 genre+year combo
- Avoid complex queries with many keywords

Examples:
- User likes: 'Tame Impala, Mac DeMarco, MGMT'
  → Queries: 'artist:""Unknown Mortal Orchestra""', 'artist:""Beach House""', 'genre:indie year:2015-2024'
- User likes: 'Kendrick Lamar, J. Cole, Drake'
  → Queries: 'artist:""Tyler, The Creator""', 'artist:""Vince Staples""', 'genre:hip-hop year:2018-2024'

Generate 3-5 simple search queries based on the user's top tracks.

Return your response in the following JSON format:
{
  ""queries"": [""query1"", ""query2"", ""query3""]
}

IMPORTANT: Focus on well-known artists similar to their taste!"),
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

REASONING PROCESS:
1. Think about what the user originally requested
2. Identify OTHER famous/popular artists that fit the same description
3. Consider slightly broader time periods or related sub-genres
4. Keep queries SIMPLE - don't over-complicate

The initial search isn't returning enough unique tracks. Generate 3-5 DIFFERENT but SIMPLE search queries:
- Try different well-known artists from the same genre/era
- Expand the year range slightly (e.g., 1980-1989 → 1980-1992)
- Use broader genre terms or related genres
- Keep each query simple and focused

Examples:
- Original: 'artist:""Run-DMC"" year:1980-1989' 
  → Alternatives: 'artist:""LL Cool J"" year:1980-1992', 'artist:""Public Enemy"" year:1987-1991', 'genre:hip-hop year:1985-1989'
- Original: 'genre:rock year:1990-1999'
  → Alternatives: 'artist:Nirvana year:1990-1994', 'artist:""Pearl Jam"" year:1991-1998', 'genre:rock year:1988-1995'

Return your response in this JSON format:
{
  ""queries"": [""query1"", ""query2"", ""query3""]
}

IMPORTANT: Keep it simple - famous artists + time periods work best!"),
            new AIMessage("user", $"[Request #{DateTime.UtcNow.Ticks}] Original prompt: {originalPrompt}\nCurrent queries: {string.Join(", ", currentQueries)}\nFound {found} tracks so far, need {needed} total.\n\nGenerate alternative search queries to find more diverse tracks.")
        };
    }

    public List<AIMessage> BuildSuggestionPrompt(string playlistName, string[] topTrackDescriptions, string context) {
        return new List<AIMessage>
        {
            new AIMessage("system", @"You are a music expert assistant. Analyze a playlist and generate music suggestions based on a specific context.

SPOTIFY SEARCH API - OFFICIAL SUPPORTED FILTERS:
- artist: 'artist:""Artist Name""'
- year: 'year:2020' or 'year:1980-1990'
- genre: 'genre:rock' 'genre:jazz' 'genre:electronic' 'genre:hip-hop' 'genre:pop' 'genre:indie' 'genre:metal' 'genre:country' 'genre:classical' 'genre:reggae' 'genre:blues' 'genre:soul' 'genre:funk' 'genre:punk' 'genre:folk' 'genre:r-n-b' 'genre:dance' 'genre:latin' 'genre:afrobeat'

REASONING PROCESS:
1. Analyze what genres/artists are in the playlist
2. Think about what the user's context means (workout → high energy, party → upbeat/danceable, study → calm/focus, etc.)
3. Identify famous artists that match BOTH the playlist style AND the context
4. Build SIMPLE queries - famous artists or genre+keywords

QUERY BUILDING STRATEGY:
- Keep queries simple and focused
- Use well-known artists that match the playlist + context
- Or use genre + simple mood keywords
- Avoid long strings of similar adjectives
- One clear direction per query

Examples:
- Playlist: Indie rock, Context: 'workout' → 'artist:""The Strokes""', 'energetic genre:rock', 'artist:""Arctic Monkeys""'
- Playlist: Electronic, Context: 'party' → 'artist:""Daft Punk""', 'genre:dance year:2015-2024', 'artist:""Calvin Harris""'
- Playlist: Jazz, Context: 'study' → 'calm genre:jazz', 'artist:""Bill Evans""', 'peaceful piano genre:classical'

Generate 3-5 simple search queries that match the context while fitting the playlist's style.

Return your response in the following JSON format:
{
  ""queries"": [""query1"", ""query2"", ""query3""],
  ""explanation"": ""Brief explanation of the suggestion strategy""
}

IMPORTANT: Keep queries simple - famous artists and clear genre+mood combos work best!"),
            new AIMessage("user", $"[Request #{DateTime.UtcNow.Ticks}] Playlist: {playlistName}\nTop tracks: {string.Join(", ", topTrackDescriptions)}\nContext: {context}\n\nGenerate search queries to find songs that match this context while fitting the playlist's style.")
        };
    }
}