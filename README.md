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

[ ] Real-time WebSocket communication for agent status updates

[ ] Streaming generation in the UI for agent responses

[ ] User configurable benchmarking tool for agent performance

### Technical Requirements

[x] Deployed in production (public internet or class Kubernetes cluster)

[x] CI/CD pipeline configured

[x] Unit tests run automatically in pipeline

[x] Linter runs automatically in pipeline

[ ] Data persisted on server for multi-client access

### Technology Requirements

[ ] Global client-side state management (Zustand/Redux)

[ ] Toasts / global notifications for agent actions

[ ] Error handling (API requests and render errors)

[ ] Network calls - read data (GET playlists, tracks)

[ ] Network calls - write data (POST/PUT/DELETE playlists)

[x] Developer type helping (TypeScript)

[ ] 10+ pages/views with router

[x] CI/CD pipeline

[x] Live production environment

[x] Automated testing and linting in pipeline (abort on fail)

[ ] 3+ reusable form input components

[ ] 2+ reusable layout components

[ ] Authentication and user account support (Spotify OAuth)

[ ] Authorized pages and public pages

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
- Production domains configured (mateo-spotify.duckdns.org, api.mateo-spotify.duckdns.org)
- TLS/SSL secrets configured


---

### Nov 1 (Week 2 - Core Infrastructure)

#### Estimates:

**Rubric Items:**
- Global client-side state management (Zustand setup)
- Developer type helping (TypeScript throughout)
- 2+ reusable layout components (MainLayout, AuthLayout)
- 3+ reusable form input components (TextInput, Select, Button)
- Error handling for render errors (Error Boundary)
- Data persisted on server setup (backend API initialization)

**Features:**
- Spotify OAuth 2.0 integration (connect Spotify within user account)
- State management store structure
- Reusable layout components (MainLayout, AuthLayout, Sidebar)
- Reusable form components (TextInput, SelectDropdown, Button, SearchInput)
- Error boundary component
- Settings page with Spotify connection (authorized)
- Backend API scaffolding (Express.js or similar)
- Database schema design for user data and agent logs
- Spotify token storage and management

#### Delivered:

**Rubric Items:**


**Features:**


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


**Features:**


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


**Features:**


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


**Features:**


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


