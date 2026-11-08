# ADR-005: Database & Vector Search — PostgreSQL 16+, EF Core 10 Code First & pgvector

## Status
**Accepted** (2026-11-08)

## Context
CRMPro AI requires:
- High transactional reliability (ACID) for commercial deals, accounts, and ticket histories.
- Semantic retrieval (RAG) over knowledge base articles and uploaded business documents (PDF, DOCX, TXT).
- Fast full-text search across customer interactions.
- Schema migrations that are versioned, reproducible, and verifiable in CI.

Introducing a separate vector database (e.g., Pinecone, Milvus, Qdrant) introduces dual-write consistency problems, cross-system sync latency, separate backup strategies, and security leaks where vector ACLs get out of sync with CRM permissions.

## Decision
We standardize on **PostgreSQL 16+** with the **`pgvector`** extension as the single unified persistence engine:

1. **EF Core 10 Code First Migrations**:
   - Entities and fluent relationships are defined in `CRMProAI.Infrastructure`.
   - Migrations are generated via dotnet EF CLI and committed to source control.
   - In production, migrations are executed as an automated pre-deployment step (migration bundles); runtime auto-migration during API startup is explicitly forbidden.
2. **`pgvector` Extension Integration**:
   - The PostgreSQL `vector` extension is enabled.
   - Knowledge chunks store dense vector embeddings using `Vector(1536)` (or model-configured dimensions).
   - HNSW (`halfvec_cosine_ops` / `vector_cosine_ops`) or IVFFlat indexes are defined for sub-millisecond approximate nearest neighbor searches.
3. **Hybrid Search in a Single Query**:
   - Knowledge search combines PostgreSQL full-text search (`tsvector` / `tsquery`) with pgvector similarity in a single SQL query:
     ```sql
     SELECT id, title, chunk_text, 
            (1 - (embedding <=> @query_vector)) AS vector_similarity,
            ts_rank(tsv_content, plainto_tsquery(@query_text)) AS text_rank
     FROM document_chunks
     WHERE tenant_id = @tenant_id 
       AND publication_status = 'Published'
       AND is_archived = FALSE
     ORDER BY (vector_similarity * 0.7 + text_rank * 0.3) DESC
     LIMIT @top_k;
     ```
4. **Data Typing Conventions**:
   - Primary keys: `uuid` (UUID v7/v4).
   - Timestamps: `timestamptz` (stored and returned in UTC).
   - Currency: `numeric(18,4)` with separate ISO currency code column.
   - Concurrency: `xmin` system token or explicit concurrency column for optimistic locking.

## Consequences

### Positive
- Unified backup, point-in-time recovery (PITR), and disaster recovery across structured CRM data and vector embeddings.
- RLS and tenant filtering apply identically to vector chunks and CRM records.
- Zero distributed synchronization overhead between CRM updates and search indexes.

### Negative / Trade-offs
- Dedicated pgvector extension must be installed in PostgreSQL instances/containers.
- Large vector datasets require adequate RAM allocation for HNSW index graphs.
