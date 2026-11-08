# ADR-007: API Conventions — REST /api/v1, DTO Boundaries & RFC 7807 ProblemDetails

## Status
**Accepted** (2026-11-08)

## Context
A consistent, well-documented API is essential for the Angular frontend, customer portal, background workers, and future third-party integrations. Without rigid conventions, endpoints suffer from inconsistent naming, leaking internal database models, uncontrolled payload sizes, and varied error formats that complicate client error handling.

## Decision
We enforce standardized API design, versioning, and error-handling standards:

1. **Routing and Versioning**:
   - URL path versioning: `/api/v1/{resource}`.
   - Resource naming uses plural lowercase nouns (e.g., `/api/v1/leads`, `/api/v1/opportunities`, `/api/v1/tickets`).
   - RPC-style commands utilize sub-actions on resource identifiers (e.g., `POST /api/v1/opportunities/{id}/stage`, `POST /api/v1/leads/{id}/convert`).
2. **Strict DTO Boundaries**:
   - EF Core entities are **never** returned directly by controllers or accepted as request parameters.
   - Every endpoint binds to an explicit request DTO and returns a strongly typed response DTO.
3. **Standardized Error Responses (RFC 7807 ProblemDetails)**:
   - All client and server errors return `application/problem+json` formatted according to RFC 7807:
     ```json
     {
       "type": "https://crmpro.ai/errors/validation-failed",
       "title": "Validation Failed",
       "status": 400,
       "detail": "One or more validation errors occurred.",
       "instance": "/api/v1/leads",
       "code": "VALIDATION_ERROR",
       "traceId": "00-123456789abcdef-00",
       "errors": {
         "email": ["Invalid email address format."]
       }
     }
     ```
   - Unhandled exceptions are intercepted by a global exception middleware, sanitized, logged with correlation IDs, and returned as a generic 500 error without exposing stack traces.
4. **Pagination, Sorting & Filtering**:
   - List endpoints accept query parameters: `page` (default 1), `pageSize` (default 25, capped at 100), `sortBy`, and `sortDirection` (`asc`/`desc`).
   - Returns a standardized wrapper: `PagedList<T> { items, totalCount, page, pageSize, totalPages, hasNextPage, hasPreviousPage }`.

## Consequences

### Positive
- Consistent API behavior allows the Angular frontend to implement a centralized HTTP interceptor for error toasts and validation mappings.
- Clean contract documentation generated automatically via Swashbuckle/OpenAPI.
- Prevents mass-assignment security vulnerabilities by using isolated request DTOs.

### Negative / Trade-offs
- Writing separate request and response DTOs for every endpoint requires additional boilerplate, which ensures long-term contract stability.
