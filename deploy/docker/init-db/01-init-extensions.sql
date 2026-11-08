-- CRMPro AI - Initial Database Setup & Extensions
-- Automatically executed by PostgreSQL container entrypoint

\connect crmpro_dev;

-- Enable Vector search extension for RAG embeddings
CREATE EXTENSION IF NOT EXISTS vector;

-- Enable UUID generation functions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Enable Trigram matching for fast fuzzy duplicate detection (Leads & Accounts)
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Create dedicated test database for local integration testing
SELECT 'CREATE DATABASE crmpro_test'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'crmpro_test')\gexec

\connect crmpro_test;

CREATE EXTENSION IF NOT EXISTS vector;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS pg_trgm;

GRANT ALL PRIVILEGES ON DATABASE crmpro_dev TO crmpro_app;
GRANT ALL PRIVILEGES ON DATABASE crmpro_test TO crmpro_app;
