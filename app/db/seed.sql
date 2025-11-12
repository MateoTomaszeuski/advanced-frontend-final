-- Seed data for development and testing

-- Note: In production, users will be created automatically via Keycloak authentication
-- This seed data is for local development testing only

-- Example user (replace with your actual test user email)
INSERT INTO users (email, display_name, created_at, updated_at)
VALUES 
    ('mateofake@gmail.com', 'Mateo Tomaszeuski', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
ON CONFLICT (email) DO NOTHING;

-- Example conversation for the test user
INSERT INTO conversations (user_id, title, created_at, updated_at)
SELECT 
    id,
    'Getting Started with AI Agent',
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
FROM users
WHERE email = 'mateofake@gmail.com'
ON CONFLICT DO NOTHING;

-- Note: Agent actions will be created automatically as users interact with the system
-- No seed data needed for agent_actions table
