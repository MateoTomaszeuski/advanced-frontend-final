-- Create database schema for Spotify AI Agent

-- Users table
CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    email VARCHAR(255) NOT NULL UNIQUE,
    display_name VARCHAR(255),
    spotify_access_token TEXT,
    spotify_refresh_token TEXT,
    spotify_token_expiry TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_users_email ON users(email);

-- Conversations table
CREATE TABLE IF NOT EXISTS conversations (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    title VARCHAR(500) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_conversations_user_id ON conversations(user_id);
CREATE INDEX idx_conversations_created_at ON conversations(created_at DESC);

-- Agent actions table
CREATE TABLE IF NOT EXISTS agent_actions (
    id SERIAL PRIMARY KEY,
    conversation_id INTEGER NOT NULL REFERENCES conversations(id) ON DELETE CASCADE,
    action_type VARCHAR(100) NOT NULL,
    status VARCHAR(50) NOT NULL,
    input_prompt TEXT,
    parameters JSONB,
    result JSONB,
    error_message TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP
);

CREATE INDEX idx_agent_actions_conversation_id ON agent_actions(conversation_id);
CREATE INDEX idx_agent_actions_action_type ON agent_actions(action_type);
CREATE INDEX idx_agent_actions_status ON agent_actions(status);
CREATE INDEX idx_agent_actions_created_at ON agent_actions(created_at DESC);

-- Function to update updated_at timestamp
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Triggers for updated_at
CREATE TRIGGER update_users_updated_at
    BEFORE UPDATE ON users
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_conversations_updated_at
    BEFORE UPDATE ON conversations
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- Comments for documentation
COMMENT ON TABLE users IS 'User accounts with Spotify integration';
COMMENT ON TABLE conversations IS 'Agent conversation sessions';
COMMENT ON TABLE agent_actions IS 'Individual agent actions within conversations';

COMMENT ON COLUMN users.spotify_access_token IS 'Spotify OAuth access token';
COMMENT ON COLUMN users.spotify_refresh_token IS 'Spotify OAuth refresh token';
COMMENT ON COLUMN users.spotify_token_expiry IS 'Expiration time for Spotify access token';

COMMENT ON COLUMN agent_actions.action_type IS 'Type of agent action: CreateSmartPlaylist, DiscoverNewMusic, RemoveDuplicates, SuggestMusicByContext';
COMMENT ON COLUMN agent_actions.status IS 'Action status: Processing, Completed, Failed, AwaitingApproval';
COMMENT ON COLUMN agent_actions.parameters IS 'JSON parameters for the action';
COMMENT ON COLUMN agent_actions.result IS 'JSON result of the action';
