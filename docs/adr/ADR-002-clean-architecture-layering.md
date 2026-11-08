# ADR-002: Architectural Layering — Clean Architecture with DDD-Informed Invariants

## Status
**Accepted** (2026-11-08)

## Context
A CRM domain contains rich business invariants:
- A qualified lead conversion must atomically link or create accounts and contacts without losing historical interactions.
- Opportunity stage transitions must obey a configured workflow graph.
- Won opportunities require outcome dates and verified final values; Lost opportunities require structured loss reasons.
- Sent proposals must remain strictly immutable.
- Support tickets require SLA response and resolution timing according to business calendar schedules.

Coupling these business rules to EF Core database concerns, ASP.NET Core controllers, or UI logic leads to fragmented rules, untestable logic, and regression risks.

## Decision
We enforce **Clean Architecture** with DDD-informed tactical patterns across 4 primary layers:

1. **`CRMProAI.Domain`**:
   - Pure C# models with zero framework dependencies (no EF Core, ASP.NET Core, or third-party packages).
   - Contains Entities, Value Objects (e.g., `Money`, `EmailAddress`), Domain Events, and Aggregate Roots.
   - Encapsulates state mutations via expressive methods (e.g., `opportunity.ChangeStage(...)`, `proposal.Approve(...)`), rejecting invalid transitions with domain exceptions.
2. **`CRMProAI.Application`**:
   - Depends solely on `CRMProAI.Domain`.
   - Organizes business use cases using CQRS-style commands and queries with explicit input/output DTOs.
   - Defines validation rules with FluentValidation before domain execution.
   - Declares outbound ports/interfaces for repositories, caching, email, and AI providers.
3. **`CRMProAI.Infrastructure`**:
   - Implements ports declared by Application.
   - Manages EF Core 10 DbContext, fluent entity configurations, migrations, PostgreSQL persistence, Redis cache adapters, and RabbitMQ message publishers.
4. **`CRMProAI.Api`**:
   - ASP.NET Core 10 Web API serving as the composition root and HTTP entry point.
   - Handles route dispatching, model binding, authentication middleware, and error translation into RFC 7807 ProblemDetails.
   - Contains no business calculations or state manipulation logic.

## Consequences

### Positive
- Domain rules are 100% testable in unit tests without database mocks or HTTP context.
- Database, messaging broker, and external AI provider details can be modified or substituted without touching core business rules.
- Clear separation of responsibilities accelerates code review and bug isolation.

### Negative / Trade-offs
- Mapping between Domain entities and Application DTOs requires explicit mapping code.
- Higher initial project scaffold overhead (multiple projects and interfaces), offset by long-term maintainability.
