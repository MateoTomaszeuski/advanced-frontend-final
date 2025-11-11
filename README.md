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

---

## Project Outline & Rubric Checklist

### Agent Requirements

[ ] 4 custom functions (actions) that can be called

[ ] `createSmartPlaylist()` - autonomous playlist creation

[ ] `discoverNewMusic()` - autonomous new music discovery

[ ] `removeDuplicates()` - requires user confirmation

[ ] `suggestMusicByContext()` - requires user confirmation

[ ] 1+ action(s) can be performed autonomously (createSmartPlaylist, discoverNewMusic)

[ ] 1+ action(s) require user confirmation to perform (removeDuplicates, suggestMusicByContext)

[ ] 1+ action(s) automatically adjust the UI when performed (navigate to new playlist, update dashboard)

[ ] Structured output, validated with Zod

[ ] Agentic loop runs until task is complete or user intervention required

[ ] LLM decisions and actions are persisted and can be inspected by users

### Additional Tasks (Choose 1+)

[x] Real-time WebSocket communication for agent status updates (planned)

[ ] Streaming generation in the UI for agent responses

[ ] User configurable benchmarking tool for agent performance

### Technical Requirements

[x] Deployed in production (public internet or class Kubernetes cluster)

[x] CI/CD pipeline configured

[x] Unit tests run automatically in pipeline

[x] Linter runs automatically in pipeline

[ ] Data persisted on server for multi-client access

### Technology Requirements

[x] Global client-side state management (Zustand)

[x] Toasts / global notifications for agent actions

[x] Error handling (API requests and render errors)

[ ] Network calls - read data (GET playlists, tracks)

[ ] Network calls - write data (POST/PUT/DELETE playlists)

[x] Developer type helping (TypeScript)

[x] 10+ pages/views with router

[x] CI/CD pipeline

[x] Live production environment

[x] Automated testing and linting in pipeline (abort on fail)

[x] 3+ reusable form input components

[x] 2+ reusable layout components

[x] Authentication and user account support (Keycloak OAuth)

[x] Authorized pages and public pages

---

## Project Schedule

### Oct 29 (Week 1 - Foundation)

#### Estimates:

**Rubric Items:**
- CI/CD pipeline configured with GitHub Actions
- Linter runs automatically in pipeline
- Automated testing setup (abort build if fails)
- Live production environment (Kubernetes)
- Authentication and user account support (Keycloak)
- Authorized pages and public pages

**Features:**
- Project scaffolding with Vite + React + TypeScript
- ESLint configuration
- GitHub Actions workflow for CI/CD
- Keycloak authentication flow
- Landing page (public)
- Basic dashboard layout (authorized)
- Protected route wrapper component

#### Delivered:

**Rubric Items:**
- CI/CD pipeline configured with GitHub Actions
- Linter runs automatically in pipeline (pnpm lint for frontend)
- Automated testing setup (dotnet test for backend, aborts on fail)
- Live production environment (Kubernetes cluster ready with all deployment configs)
- Developer type helping (TypeScript throughout)

**Features:**
- Project scaffolding with Vite + React + TypeScript (app/client)
- C# ASP.NET Core Web API with controllers (app/server/API)
- Unit test project with TUnit (app/server/API.UnitTests)
- Integration test project with TUnit (app/server/API.IntegrationTests)
- Solution file with all projects referenced (app/server/Server.sln)
- ESLint configuration with TypeScript support
- GitHub Actions workflow with lint and test jobs
- Dockerfiles for frontend (Nginx-based) and backend (.NET 9)
- Kubernetes deployments, services, and ingress configs
- PostgreSQL database setup in Kubernetes

---

### Nov 1 (Week 2 - Core Infrastructure)

#### Estimates:

**Rubric Items:**
- Global client-side state management
- Developer type helping (TypeScript throughout)
- 2+ reusable layout components (MainLayout, AuthLayout)
- 3+ reusable form input components (TextInput, Select, Button)
- Error handling for render errors (Error Boundary)
- Data persisted on server setup (backend API initialization)

**Features:**
- State management store structure
- Reusable layout components (MainLayout, AuthLayout, Sidebar)
- Reusable form components (TextInput, SelectDropdown, Button, SearchInput)
- Error boundary component
- Settings page with Spotify connection (authorized)
- Backend API scaffolding
- Database schema design for user data and agent logs
- Spotify token storage and management

#### Delivered:

**Rubric Items:**
- Global client-side state management (Zustand with 3 stores: user, agent, UI)
- Developer type helping (TypeScript with strict types throughout)
- 2+ reusable layout components (MainLayout, AuthLayout, Sidebar, Header - 4 total)
- 3+ reusable form input components (TextInput, SelectDropdown, Button, SearchInput, Checkbox - 5 total)
- Error handling for render errors (ErrorBoundary with fallback UI)
- Authentication and user account support (Keycloak OAuth with react-oidc-context)
- Authorized pages and public pages (ProtectedRoute wrapper)
- 10+ pages/views with router (Landing, Dashboard, Settings, + 7 feature pages)
- Toasts / global notifications (react-hot-toast configured)

**Features:**
- Zustand stores: useUserStore, useAgentStore, useUIStore with localStorage persistence
- Layout components: MainLayout (sidebar+header+scrollable content), AuthLayout (centered card), Sidebar (green gradient navigation), Header (user dropdown menu)
- Form components: TextInput, SelectDropdown, Button (4 variants: primary, secondary, danger, ghost), SearchInput, Checkbox - all TypeScript typed with error states
- ErrorBoundary class component with fallback UI and error details
- Landing page with Spotify branding, gradient background, and Keycloak OAuth sign-in
- Dashboard page with agent status cards, metrics display, and recent activity section
- Settings page with Keycloak account info, Spotify account info, connection toggle, agent preferences, and danger zone
- 7 additional feature pages (Playlist Creator, Discover, Duplicate Cleaner, Suggestions, Agent Control, Analytics, History)
- React Router v7 with BrowserRouter and protected route wrapper
- Toast notification utility with react-hot-toast integration
- Welcome toast on user login with personalized greeting
- Environment variables for Keycloak configuration (.env.development and .env.production)
- TypeScript strict mode with proper type imports and interfaces
- Tailwind CSS v4 with custom index.css (minimal, clean setup)
- Responsive design with proper scrolling behavior in MainLayout
- Dark green sidebar theme (green-900 to green-950 gradient) matching Spotify aesthetic
- Button components with loading states, icon support, and green color scheme
- Proper overflow handling for main content area with h-screen and overflow-y-auto
- Toggle switches in settings with green accent colors
- Keycloak CORS configuration resolved (Web Origins set to allow frontend origin)
- Fixed CORS policy blocking on token endpoint (https://auth-dev.snowse.io)
- Keycloak client configuration validated (Web Origins includes https://mateo-spotify.duckdns.org)
- TLS/SSL certificates configured for HTTPS on production domain (https://mateo-spotify.duckdns.org)
- Secure HTTPS communication enabled for all API and authentication requests
- Kubernetes TLS secret configured with SSL certificates for ingress


---

### Nov 5 (Week 3 - Agent Core Functions Part 1)

#### Estimates:

**Rubric Items:**
- 2 custom functions implemented (`createSmartPlaylist`, `discoverNewMusic`)
- 1+ action(s) can be performed autonomously
- Structured output validated with Zod
- Network calls - read data (GET Spotify playlists, tracks, recommendations)
- Network calls - write data (POST create playlist)

**Features:**
- Playlist Creator page with natural language input
- `createSmartPlaylist()` agent function implementation
- `discoverNewMusic()` agent function implementation
- New Music Discovery page
- Spotify API integration service
- Zod schemas for API responses and agent outputs
- Agent action logging to database

#### Delivered:

**Rubric Items:**
- 2 custom functions implemented (`createSmartPlaylist`, `discoverNewMusic`)
- 1+ action(s) can be performed autonomously (both functions run autonomously)
- Structured output validated with Zod (frontend validation schemas)
- Network calls - read data (GET Spotify tracks, recommendations, audio features)
- Network calls - write data (POST create playlist, add tracks to playlist)
- Unit tests run automatically in pipeline (backend service and repository tests)
- Data persisted on server (PostgreSQL with Dapper ORM)

**Features:**
- **Backend Architecture Switch**: Migrated from EF Core to Dapper for better performance and control
- **Database Schema**: PostgreSQL with init.sql and seed.sql for Docker integration
  - Users table with Spotify token management
  - Conversations table for user context
  - Agent actions table with JSONB for flexible data storage
  - Database README with schema documentation
- **JWT Authentication Middleware**: UserContextMiddleware extracts user from Keycloak tokens
- **User Service**: User creation, retrieval, Spotify token management with authorization checks
- **AI Integration**:
  - AIService - Full integration with AI API (gpt-oss-120b model)
  - AITools - Spotify tool definitions for AI function calling
  - AI-powered playlist name generation from user prompts
  - AI-powered search query generation with genre extraction
  - Fallback to simple string matching if AI fails
  - Intelligent translation of descriptive words to music genres (upbeat → funk/pop/dance)
  - Token counting and summarization support for long conversations
- **Spotify API Service**: Complete integration with OAuth token management
  - SearchTracksAsync - Search Spotify catalog with logging
  - GetRecommendationsAsync - AI-powered recommendations
  - GetAudioFeaturesAsync - Track analysis (energy, tempo, valence)
  - CreatePlaylistAsync - Playlist creation
  - AddTracksToPlaylistAsync - Track management
  - GetCurrentUserIdAsync - User ID retrieval
  - GetCurrentUserProfileAsync - Full profile with image support
- **Agent Service**: Core AI functions with intelligent algorithms
  - `CreateSmartPlaylist()` - AI-powered prompt parsing, multi-query fallback system, audio feature filtering, intelligent playlist naming, track deduplication, ensures requested track count (up to 250 tracks)
  - `DiscoverNewMusic()` - AI-powered music discovery with intelligent search query generation, multi-search fallback, track deduplication, ensures requested track count (up to 250 tracks), filters already-saved songs
- **API Controllers**:
  - ConversationsController - CRUD operations with user isolation, action count aggregation
  - AgentController - Agent function endpoints with conversation tracking and user authorization
  - SpotifyController - OAuth code exchange, token management, connection status, profile retrieval, disconnect
  - ChatController - AI chat endpoint with tool calling support
  - TestController - Authentication verification endpoints
- **Dapper Repositories**:
  - UserRepository - User CRUD with async/await, column mapping
  - ConversationRepository - Conversation management with action count aggregation
  - AgentActionRepository - Agent action logging with JSONB serialization/deserialization
- **Frontend API Integration**:
  - apiClient.ts - Centralized HTTP client with JWT token injection from sessionStorage, error handling for 401/403
  - services/api.ts - Type-safe API function wrappers for all endpoints (conversation, agent, spotify, test)
  - schemas/api.ts - 13 Zod schemas for runtime validation (User, Conversation, AgentAction, Spotify models, PlaylistPreferences)
  - types/api.ts - TypeScript types exported from Zod schemas
  - hooks/useAgent.ts - React hook for agent operations with state management, progress tracking, toast notifications
- **Spotify Connection Alert Component**:
  - Checks connection status on mount
  - Displays warning if Spotify not connected
  - Direct link to connect with OAuth authorization code flow
  - Yellow alert styling with icon
- **Playlist Creator Page UI** (PlaylistCreatorPage.tsx):
  - Natural language prompt input with examples
  - Number of tracks selector (10, 20, 30, 50, 75, 100, 150, 200, 250)
  - Advanced options toggle with energy/tempo filters
  - Agent status display with real-time updates and spinner
  - Integration with createSmartPlaylist endpoint
  - Conversation initialization and tracking
  - Spotify connection check with alert
  - Clear button to reset form
  - Tips section with usage guidance
- **New Music Discovery Page UI** (DiscoverPage.tsx):
  - Discovery settings panel (limit selection 5-250 tracks)
  - Results display with track list, artist names, and Spotify links
  - Latest discovery section showing created playlist details
  - Integration with discoverNewMusic endpoint
  - Spotify connection check with alert
  - "How it works" explanation panel
  - AI-powered search query generation
- **Individual Page Components**: Refactored from single OtherPages.tsx to separate files
  - PlaylistCreatorPage.tsx - Playlist creation with AI
  - DiscoverPage.tsx - Music discovery
  - DuplicateCleanerPage.tsx - Duplicate removal (placeholder)
  - SuggestionsPage.tsx - Music suggestions (placeholder)
  - AgentControlPage.tsx - Agent control center (placeholder)
  - AnalyticsPage.tsx - Playlist analytics (placeholder)
  - HistoryPage.tsx - Activity history (placeholder)
- **Dashboard Page Enhancements** (DashboardPage.tsx):
  - Agent status cards with real-time updates
  - Total actions and completed actions metrics
  - Recent activity feed showing last 5 actions (fixed to show newest first)
  - Current task display when agent is processing
  - Status color coding (idle, processing, error states)
  - Conversation tracking with current conversation state
  - Agent action history with recent actions
  - Real-time status updates (idle, processing, error)
  - Current task description
  - Progress tracking (0-100)
  - localStorage persistence for conversations list
- **Settings Page Enhancements**:
  - Spotify profile display with image, display name, email, country
  - Token expiry display
  - OAuth authorization code flow with code exchange
  - Connection status checking
  - Disconnect functionality
  - Environment variable configuration for Spotify client ID/redirect URI
- **Comprehensive Backend Testing**:
  - **Unit Tests** (TUnit framework):
    - UserServiceTests - 4 tests (GetOrCreate, GetByEmail, UpdateUser with timestamp)
    - AgentServiceTests - 3 tests (CreateSmartPlaylist success/failure scenarios, DiscoverNewMusic)
    - AIServiceTests - 3 tests (token counting, summarization threshold)
    - UserRepositoryTests - 5 tests with PostgreSQL testcontainer (CRUD operations)
    - ConversationRepositoryTests - 4 tests with testcontainer (create, get, update, delete)
  - **Integration Tests** (Docker Compose based):
    - ConversationsControllerTests - Authentication and CRUD endpoint tests
    - AgentControllerTests - Authorization verification tests (createSmartPlaylist, discoverNewMusic without auth)
    - docker-compose.integration-tests.yml - External test database on port 5433
    - run-integration-tests.sh - Automated test infrastructure management with cleanup
    - Separate test database instance to avoid conflicts with development
  - **Testing Infrastructure**:
    - Moq for service mocking
    - FluentAssertions for readable test assertions
    - WebApplicationFactory for integration testing
    - Docker Compose for test database isolation
    - Health checks for database readiness
    - Testcontainers for PostgreSQL in unit tests
- **Helper Classes**:
  - PlaylistHelper - Fallback search query generation and playlist naming
  - SpotifyJsonParser - JSON parsing for Spotify API responses
  - TrackFilterHelper - Audio feature-based track filtering
  - ToolExecutionHelper - AI tool execution for chat endpoint
- **DTOs and Models**:
  - Agent DTOs (CreateSmartPlaylistRequest, PlaylistPreferences, DiscoverNewMusicRequest, AgentActionResponse)
  - Chat DTOs (ChatRequestDto, ChatMessageDto)
  - Spotify DTOs (SpotifyTrack, SpotifyArtist, SpotifyAlbum, AudioFeatures, SpotifyPlaylist, SpotifyUserProfile)
  - AI Models (AIMessage, AIResponse, AITool, AIToolFunction, AIToolCall, ExecutedToolCall)
  - Core Models (User, Conversation, AgentAction)
- **Program.cs Configuration**:
  - Dependency injection for all services and repositories
  - JWT Bearer authentication with Keycloak
  - CORS configuration with environment-based allowed origins
  - Middleware pipeline (CORS → Authentication → Authorization → UserContext → Controllers)
  - HttpClient factory for AI and Spotify services
  - OpenAPI/Swagger in development
- **Environment Configuration**:
  - .env.example with all required configuration
  - Keycloak configuration (Authority, MetadataAddress)
  - CORS allowed origins
  - AI API configuration (BaseUrl, Model, ApiKey)
  - Spotify API configuration (ClientId, ClientSecret)
- **Error Handling**:
  - Try-catch blocks in all service methods
  - Validation in controllers
  - NotFound responses for missing resources
  - Forbidden responses for unauthorized access
  - Proper HTTP status codes throughout
  - Error logging with ILogger
- **Authorization & Security**:
  - User-scoped data access in all endpoints
  - JWT token verification via [Authorize] attribute
  - User context injection via middleware
  - Conversation ownership validation
  - Agent action ownership validation via conversation
  - Spotify token expiry checking
- **Advanced Playlist Features**:
  - Multi-query fallback system if initial search returns insufficient tracks
  - Genre word extraction from search queries for better fallback
  - Track deduplication using HashSet
  - Ensures requested track count is met (or as close as possible)
  - Logs each step of the process for debugging
  - AI-generated playlist metadata with fallback to simple logic
  - Support for large playlists (up to 250 tracks)
- **Advanced Music Discovery Features**:
  - AI analyzes user's top saved tracks to generate intelligent search queries
  - Multi-search strategy with different query variations
  - Deduplication across all search results using HashSet
  - Filters out already-saved tracks to ensure novelty
  - Fallback to Spotify recommendations API if searches insufficient
  - Handles users with no saved tracks using AI-generated genre diversity
  - Multiple batches of recommendations to meet requested track count
  - Comprehensive logging for debugging and optimization


---

### Nov 8 (Week 4 - Agent Core Functions Part 2)

#### Estimates:

**Rubric Items:**
- 2 additional custom functions (`removeDuplicates`, `suggestMusicByContext`)
- 1+ action(s) require user confirmation to perform
- 1+ action(s) automatically adjust UI when performed
- Toasts / global notifications for agent actions
- Error handling for API requests

**Features:**
- Duplicate Cleaner page with confirmation UI
- `removeDuplicates()` agent function with user approval flow
- Music Suggestion Engine page
- `suggestMusicByContext()` agent function
- Toast notification system (react-hot-toast or similar)
- API error handling with retry logic
- Auto-navigation after playlist creation

#### Delivered:

**Rubric Items:**
- 2 additional custom functions (`ScanForDuplicatesAsync`, `ConfirmRemoveDuplicatesAsync`, `SuggestMusicByContextAsync`)
- 1+ action(s) require user confirmation to perform (duplicate removal requires user to select which tracks to remove)
- 1+ action(s) automatically adjust UI when performed (scan results update UI, suggestions display dynamically)
- Toasts / global notifications for agent actions (customized with green Spotify theme)
- Error handling for API requests (persistent error toasts with manual dismiss)

**Features:**
- **Enhanced Toast System**:
  - Custom styled toasts matching Spotify green theme
  - Success toasts: green background (#065f46), auto-dismiss after 4s
  - Error toasts: red background (#991b1b), persistent (require manual close)
  - Loading toasts: dark green background (#064e3b)
  - All toasts use white text and consistent icon theming
- **Backend DTOs**:
  - `RemoveDuplicatesDto.cs`: DuplicateGroup, DuplicateTrack, RemoveDuplicatesResponse, ConfirmRemoveDuplicatesRequest
  - `SuggestMusicDto.cs`: SuggestMusicRequest, SuggestedTrack, SuggestMusicResponse
- **Spotify Service Methods**:
  - `GetPlaylistAsync()`: Fetch individual playlist metadata
  - `GetPlaylistTracksAsync()`: Retrieve all tracks from a playlist with pagination
  - `RemoveTracksFromPlaylistAsync()`: Delete tracks from playlist
  - `GetUserPlaylistsAsync()`: List all user playlists with pagination
  - Added `SpotifyPlaylistTrack` model with addedAt timestamp
- **Agent Service Methods**:
  - `ScanForDuplicatesAsync()`: Intelligent duplicate detection
    - Normalizes track names (removes parentheses, brackets, extra spaces)
    - Fuzzy artist matching using set overlap
    - Groups duplicates by track name + artists
    - Recommends version to keep based on popularity and add date
    - Returns detailed duplicate groups with album info
  - `ConfirmRemoveDuplicatesAsync()`: User-confirmed duplicate removal
    - Removes selected track URIs from playlist
    - Logs action to database
    - Returns removal confirmation
  - `SuggestMusicByContextAsync()`: AI-powered contextual recommendations
    - Analyzes top 10 tracks from playlist
    - Uses AI to generate search queries based on context
    - Filters out tracks already in playlist
    - Provides reasoning for each suggestion
    - Returns up to 10 suggestions with metadata
- **Backend Endpoints** (AgentController):
  - `POST /api/agent/scan-duplicates`: Scan playlist for duplicates (returns scan results without modifying playlist)
  - `POST /api/agent/confirm-remove-duplicates`: Remove selected duplicates after user confirmation
  - `POST /api/agent/suggest-music`: Generate contextual music suggestions
  - `GET /api/spotify/playlists`: Retrieve user's Spotify playlists
- **Frontend Schemas & Types**:
  - `DuplicateTrackSchema`, `DuplicateGroupSchema`, `RemoveDuplicatesResponseSchema`
  - `ScanDuplicatesRequestSchema`, `ConfirmRemoveDuplicatesRequestSchema`
  - `SuggestedTrackSchema`, `SuggestMusicResponseSchema`, `SuggestMusicRequestSchema`
  - `SpotifyPlaylistSchema` for playlist metadata
  - All schemas exported as TypeScript types
- **Frontend API Functions**:
  - `agentApi.scanDuplicates()`: Scan for duplicates
  - `agentApi.confirmRemoveDuplicates()`: Confirm duplicate removal
  - `agentApi.suggestMusic()`: Get music suggestions
  - `spotifyApi.getPlaylists()`: Fetch user playlists
- **useAgent Hook Enhancements**:
  - `scanDuplicates()`: Scans playlist, shows success toast with duplicate count
  - `confirmRemoveDuplicates()`: Removes tracks, shows success toast with count
  - `suggestMusic()`: Generates suggestions, shows success toast with count
  - All methods include loading states, progress tracking, and error handling
  - Custom toast messages for each operation
- **Duplicate Cleaner Page** (DuplicateCleanerPage.tsx):
  - Playlist selector dropdown with track counts
  - Scan button with loading state
  - Duplicate groups display:
    - Each group shows track name and artists
    - Individual duplicate cards with:
      - Album name
      - Popularity score
      - Added date
      - Recommended badge (green highlight)
      - Checkbox for selection (disabled for recommended track)
  - "Select Recommended" button (selects all non-recommended tracks)
  - "Remove Selected" button with count badge
  - Scan results summary (duplicate groups and track counts)
  - No duplicates found message
  - "How it works" info panel
  - Spotify connection alert
  - Full conversation tracking
- **Music Suggestions Page** (SuggestionsPage.tsx):
  - Playlist selector dropdown
  - Context/description text input with helper text
  - Quick context example buttons:
    - "more upbeat and energetic"
    - "similar but more chill"
    - "different artists with same vibe"
    - "newer releases in the same genre"
    - "deeper cuts and b-sides"
  - Generate button with loading state
  - Suggestions display:
    - Track name and artists
    - AI-generated reason for each suggestion
    - Popularity score
    - "Play" button linking to Spotify URI
    - Hover effects with green border
  - Empty state for no suggestions
  - "How it works" info panel
  - Spotify connection alert
  - Full conversation tracking
- **Helper Methods**:
  - `NormalizeTrackName()`: Removes parentheses, brackets, and extra whitespace for duplicate matching
  - `AreArtistsSimilar()`: Uses set overlap to match artist combinations
  - `ParseReleaseDate()`: Safely parses date strings with fallback
- **UI/UX Improvements**:
  - Consistent green Spotify theme throughout
  - Loading states with spinners
  - Disabled states for invalid actions
  - Hover effects and transitions
  - Info panels with usage tips
  - Responsive layouts
  - Proper error boundaries
- **Error Handling**:
  - Try-catch blocks in all async operations
  - Detailed error logging with ILogger
  - User-friendly error messages in toasts
  - Persistent error toasts requiring manual dismissal
  - Graceful fallbacks for API failures
- **Bug Fixes**:
  - Fixed infinite conversation creation loop in DuplicateCleanerPage and SuggestionsPage (useEffect with empty deps + mounted flag)
  - Removed auto-rescan after duplicate removal (manual rescan with button)
  - Added "Sync with Spotify" buttons to refresh playlist lists without page reload
  - Fixed duplicate tracks in smart playlists with three-layer deduplication strategy:
    - trackIds HashSet for ID-based deduplication
    - trackUris HashSet for URI-based deduplication
    - Final loop with finalTrackUrisSet to enforce exact track count
  - Fixed playlist creation with multiple genre filters returning 0 results (changed to single genre filter with keyword mixing)
  - Fixed 400 Bad Request when adding >100 tracks to playlist (implemented batching with 100-track limit per request)
  - Fixed missing playlists in dropdown - playlists created by app were private (changed CreatePlaylistRequest Public parameter from false to true)
- **Spotify API Improvements**:
  - Updated `GetUserPlaylistsAsync()` to include explicit offset parameter: `?limit=50&offset=0`
  - Added comprehensive logging to playlist fetching with batch counts and totals
  - Implemented batching in `AddTracksToPlaylistAsync()` to respect Spotify's 100-track limit per request
  - Logs each batch addition with batch number for debugging
- **AI Service Enhancements**:
  - Added temperature (0.9) and top_p (0.95) parameters to increase response diversity
  - Added unique request identifiers using `DateTime.UtcNow.Ticks` to prevent caching
  - Added explicit anti-repetition instructions to all AI prompts
  - Updated all AI prompts with official Spotify Search API filter documentation:
    - Corrected filter usage: album, artist, track, year, genre, isrc
    - Changed strategy to use only ONE genre filter per query (multiple filters don't work reliably)
    - Keywords can be mixed with filters: `'upbeat dance genre:funk'`
    - Year ranges supported: `'year:1980-1990'`
    - Quotes for multi-word values: `'artist:"Daft Punk"'`
  - Query examples updated to reflect correct Spotify syntax
- **UI/UX Improvements**:
  - Enhanced Duplicate Cleaner page with clearer labeling:
    - Added "Song:" label before track name
    - Added "Found in X albums:" subtitle to clarify duplicate versions
    - Improved visual hierarchy with uppercase labels
  - Debug logging added to track playlist fetching through the system
  - Console logs for diagnosing playlist count issues
- **Backend Configuration**:
  - Updated Kubernetes ingress to use `mateo-spotify-api.duckdns.org` domain
  - Backend ingress now routes HTTPS traffic to backend service
  - TLS secret configuration updated for new domain


---

### Nov 12 (Week 5 - Agentic Loop & Persistence)

#### Estimates:

**Rubric Items:**
- Agentic loop runs until task complete or user intervention required
- LLM decisions and actions persisted and can be inspected
- Unit tests run automatically in pipeline
- Real-time WebSocket communication (Additional Task)

**Features:**
- Agent Control Center page with live status
- Agentic orchestration loop implementation
- Agent action history database persistence
- Activity History page with action logs
- WebSocket server setup for real-time updates
- WebSocket client integration
- Unit tests for agent functions
- Unit tests for API endpoints

#### Delivered:

**Rubric Items:**
- Agentic loop runs until task complete or user intervention required (smart playlist creation runs iterative search loop until track count met)
- LLM decisions and actions persisted and can be inspected (all agent actions logged to database with JSONB data)
- Unit tests run automatically in pipeline (backend service and repository tests)

**Features:**
- **Spotify Token Refresh & Persistence**:
  - SpotifyTokenService - Centralized token validation and refresh logic
  - RefreshAccessTokenAsync() - Automatic token renewal using refresh_token
  - GetValidAccessTokenAsync() - Validates token expiry (5-minute buffer) and auto-refreshes if needed
  - Token persistence in database - Stores access_token, refresh_token, and token_expiry
  - All Spotify API calls automatically use fresh tokens without user intervention
  - Users no longer need to reconnect Spotify after token expiry
  - Graceful error handling with clear messages if reconnection needed
  - SpotifyController updated to use token service for all endpoints
  - AgentService updated to use token service for all agent functions
- **Agent Control Center Page** (AgentControlPage.tsx):
  - Real-time agent status display (idle, processing, awaiting-approval, error)
  - Current task description with live updates
  - Progress percentage with visual progress bar
  - Active conversation details (ID, title, created date, action count)
  - Recent actions list with expandable details (last 10 actions)
  - Action type badges with color coding
  - Status indicators with appropriate styling
  - Detailed action logs with timestamps
  - Error message display for failed actions
  - Collapsible JSON result viewer with syntax highlighting
  - All conversations list with action count aggregation
  - Active conversation highlighting
  - Refresh functionality to reload data
  - Responsive layout with grid system
- **Activity History Page** (HistoryPage.tsx):
  - Comprehensive chronological log of all agent actions
  - Filtering by action type (CreateSmartPlaylist, DiscoverNewMusic, etc.)
  - Filtering by status (Processing, Completed, Failed, AwaitingApproval)
  - Combined filter support with clear filters button
  - Action count display with filter results
  - Color-coded action type badges (purple, blue, yellow, red, green)
  - Color-coded status indicators
  - Conversation ID linking for context
  - Duration calculation and display (mm:ss format)
  - Start and completion timestamps
  - Error message display in highlighted boxes
  - Collapsible parameters viewer (JSON formatted)
  - Collapsible result viewer (JSON formatted)
  - Refresh button for manual reload
  - Loading states with spinner animation
  - Empty state messages for no results
  - Hover effects for better UX
- **Backend Enhancements**:
  - GET /api/agent/history endpoint with query parameters
  - Action type filtering support
  - Status filtering support
  - Configurable limit parameter (default 50, max 100)
  - User-scoped action retrieval (only shows user's own actions)
  - AgentActionRepository.GetAllByConversationIdAsync() method
  - ConversationRepository.GetAllByUserIdAsync() method
  - Efficient database queries with joins
  - Comprehensive error logging
- **Frontend API Integration**:
  - agentApi.getHistory() with optional filter parameters
  - Type-safe API calls with Zod validation
  - AgentAction schema updated to include conversationId
  - Query parameter building for filters
  - Error handling for failed requests
- **Agentic Loop Implementation**:
  - Smart playlist creation uses iterative search strategy with AI-driven query adaptation
  - Loop continues until requested track count is met or maximum iterations reached
  - AI generates new search queries every 3 iterations when track count insufficient
  - Dynamic iteration limits based on requested track count: `Math.Max(20, requestedTrackCount / 10)`
  - Example: 250-track request gets 25 iterations (previously hardcoded at 10)
  - AI analyzes current tracks and generates diverse alternative search strategies mid-process
- **Triple-Layer Deduplication System**:
  - Track ID HashSet for Spotify ID-based deduplication
  - Track URI HashSet for URI-based deduplication
  - Track Name + Artist HashSet with normalization for semantic duplicate detection
  - `NormalizeTrackName()` removes parentheses, brackets, extra spaces
  - `AreArtistsSimilar()` uses set overlap matching for artist combinations
  - Prevents duplicate songs with same title from different albums
- **AI-Driven Query Adaptation for All Features**:
  - Smart playlists: AI adapts search queries every 3 iterations if track count insufficient
  - Music discovery: AI generates intelligent search queries based on user's saved tracks
  - Music suggestions: AI adapts queries every 3 iterations based on playlist context
  - All features use temperature=0.9 and top_p=0.95 for creative diversity
  - Anti-caching with unique request identifiers using timestamps
- **Recent Playlists History**:
  - Added `GetRecentPlaylistCreationsAsync()` to AgentActionRepository
  - Backend endpoint: `GET /api/agent/recent-playlists` (returns last 10 playlists)
  - SQL joins agent_actions with conversations, filters by user_id and action types
  - Frontend integration in PlaylistCreatorPage.tsx with "Recent Playlists" section
  - Displays playlist name, track count, created date, and "Open in Spotify" links
  - Auto-refreshes after creating new playlists
- **Configurable Music Suggestions**:
  - Added `Limit` parameter to SuggestMusicDto (5-50 tracks)
  - Frontend limit selector with predefined options (5, 10, 15, 20, 30, 40, 50)
  - Backend respects limit parameter and generates appropriate number of suggestions
  - AI adapts search strategy based on requested quantity
- **Batch Add-to-Playlist Functionality**:
  - Added checkbox selection system to music suggestions page
  - "Add to Playlist" button with selected track count badge
  - Backend endpoint: `POST /api/spotify/add-tracks-to-playlist`
  - Frontend API function: `spotifyApi.addTracksToPlaylist(playlistId, trackUris)`
  - Success toast shows count of tracks added
  - Checkbox states managed with Set<string> for efficient lookups
- **Select All/Deselect All for Bulk Operations**:
  - Toggle button that switches between "Select All" and "Deselect All" based on selection state
  - `handleSelectAll()` creates Set from all track URIs
  - `handleDeselectAll()` clears Set
  - Button placed next to context description for easy access
  - Enables quick selection of 20-50 suggestions without manual clicking
- **Bug Fixes**:
  - Fixed smart playlist creation returning only 12 tracks for 250-track requests (dynamic iteration limits)
  - Fixed duplicate tracks with same title appearing in playlists (triple deduplication)
  - Fixed music discovery 404 errors (improved error handling in recommendations API)
  - Fixed iteration limit bottleneck (changed from const 10 to dynamic based on track count)
  - Backend container restart required for iteration limit fix to take effect
- **AI Integration Improvements**:
  - Added explicit anti-repetition instructions to all AI prompts
  - Updated prompts with official Spotify Search API filter documentation
  - Corrected filter usage: only ONE genre filter per query (multiple don't work)
  - Keywords can be mixed with filters: `'upbeat dance genre:funk'`
  - Year ranges supported: `'year:1980-1990'`
  - Quotes for multi-word values: `'artist:"Daft Punk"'`
- **Enhanced Error Handling**:
  - Try-catch blocks around Spotify recommendations API calls
  - Graceful fallbacks when AI query generation fails
  - Persistent error toasts requiring manual dismissal
  - Comprehensive logging at each step of agentic loop
  - InvalidOperationException for token refresh failures with clear messages
- **Database Persistence**:
  - All agent actions logged to `agent_actions` table with JSONB data
  - Conversation tracking for all agent operations
  - Action history viewable on Dashboard (recent activity feed)
  - Recent playlists query with SQL joins for metadata retrieval
  - Token storage in users table with automatic updates
- **UX Improvements**:
  - Loading states with spinners during agentic loop execution
  - Progress tracking (though currently not granular iteration-by-iteration)
  - Success toasts with custom green Spotify theme
  - Clear feedback on track counts and duplicate removal
  - Select All/Deselect All for convenient bulk operations
  - Real-time status updates in Agent Control Center
  - Filterable and searchable action history
  - Color-coded visual indicators throughout
  - Responsive design for all new pages
  - Hover effects and smooth transitions


---

### Nov 15 (Week 6 - Additional Pages & UI Polish)

#### Estimates:

**Rubric Items:**
- 10+ pages/views with router (complete remaining pages)
- Streaming generation in UI (Additional Task - optional)

**Features:**
- Playlist Analytics page with visualizations
- Dashboard home page with metrics and quick actions
- Complete all 10 pages/views
- Chart.js or Recharts integration for analytics
- Audio feature visualization (radar charts, histograms)
- Genre distribution visualizations
- Streaming LLM responses in Playlist Creator
- UI polish and responsive design improvements

#### Delivered:

**Rubric Items:**


**Features:**


---

### Nov 19 (Week 7 - Advanced Features & Testing)

#### Estimates:

**Rubric Items:**
- User configurable benchmarking tool (Additional Task - optional)
- Comprehensive unit test coverage

**Features:**
- Agent performance benchmarking dashboard
- Benchmark configuration options
- Batch operations support (process multiple playlists)
- Playlist comparison tools
- Integration tests for Spotify API
- E2E tests for critical user flows
- Test coverage reporting in CI/CD

#### Delivered:

**Rubric Items:**


**Features:**


---

### Nov 22 (Week 8 - Optimization & UX)

#### Estimates:

**Rubric Items:**
- Performance optimization complete
- All error handling edge cases covered

**Features:**
- Lazy loading for large playlists
- Virtualization for long lists
- Optimistic UI updates
- Loading states and skeletons
- Rate limiting handling for Spotify API
- Token refresh logic
- User preference persistence
- Keyboard shortcuts and accessibility improvements

#### Delivered:

**Rubric Items:**


**Features:**


---

### Nov 25 (Week 9 - Integration & Bug Fixes)

#### Estimates:

**Rubric Items:**
- All rubric items integration tested
- Production deployment stable

**Features:**
- End-to-end feature integration testing
- Cross-browser compatibility testing
- Mobile responsiveness verification
- Bug fixes from testing
- Performance profiling and optimization
- Security audit (OAuth flow, API keys)
- Documentation updates

#### Delivered:

**Rubric Items:**


**Features:**


---

### Dec 3 (Week 10 - Buffer & Final Polish)

#### Estimates:

**Rubric Items:**
- All remaining rubric items completed
- Final production deployment

**Features:**
- Overflow work from previous weeks
- Final bug fixes
- UI/UX refinements based on testing
- Performance final optimizations
- Complete documentation
- Demo preparation
- User guide/help section

#### Delivered:

**Rubric Items:**


**Features:**


---

### Dec 6 (Final Submission)

#### Estimates:

**Rubric Items:**
- Final verification all rubric items complete
- Production deployment verified

**Features:**
- Final testing and validation
- Submission preparation
- Demo video/presentation materials
- Code cleanup and commenting
- README updates with deployment links
- Final deployment verification

#### Delivered:

**Rubric Items:**


**Features:**


---

## Notes
- Each check-in targets approximately 10% of project completion
- First two weeks focus on infrastructure (CI/CD, auth, deployment)
- Weeks 3-5 implement core agent functionality
- Weeks 6-8 complete remaining features and testing
- Weeks 9-10 serve as buffer for overflow and polish
- Production deployment maintained throughout for continuous testing


