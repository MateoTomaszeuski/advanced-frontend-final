# Database Setup

This directory contains SQL scripts for initializing the Spotify AI Agent database.

## Files

- **init.sql**: Creates the database schema (tables, indexes, triggers)
- **seed.sql**: Inserts seed data for development/testing

## Schema Overview

### Tables

#### users
Stores user account information and Spotify OAuth tokens.

| Column | Type | Description |
|--------|------|-------------|
| id | SERIAL | Primary key |
| email | VARCHAR(255) | Unique user email (from Keycloak) |
| display_name | VARCHAR(255) | User's display name |
| spotify_access_token | TEXT | Spotify OAuth access token |
| spotify_refresh_token | TEXT | Spotify OAuth refresh token |
| spotify_token_expiry | TIMESTAMP | Token expiration time |
| created_at | TIMESTAMP | Record creation timestamp |
| updated_at | TIMESTAMP | Record update timestamp |

#### conversations
Stores agent conversation sessions.

| Column | Type | Description |
|--------|------|-------------|
| id | SERIAL | Primary key |
| user_id | INTEGER | Foreign key to users table |
| title | VARCHAR(500) | Conversation title |
| created_at | TIMESTAMP | Record creation timestamp |
| updated_at | TIMESTAMP | Record update timestamp |

#### agent_actions
Stores individual agent actions within conversations.

| Column | Type | Description |
|--------|------|-------------|
| id | SERIAL | Primary key |
| conversation_id | INTEGER | Foreign key to conversations table |
| action_type | VARCHAR(100) | Type of action (CreateSmartPlaylist, DiscoverNewMusic, etc.) |
| status | VARCHAR(50) | Action status (Processing, Completed, Failed, AwaitingApproval) |
| input_prompt | TEXT | User's input prompt |
| parameters | JSONB | Action parameters in JSON format |
| result | JSONB | Action result in JSON format |
| error_message | TEXT | Error message if action failed |
| created_at | TIMESTAMP | Record creation timestamp |
| completed_at | TIMESTAMP | Action completion timestamp |

## Docker Integration

The SQL scripts are automatically executed when the PostgreSQL container starts via docker-compose:

```yaml
volumes:
  - ../app/db/init.sql:/docker-entrypoint-initdb.d/01-init.sql
  - ../app/db/seed.sql:/docker-entrypoint-initdb.d/02-seed.sql
```

Scripts in `/docker-entrypoint-initdb.d/` are executed in alphabetical order when the database is first initialized.

## Usage

### Via Docker Compose
```bash
cd docker-compose
docker-compose up postgres
```

### Manual Setup
```bash
psql -U spotifyuser -d spotifydb -f app/db/init.sql
psql -U spotifyuser -d spotifydb -f app/db/seed.sql
```

### Connect to Database
```bash
# Via docker-compose
docker exec -it spotify-postgres psql -U spotifyuser spotifydb

# Direct connection
psql -h localhost -U spotifyuser -d spotifydb
```

## Indexes

The following indexes are created for optimal query performance:

- `idx_users_email` - Fast user lookup by email
- `idx_conversations_user_id` - Fast conversation lookup by user
- `idx_conversations_created_at` - Sorted conversation retrieval
- `idx_agent_actions_conversation_id` - Fast action lookup by conversation
- `idx_agent_actions_action_type` - Filter actions by type
- `idx_agent_actions_status` - Filter actions by status
- `idx_agent_actions_created_at` - Sorted action retrieval

## Triggers

- `update_users_updated_at` - Automatically updates `updated_at` on user record changes
- `update_conversations_updated_at` - Automatically updates `updated_at` on conversation record changes
