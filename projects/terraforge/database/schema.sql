-- TerraForge PostgreSQL Database Schema
-- A Minecraft-like world persistence layer
-- Version: 1.0.0
-- Last Updated: 2026-02-09

-- Enable necessary extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Core table: Players
-- Stores all player accounts and their metadata
CREATE TABLE players (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    username        VARCHAR(32) NOT NULL UNIQUE,
    created_at      TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    last_login      TIMESTAMP WITH TIME ZONE,
    
    -- Constraints
    CONSTRAINT chk_username_length CHECK (LENGTH(username) >= 3 AND LENGTH(username) <= 32),
    CONSTRAINT chk_username_alphanumeric CHECK (username ~ '^[a-zA-Z0-9_]+$')
);

-- Core table: Chunks
-- Stores world chunk data indexed by coordinates
-- Each chunk represents a 16x16 block region
CREATE TABLE chunks (
    x               INTEGER NOT NULL,
    z               INTEGER NOT NULL,
    data            BYTEA NOT NULL,
    modified_at     TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    -- Composite primary key on chunk coordinates
    CONSTRAINT pk_chunks PRIMARY KEY (x, z),
    
    -- Ensure coordinates are within world bounds
    CONSTRAINT chk_chunk_x CHECK (x >= -2147483648 AND x <= 2147483647),
    CONSTRAINT chk_chunk_z CHECK (z >= -2147483648 AND z <= 2147483647)
);

-- Core table: Player Positions
-- Tracks current location of each player in the world
-- Separate from players table to allow for world-specific positioning
CREATE TABLE player_positions (
    player_id       UUID NOT NULL,
    x               DOUBLE PRECISION NOT NULL,
    y               DOUBLE PRECISION NOT NULL,
    z               DOUBLE PRECISION NOT NULL,
    chunk_x         INTEGER NOT NULL,
    chunk_z         INTEGER NOT NULL,
    
    -- Primary key and foreign key
    CONSTRAINT pk_player_positions PRIMARY KEY (player_id),
    CONSTRAINT fk_player_positions_player
        FOREIGN KEY (player_id) 
        REFERENCES players(id) 
        ON DELETE CASCADE,
    
    -- Validate position coordinates
    CONSTRAINT chk_player_x CHECK (x >= -30000000 AND x <= 30000000),
    CONSTRAINT chk_player_y CHECK (y >= -2048 AND y <= 2048),
    CONSTRAINT chk_player_z CHECK (z >= -30000000 AND z <= 30000000)
);

-- Core table: Inventories
-- Stores player inventory contents
-- Supports multiple slots per player (typically 36 slots + hotbar)
CREATE TABLE inventories (
    player_id       UUID NOT NULL,
    slot            SMALLINT NOT NULL,
    item            VARCHAR(256) NOT NULL DEFAULT '',
    quantity        INTEGER NOT NULL DEFAULT 0,
    
    -- Composite primary key on player and slot
    CONSTRAINT pk_inventories PRIMARY KEY (player_id, slot),
    CONSTRAINT fk_inventories_player
        FOREIGN KEY (player_id) 
        REFERENCES players(id) 
        ON DELETE CASCADE,
    
    -- Slot numbering constraints (0-99 for backpack + armor + offhand)
    CONSTRAINT chk_slot_range CHECK (slot >= 0 AND slot < 100),
    CONSTRAINT chk_quantity_positive CHECK (quantity >= 0),
    CONSTRAINT chk_quantity_max CHECK (quantity <= 64)
);

-- ============================================
-- INDEXES
-- ============================================

-- Players indexes for faster lookups
CREATE INDEX idx_players_username ON players(username);
CREATE INDEX idx_players_last_login ON players(last_login);
CREATE INDEX idx_players_created_at ON players(created_at);

-- Chunks indexes for spatial queries
CREATE INDEX idx_chunks_modified ON chunks(modified_at DESC);

-- Player positions indexes for location-based queries
CREATE INDEX idx_player_positions_chunk ON player_positions(chunk_x, chunk_z);
CREATE INDEX idx_player_positions_coords ON player_positions(x, y, z);

-- Inventory indexes for item lookups
CREATE INDEX idx_inventories_item ON inventories(item) WHERE item != '';

-- ============================================
-- TRIGGERS
-- ============================================

-- Function to auto-update modified_at on chunks
CREATE OR REPLACE FUNCTION update_modified_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.modified_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Trigger for chunks table
CREATE TRIGGER trigger_chunks_modified
    BEFORE UPDATE ON chunks
    FOR EACH ROW
    EXECUTE FUNCTION update_modified_column();

-- Function to auto-update last_login on players
CREATE OR REPLACE FUNCTION update_login_timestamp()
RETURNS TRIGGER AS $$
BEGIN
    NEW.last_login = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Trigger for players when they join (can be called explicitly)
-- Note: This trigger runs on UPDATE to refresh last_login
CREATE TRIGGER trigger_players_login
    BEFORE UPDATE OF last_login ON players
    FOR EACH ROW
    EXECUTE FUNCTION update_login_timestamp();

-- ============================================
-- VIEWS
-- ============================================

-- View: Active players (logged in within last 24 hours)
CREATE VIEW active_players AS
SELECT 
    p.id,
    p.username,
    p.created_at,
    p.last_login,
    CASE 
        WHEN p.last_login > NOW() - INTERVAL '1 hour' THEN 'online'
        WHEN p.last_login > NOW() - INTERVAL '24 hours' THEN 'recently_active'
        ELSE 'offline'
    END AS status
FROM players p
WHERE p.last_login > NOW() - INTERVAL '24 hours';

-- View: Player full state (position + inventory summary)
CREATE VIEW player_state AS
SELECT 
    p.id,
    p.username,
    pos.x,
    pos.y,
    pos.z,
    pos.chunk_x,
    pos.chunk_z,
    COUNT(i.slot) FILTER (WHERE i.quantity > 0) AS occupied_slots
FROM players p
LEFT JOIN player_positions pos ON p.id = pos.player_id
LEFT JOIN inventories i ON p.id = i.player_id
GROUP BY p.id, p.username, pos.x, pos.y, pos.z, pos.chunk_x, pos.chunk_z;

-- View: Chunk statistics
CREATE VIEW chunk_statistics AS
SELECT 
    c.x,
    c.z,
    c.modified_at,
    COUNT(pp.player_id) AS players_in_chunk
FROM chunks c
LEFT JOIN player_positions pp 
    ON c.x = pp.chunk_x AND c.z = pp.chunk_z
GROUP BY c.x, c.z, c.modified_at;

-- ============================================
-- INITIAL DATA & PERMISSIONS
-- ============================================

-- Add comment documentation for tables
COMMENT ON TABLE players IS 'Stores all player accounts and authentication metadata';
COMMENT ON TABLE chunks IS 'World chunk data - 16x16 block regions stored as binary blob';
COMMENT ON TABLE player_positions IS 'Current location of each player in the world';
COMMENT ON TABLE inventories IS 'Player inventory contents - one row per occupied slot';

-- Add column comments
COMMENT ON COLUMN players.username IS 'Unique player name, 3-32 alphanumeric characters';
COMMENT ON COLUMN players.last_login IS 'Timestamp of last player login for activity tracking';
COMMENT ON COLUMN chunks.data IS 'Serialized chunk data including blocks, entities, and tile entities';
COMMENT ON COLUMN chunks.modified_at IS 'Last time this chunk was modified (auto-updated)';
COMMENT ON COLUMN player_positions.chunk_x IS 'X coordinate of the chunk containing this player';
COMMENT ON COLUMN inventories.slot IS 'Inventory slot 0-35 (backpack/hotbar), 36-39 (armor), 40 (offhand)';
COMMENT ON COLUMN inventories.item IS 'Namespaced item ID (e.g., minecraft:diamond_sword)';

-- Schema version tracking
CREATE TABLE IF NOT EXISTS schema_version (
    version     VARCHAR(16) PRIMARY KEY,
    applied_at  TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    notes       TEXT
);

-- Record this schema version
INSERT INTO schema_version (version, notes) 
VALUES ('1.0.0', 'Initial TerraForge schema with players, chunks, positions, and inventories');
