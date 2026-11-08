# Architecture Decision Records (ADRs)

This directory documents the significant architectural and technical decisions made for **CRMPro AI**. Each record captures the context, decision, and consequences in a standardized, immutable format.

---

## Index of Decisions

| ADR ID | Title | Status | Date |
|---|---|---|---|
| [ADR-001](ADR-001-modular-monolith.md) | Architectural Style: Modular Monolith over Distributed Microservices | **Accepted** | 2026-11-08 |
| [ADR-002](ADR-002-clean-architecture-layering.md) | Architectural Layering: Clean Architecture with DDD-Informed Invariants | **Accepted** | 2026-11-08 |
| [ADR-003](ADR-003-multi-tenancy-isolation-and-rls.md) | Multi-Tenancy Strategy: Logical Separation with PostgreSQL Row-Level Security | **Accepted** | 2026-11-08 |
| [ADR-004](ADR-004-authentication-and-token-lifecycle.md) | Identity & Access: ASP.NET Core Identity with Rotating Refresh Tokens | **Accepted** | 2026-11-08 |
| [ADR-005](ADR-005-postgresql-efcore-pgvector.md) | Database & Vector Search: PostgreSQL 16+, EF Core 10 Code First & pgvector | **Accepted** | 2026-11-08 |
| [ADR-006](ADR-006-ai-provider-abstraction-and-governance.md) | AI Architecture: Microsoft.Extensions.AI, Prompt Registry & Human-in-the-Loop | **Accepted** | 2026-11-08 |
| [ADR-007](ADR-007-api-design-versioning-and-error-handling.md) | API Conventions: REST /api/v1, DTO Boundaries & RFC 7807 ProblemDetails | **Accepted** | 2026-11-08 |
| [ADR-008](ADR-008-tabular-export-closedxml-xlsx.md) | Export Strategy: ClosedXML (.xlsx Only) with Formula Injection Defense | **Accepted** | 2026-11-08 |
