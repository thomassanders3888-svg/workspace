-- TerraForge Database Schema
-- PostgreSQL schema for voxel game persistence

-- Players table for user data
CREATE TABLE IF NOT EXISTS players (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(32) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    display_name VARCHAR(64),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    last_login TIMESTAMPTZ,
    last_position_x FLOAT DEFAULT 0,
    last_position_y FLOAT DEFAULT 100,
    last_position_z FLOAT DEFAULT 0,
    health INTEGER DEFAULT 20,
    hunger INTEGER DEFAULT 20,
    experience INTEGER DEFAULT 0,
    inventory_json JSONB DEFAULT '[]',
    is_online BOOLEAN DEFAULT FALSE,
    is_banned BOOLEAN DEFAULT FALSE
);

-- Chunks table for voxel world data
CREATE TABLE IF NOT EXISTS chunks (
    id BIGINT GENERATED ALWAYS AS (
        ((x_coord::BIGINT + 2147483648) << 32) | 
        (z_coord::BIGINT + 2147483648)
    ) STORED PRIMARY KEY,
    x_coord INTEGER NOT NULL,
    y_coord INTEGER NOT NULL DEFAULT 0,
    z_coord INTEGER NOT NULL,
    world VARCHAR(64) DEFAULT 'world',
    data BYTEA NOT NULL,
    compressed BOOLEAN DEFAULT TRUE,
    version INTEGER DEFAULT 1,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    modified_at TIMESTAMPTZ DEFAULT NOW(),
    UNIQUE(x_coord, y_coord, z_coord, world)
);

-- Items table for item definitions and metadata
CREATE TABLE IF NOT EXISTS items (
    id SERIAL PRIMARY KEY,
    item_id VARCHAR(64) UNIQUE NOT NULL,
    display_name VARCHAR(128),
    category VARCHAR(32) DEFAULT 'misc',
    max_stack INTEGER DEFAULT 64,
    durability INTEGER,
    properties JSONB DEFAULT '{}',
    icon_path VARCHAR(255),
    is_craftable BOOLEAN DEFAULT FALSE,
    craft_recipe JSONB
);

-- World state table for global server state
CREATE TABLE IF NOT EXISTS world_state (
    key VARCHAR(128) PRIMARY KEY,
    value JSONB NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- Player sessions for tracking
CREATE TABLE IF NOT EXISTS player_sessions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id UUID REFERENCES players(id) ON DELETE CASCADE,
    session_token VARCHAR(255) UNIQUE NOT NULL,
    ip_address INET,
    started_at TIMESTAMPTZ DEFAULT NOW(),
    ended_at TIMESTAMPTZ,
    is_active BOOLEAN DEFAULT TRUE
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_chunks_world ON chunks(world);
CREATE INDEX IF NOT EXISTS idx_chunks_coords ON chunks(x_coord, z_coord);
CREATE INDEX IF NOT EXISTS idx_players_online ON players(is_online) WHERE is_online = TRUE;
CREATE INDEX IF NOT EXISTS idx_sessions_active ON player_sessions(is_active) WHERE is_active = TRUE;
