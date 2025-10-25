# Spotify Manager AI Agent

## Elevator Pitch
An intelligent Spotify management assistant that automates playlist creation, music discovery, and library organization. Users can interact with an AI agent that creates custom playlists based on natural language descriptions, suggests personalized music recommendations, discovers new tracks aligned with their taste, and cleans up duplicate songs across playlists—all while maintaining full visibility and control over every action the agent performs.

## Contributors
- Mateo Tomaszeuski

## Project Features

### Core Features
1. **AI-Powered Playlist Creation**
   - Natural language playlist generation (e.g., "create a workout playlist with high-energy rock songs")
   - Genre, mood, and tempo-based curation
   - Automatic playlist naming and description

2. **Personalized Music Suggestions**
   - Analyze user's existing playlists to understand music preferences
   - Provide contextual song recommendations
   - Suggest similar artists and tracks

3. **New Music Discovery**
   - Generate a "Discover Weekly" style playlist with 10 fresh recommendations
   - Based on user's saved songs and listening patterns
   - Filters out already-saved tracks to ensure novelty

4. **Duplicate Removal Tool**
   - Scan playlists for duplicate tracks (same song from different albums)
   - Present duplicates to user for review before removal
   - Batch cleanup with user confirmation

5. **Agent Status Dashboard**
   - Real-time visualization of agent's current task
   - Progress tracking for ongoing operations
   - Action history and logs

6. **User Control Panel**
   - Pause/resume agent operations
   - Approve or reject agent suggestions
   - Configure agent behavior and preferences

7. **Playlist Analytics**
   - Visualize playlist composition (genres, energy levels, etc.)
   - Track listening statistics
   - Compare playlists side-by-side

8. **Batch Operations**
   - Process multiple playlists simultaneously
   - Schedule recurring tasks (e.g., weekly new music discovery)
   - Export/import playlist configurations

## 4 Custom Agent Functions

1. **`createSmartPlaylist(prompt: string, preferences: object)`**
   - Processes natural language input to understand user intent
   - Queries Spotify API for matching tracks based on audio features
   - Generates and saves a new playlist to user's library
   - Returns playlist metadata and creation status

2. **`discoverNewMusic(limit: number, basePlaylists: string[])`**
   - Analyzes user's saved songs and specified playlists
   - Uses Spotify's recommendation API with seed tracks
   - Filters out already-saved songs to ensure fresh content
   - Creates a new playlist with recommended tracks

3. **`removeDuplicates(playlistId: string)`**
   - Scans target playlist for duplicate tracks (same ISRC or track name + artist)
   - Identifies which version of duplicate to keep (most popular, earliest release, etc.)
   - Presents findings to user for approval
   - Removes approved duplicates while maintaining playlist order

4. **`suggestMusicByContext(playlistId: string, context: string)`**
   - Analyzes specified playlist's audio features and themes
   - Generates contextual recommendations (e.g., "similar but more upbeat")
   - Returns curated list of suggested tracks with reasoning
   - Allows user to add selected suggestions to playlist

## Additional Tasks Targeting

- **Real-time Updates**: WebSocket connection for live agent status updates
- **Advanced API Integration**: Spotify Web API with OAuth 2.0 authentication
- **Data Visualization**: Interactive charts for playlist analytics
- **State Management**: Complex state handling for agent operations
- **Performance Optimization**: Lazy loading, virtualization for large playlists
- **Testing**: Unit tests for agent functions, integration tests for Spotify API

## New Technologies & Project Risks

### New Technologies to Learn
1. **Spotify Web API**
   - API endpoints for playlists, tracks, and recommendations
   - Audio features analysis
   - Rate limiting and pagination handling

2. **WebSockets**
   - Real-time agent status updates
   - Live progress tracking
   - Bi-directional communication for user control

3. **Audio Feature Analysis**
   - Understanding Spotify's audio feature metrics (danceability, energy, valence, etc.)
   - Implementing recommendation algorithms
   - Track similarity calculations

4. **Advanced State Management**
   - Managing complex agent state (idle, processing, awaiting approval, error)
   - Queue management for batch operations
   - Undo/redo functionality for agent actions

### Project Risks
1. **Spotify API Rate Limits**
   - Risk: Agent operations may be throttled during heavy usage
   - Mitigation: Implement request queuing and caching strategies

2. **OAuth Token Management**
   - Risk: Token expiration during long-running operations
   - Mitigation: Implement automatic token refresh with retry logic

3. **Duplicate Detection Accuracy**
   - Risk: False positives/negatives in duplicate identification
   - Mitigation: Multiple detection strategies (ISRC, name+artist fuzzy matching)

4. **Recommendation Quality**
   - Risk: AI suggestions may not align with user taste
   - Mitigation: Implement feedback loop and preference learning

5. **Real-time Communication Reliability**
   - Risk: WebSocket connection drops during operations
   - Mitigation: Fallback to polling, persistent operation state

6. **Complex User Permissions**
   - Risk: Users may not grant all required Spotify scopes
   - Mitigation: Graceful degradation, clear permission explanations

## 10 Pages/Views

1. **Landing Page**
   - Project overview and value proposition
   - Authentication button (Connect with Spotify)
   - Feature highlights and demo screenshots

2. **Dashboard (Home)**
   - Agent status overview
   - Quick action buttons
   - Recent activity feed
   - Key metrics (playlists managed, duplicates removed, etc.)

3. **Playlist Creator**
   - Natural language input form
   - Genre/mood/tempo selectors
   - Preview of potential tracks
   - Create/save playlist interface

4. **New Music Discovery**
   - Configuration panel (number of songs, source playlists)
   - Generated recommendations list with preview players
   - Save as new playlist or add to existing
   - Refresh recommendations button

5. **Duplicate Cleaner**
   - Playlist selector dropdown
   - Scan results table with duplicate groups
   - Selection interface for which version to keep
   - Bulk approve/reject controls

6. **Music Suggestion Engine**
   - Playlist/context selection
   - AI-generated suggestions with explanations
   - Add to playlist or save for later
   - Feedback mechanism (like/dislike)

7. **Agent Control Center**
   - Live agent status with progress indicators
   - Operation queue visualization
   - Pause/resume/cancel controls
   - Detailed action logs

8. **Playlist Analytics**
   - Audio feature visualizations (radar charts, histograms)
   - Genre distribution pie charts
   - Playlist comparison tools
   - Export analytics reports

9. **Settings & Preferences**
   - Agent behavior configuration
   - Default preferences for operations
   - Spotify account connection status
   - Privacy and data management

10. **Activity History**
    - Chronological log of all agent actions
    - Undo/revert capabilities for recent actions
    - Filter by operation type
    - Export history as JSON/CSV


