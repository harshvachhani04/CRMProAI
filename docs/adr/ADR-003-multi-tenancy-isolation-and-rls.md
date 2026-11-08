# ADR-003: Multi-Tenancy Strategy — Logical Separation with PostgreSQL Row-Level Security

## Status
**Accepted** (2026-11-08)

## Context
CRMPro AI is a SaaS platform serving multiple independent customer organizations (tenants). Each tenant stores proprietary and confidential commercial information: customer lists, deals, proposal pricing, contracts, internal tickets, and knowledge documents. Cross-tenant leakage is a critical business risk and must be impossible across standard APIs, background jobs, vector searches, and AI tool operations.

At the same time, maintaining separate databases per tenant for hundreds of small-to-medium clients creates high infrastructure overhead, slow schema migrations, and connection pool exhaustion.

## Decision
We implement a **shared database, shared schema** architecture with defense-in-depth isolation across three tiers:

1. **Non-Nullable `TenantId` Column**:
   - Every tenant-owned table contains `TenantId UUID NOT NULL`.
   - Platform/global tables (e.g., system configuration, platform audit) are explicitly separated from tenant data and never use a nullable `TenantId` as a shortcut.
   - All unique constraints and performance indexes on tenant tables are scoped by `TenantId` (e.g., `UNIQUE (TenantId, NormalizedEmail)`).
2. **Trusted Tenant Resolution & EF Core Global Query Filters**:
   - The tenant context is resolved strictly from trusted authenticated claims (`tenant_id`) in the JWT or validated subdomain host mapping. Client-supplied query parameters or headers for `TenantId` are discarded and forbidden for authorization.
   - All tenant-owned aggregates implement `ITenantScoped`. EF Core applies Global Query Filters automatically (`e => e.TenantId == _tenantContext.TenantId`) to all queries.
3. **PostgreSQL Row-Level Security (RLS) as Defense in Depth**:
   - PostgreSQL RLS policies are applied to tenant tables:
     ```sql
     ALTER TABLE leads ENABLE ROW LEVEL SECURITY;
     CREATE POLICY tenant_isolation_policy ON leads
     USING (tenant_id = NULLIF(current_setting('app.current_tenant_id', true), '')::uuid);
     ```
   - Before executing queries within a connection/transaction, the application sets the session parameter: `SET LOCAL app.current_tenant_id = '...'`.
   - This prevents cross-tenant access even if a raw SQL query or missing EF filter bypass occurs.

## Consequences

### Positive
- Maximum security: leakage is blocked at both application (EF Core) and database engine (PostgreSQL RLS) levels.
- Efficient operational costs: single database to back up, monitor, and scale.
- Vector search with pgvector naturally inherits tenant filtering in SQL queries.

### Negative / Trade-offs
- Every database transaction must reliably set the tenant session parameter via connection interceptors.
- Superuser database connections bypass RLS, so application connection strings must use a non-superuser application role.
