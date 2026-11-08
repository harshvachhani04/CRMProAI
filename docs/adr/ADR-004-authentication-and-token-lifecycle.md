# ADR-004: Identity & Access — ASP.NET Core Identity with Rotating Refresh Tokens

## Status
**Accepted** (2026-11-08)

## Context
CRMPro AI serves two distinct user constituencies:
1. **Internal Organization Users**: Sales executives, account managers, support agents, managers, and tenant administrators requiring role-based access control (RBAC), team scoping, and permission checks.
2. **Customer Portal Users**: External clients who must authenticate to view only their organization's support tickets and public replies.

The system requires secure authentication that works seamlessly across Single Page Applications (Angular 20+), supports instantaneous session revocation on deactivation or password reset, and never creates custom password hashing cryptography.

## Decision
We leverage **ASP.NET Core Identity** extended with custom multi-tenant session and token controls:

1. **Identity Integration**:
   - `ApplicationUser` extends `IdentityUser<Guid>` and enforces a non-nullable `TenantId`.
   - Identity password hasher (PBKDF2 with HMAC-SHA256) is utilized without custom crypto.
   - User types (`Internal` vs `Portal`) are strictly segregated. Portal users are explicitly bound to their specific `AccountId` and `ContactId`.
2. **Short-Lived Access Tokens**:
   - JSON Web Tokens (JWT) signed with HMAC-SHA256 / RSA using secure environment keys.
   - 15-minute token expiry minimizes the vulnerability window if an access token is intercepted.
   - Contains claims: `sub` (UserId), `tenant_id`, `role`, `user_type`, and permission keys.
3. **Rotating Refresh Tokens with Session Family Tracking**:
   - Refresh tokens are cryptographically strong random strings stored as SHA-256 hashes in the database with expiry (e.g., 7 days).
   - Upon each refresh, the old token is revoked, and a new refresh token is issued (token rotation).
   - If an already revoked refresh token is presented, the entire session family is immediately revoked as a suspected compromise.
4. **Immediate Revocation**:
   - Logout, user deactivation, or password reset invalidates all active refresh tokens for the user in the database.

## Consequences

### Positive
- Stateless API authorization for optimal throughput.
- Full revocation capability via refresh token database validation.
- Distinct isolation between internal staff and external customer portal accounts.

### Negative / Trade-offs
- Slight latency penalty on token refresh (every 15 minutes) to perform database validation and rotation.
- Secure HTTP-only cookies or protected client storage required for refresh tokens.
