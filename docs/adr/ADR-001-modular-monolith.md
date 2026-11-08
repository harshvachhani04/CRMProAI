# ADR-001: Architectural Style — Modular Monolith over Distributed Microservices

## Status
**Accepted** (2026-11-08)

## Context
CRMPro AI is designed as a multi-tenant B2B CRM tailored for small and medium-sized IT services and software development firms (5–200 internal users per organization). The platform encompasses closely related business domains: Leads, Accounts & Contacts, Opportunity Pipeline, Proposals, Activities, Support Tickets, SLAs, Knowledge Management, and AI Workflows.

A distributed microservices architecture introduces substantial distributed system overhead: network serialization latency, independent database management, distributed transactions / saga coordination, complex deployment pipelines, and eventual consistency challenges across tightly coupled sales and ticketing entities.

## Decision
We adopt a **Modular Monolith** architecture packaged in a single repository and deployed as a cohesive service along with a dedicated background worker:

1. **Clear Module Boundaries**: The application is segmented into logical domain modules (`Identity`, `Tenancy`, `Leads`, `Accounts`, `Sales`, `Activities`, `Proposals`, `Support`, `Knowledge`, `AI`, `Reporting`, `Audit`).
2. **Encapsulation**: Modules must own their domain models and entities. No module application code is permitted to directly query or write to another module's database tables.
3. **Cross-Module Communication**:
   - Synchronous queries across module boundaries use explicitly published Application contracts and interfaces.
   - Asynchronous and decoupled workflows utilize in-process domain events and durable transactional integration events backed by an Outbox table.
4. **Automated Boundary Enforcement**: NetArchTest architecture tests in CI verify that module dependencies adhere strictly to architectural rules.

## Consequences

### Positive
- High developer velocity and simplified debugging: single solution build, synchronous testing with Testcontainers, and unified code navigation.
- Atomic local database transactions where needed (e.g., converting a lead into linked account, contact, and opportunity atomically in one database transaction).
- Zero network hops between module operations in standard request processing.
- Clean separation of concerns allows extracting any module (such as the AI engine or Worker) into an independent service in the future if specific scaling needs demand it.

### Negative / Trade-offs
- Requires discipline to prevent accidental coupling; enforced by architecture tests.
- Shared database connection pool and compute resources require proper indexing and query optimization.
