# Coding Standards & Engineering Conventions

This document defines the strict engineering conventions, architectural constraints, and coding standards for **CRMPro AI**.

---

## 1. Clean Architecture & Dependency Direction

The solution is divided into strictly layered assemblies:

```
        ┌────────────────────────────────────────────────────────┐
        │                        CRMProAI.Api                    │
        └───────────────────────────┬────────────────────────────┘
                                    │ References
                                    ▼
        ┌────────────────────────────────────────────────────────┐
        │                  CRMProAI.Application                 │
        └──────────────┬──────────────────────────┬──────────────┘
                       │ References               ▲ Implemented by
                       ▼                          │
        ┌─────────────────────────────┐ ┌────────────────────────┐
        │       CRMProAI.Domain       │ │  CRMProAI.Infrastructure │
        └─────────────────────────────┘ └────────────────────────┘
```

### Invariant Rules
1. **`CRMProAI.Domain`**:
   - Zero external dependencies (no EF Core, ASP.NET Core, MediatR, or third-party SDKs).
   - Contains Entities, Value Objects, Domain Events, Domain Exceptions, and Aggregate Roots.
   - All state transitions and business invariants must be enforced within domain models.
2. **`CRMProAI.Application`**:
   - Depends **only** on `CRMProAI.Domain`.
   - Contains CQRS commands, queries, DTOs, FluentValidation rules, and port/interface definitions.
   - Does not reference EF Core, database drivers, or web framework packages.
3. **`CRMProAI.Infrastructure`**:
   - Implements ports defined in `CRMProAI.Application` (repositories, email providers, AI adapters, Redis, RabbitMQ Outbox).
   - Manages EF Core DbContext, entity configurations, and migrations.
4. **`CRMProAI.Api`**:
   - Composition root and HTTP endpoints.
   - Translates HTTP requests to Application commands/queries; returns standard DTOs.
   - Contains **no business logic**.

---

## 2. Multi-Tenancy & Authorization Rules

1. **Non-Nullable `TenantId`**:
   - Every tenant-scoped entity must inherit from `ITenantScoped` and define `public Guid TenantId { get; private set; }`.
   - Global/platform entities (e.g., platform logs, system catalogs) must be explicitly separated and never use a nullable `TenantId` as a shortcut.
2. **Tenant Resolution**:
   - The current tenant is resolved via authenticated claims (`tenant_id`) or validated custom domain mapping.
   - **Never trust a client-supplied `tenant_id` query parameter or body property.**
3. **Defense in Depth**:
   - EF Core Global Query Filters (`HasQueryFilter(e => e.TenantId == _tenantContext.TenantId)`).
   - Composite unique indexes including `TenantId` (e.g., `(TenantId, Email)`).
   - PostgreSQL Row-Level Security (RLS) configured as a defense-in-depth database barrier.
4. **Permissions**:
   - Authorization handlers check granular permission keys (e.g., `leads.create`, `opportunities.stage.change`, `tickets.internal-note.read`).
   - Customer Portal users have an isolated security role and cannot query internal notes, attachments, or other organizations' tickets.

---

## 3. C# 14 & .NET 10 Standards

- **Language Features**: Use file-scoped namespaces (`namespace CRMProAI.Domain.Leads;`), nullable reference types enabled (`<Nullable>enable</Nullable>`), and pattern matching.
- **Primary Keys**: Always `Guid` (UUID v7 or v4).
- **Timestamps**: Always `DateTimeOffset` in UTC (`DateTimeOffset.UtcNow`).
- **Money & Currencies**: Always use `decimal` for monetary values (never `float` or `double`). Store the 3-letter ISO currency code alongside the amount.
- **DTOs**: Use `record` or `record class` for immutable request and response DTOs.
- **Cancellation Tokens**: Every asynchronous API and repository method must accept a `CancellationToken ct = default`.
- **Concurrency**: Use PostgreSQL `xmin` system column or explicit concurrency tokens for optimistic locking; return `409 Conflict` on collisions.

---

## 4. API Design Standards

- **Routing**: Base path is `/api/v1/{resource}`. Plural nouns for resource names (`/api/v1/leads`, `/api/v1/opportunities`).
- **Error Responses**: All HTTP errors conform to **RFC 7807 ProblemDetails** with a consistent `traceId`, machine-readable error `code`, and human-readable `detail`.
- **Status Codes**:
  - `200 OK`: Successful retrieval or update.
  - `201 Created`: Successful creation with `Location` header pointing to new resource.
  - `204 No Content`: Successful soft-delete/archive.
  - `400 Bad Request`: Validation failure.
  - `401 Unauthorized`: Missing or invalid JWT.
  - `403 Forbidden`: Authenticated user lacks required permission or tenant scope.
  - `404 Not Found`: Resource does not exist or user is not authorized to see its existence.
  - `409 Conflict`: Concurrency conflict or business invariant violation.
- **Pagination & Sorting**:
  - Query parameters: `page` (1-based), `pageSize` (capped at 100), `sortBy`, `sortDirection` (`asc` / `desc`).
  - Response wrapper: `PagedList<T> { Items, TotalCount, Page, PageSize, TotalPages }`.

---

## 5. AI & Security Guardrails

- **Deterministic Core**: Calculations, SLA timers, and state transitions are always computed deterministically. AI does not perform accounting or change statuses autonomously.
- **Allowlisted Tools**: AI tool calling uses strictly typed schemas, validated tenant context, and inherits caller permissions.
- **Approval State Machine**: Actions modifying records or communicating externally must produce an `AiPendingAction` requiring human review and approval.
- **Untrusted Input**: Retrieved RAG document chunks are treated as untrusted text. System instructions cannot be overridden by document contents.
- **Secrets**: Never commit passwords, connection strings, or API keys. Load from environment variables or User Secrets in development.

---

## 6. Frontend Standards (Angular 20+)

- **Standalone Architecture**: All components, directives, and pipes are `standalone: true`. No legacy `NgModule`.
- **State Management**: Utilize Angular **Signals** (`signal()`, `computed()`) for component reactivity and RxJS where event streams are needed.
- **Component Design**: UI built with **PrimeNG** components styled using **Tailwind CSS**.
- **Forms**: Strictly typed **Reactive Forms** (`FormGroup`, `FormControl`). Prevent double-submission and display inline field errors.
- **Route Guards**: Protect all authenticated and portal routes with tenant and permission guards.
