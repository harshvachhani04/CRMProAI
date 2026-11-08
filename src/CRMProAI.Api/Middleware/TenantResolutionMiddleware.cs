using System.Security.Claims;
using CRMProAI.Application.Common;
using CRMProAI.Domain.Exceptions;
using CRMProAI.Domain.Modules.Tenancy;
using CRMProAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRMProAI.Api.Middleware;

/// <summary>
/// Resolves the tenant context for incoming HTTP requests based on trusted authentication claims,
/// host/subdomain mapping, or explicit client headers. Rejects cross-tenant override attempts.
/// </summary>
public class TenantResolutionMiddleware
{
    public const string HeaderTenantId = "X-Tenant-ID";
    public const string HeaderTenantSlug = "X-Tenant-Slug";
    public const string ClaimTenantId = "tenant_id";

    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(
        RequestDelegate next,
        ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ApplicationDbContext dbContext,
        ITenantSetter tenantSetter)
    {
        Tenant? resolvedTenant = null;

        // 1. Primary Source: Authenticated user claims (most trusted)
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var claimValue = context.User.FindFirst(ClaimTenantId)?.Value
                ?? context.User.FindFirst(ClaimTypes.GroupSid)?.Value;

            if (!string.IsNullOrWhiteSpace(claimValue) && Guid.TryParse(claimValue, out var claimTenantId))
            {
                // Security Check: Untrusted override detection
                // If caller also sent X-Tenant-ID header, it MUST match the authenticated claim
                if (context.Request.Headers.TryGetValue(HeaderTenantId, out var headerIdValues))
                {
                    var headerIdStr = headerIdValues.FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(headerIdStr) &&
                        Guid.TryParse(headerIdStr, out var headerTenantId) &&
                        headerTenantId != claimTenantId)
                    {
                        throw new ForbiddenException(
                            "Cross-tenant override attempt detected. Client-supplied tenant ID does not match authenticated user tenant claim.",
                            "UNTRUSTED_TENANT_OVERRIDE");
                    }
                }

                // If caller also sent X-Tenant-Slug header, verify it matches
                if (context.Request.Headers.TryGetValue(HeaderTenantSlug, out var headerSlugValues))
                {
                    var headerSlug = headerSlugValues.FirstOrDefault()?.Trim().ToLowerInvariant();
                    if (!string.IsNullOrWhiteSpace(headerSlug))
                    {
                        var tenantBySlug = await dbContext.Tenants
                            .IgnoreQueryFilters()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(t => t.Slug == headerSlug, context.RequestAborted);

                        if (tenantBySlug != null && tenantBySlug.Id != claimTenantId)
                        {
                            throw new ForbiddenException(
                                "Cross-tenant override attempt detected. Client-supplied tenant slug does not match authenticated user tenant claim.",
                                "UNTRUSTED_TENANT_OVERRIDE");
                        }
                    }
                }

                // Load tenant for authenticated claim
                resolvedTenant = await dbContext.Tenants
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == claimTenantId && !t.IsArchived, context.RequestAborted);

                if (resolvedTenant == null)
                {
                    throw new ForbiddenException(
                        "The tenant specified in user credentials was not found or has been decommissioned.",
                        "TENANT_NOT_FOUND");
                }
            }
        }

        // 2. Secondary Source: Explicit client header (for public/pre-auth routes like login/signup)
        if (resolvedTenant == null)
        {
            if (context.Request.Headers.TryGetValue(HeaderTenantId, out var headerIdVals) &&
                Guid.TryParse(headerIdVals.FirstOrDefault(), out var headerId))
            {
                resolvedTenant = await dbContext.Tenants
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == headerId && !t.IsArchived, context.RequestAborted);
            }
            else if (context.Request.Headers.TryGetValue(HeaderTenantSlug, out var headerSlugVals))
            {
                var slug = headerSlugVals.FirstOrDefault()?.Trim().ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(slug))
                {
                    resolvedTenant = await dbContext.Tenants
                        .IgnoreQueryFilters()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.Slug == slug && !t.IsArchived, context.RequestAborted);
                }
            }
        }

        // 3. Tertiary Source: Subdomain host mapping (e.g. northstar.crmpro.ai)
        if (resolvedTenant == null)
        {
            var host = context.Request.Host.Host;
            var subdomain = ExtractSubdomain(host);

            if (!string.IsNullOrWhiteSpace(subdomain))
            {
                resolvedTenant = await dbContext.Tenants
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Slug == subdomain && !t.IsArchived, context.RequestAborted);
            }
        }

        // Validate resolved tenant status
        if (resolvedTenant != null)
        {
            if (resolvedTenant.Status == TenantStatus.Suspended)
            {
                throw new ForbiddenException(
                    "This organization workspace is currently suspended. Please contact your organization administrator or support.",
                    "TENANT_SUSPENDED");
            }

            tenantSetter.SetTenant(resolvedTenant.Id, resolvedTenant.Slug);
            context.Items["TenantId"] = resolvedTenant.Id;
            context.Items["TenantSlug"] = resolvedTenant.Slug;
        }

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["TenantId"] = resolvedTenant?.Id ?? Guid.Empty,
            ["TenantSlug"] = resolvedTenant?.Slug ?? "none"
        }))
        {
            await _next(context);
        }
    }

    public static string? ExtractSubdomain(string host)
    {
        if (string.IsNullOrWhiteSpace(host) || host == "localhost" || host == "127.0.0.1")
        {
            return null;
        }

        var parts = host.Split('.');
        if (parts.Length >= 2)
        {
            var potentialSubdomain = parts[0].ToLowerInvariant();
            // Ignore system/infrastructure subdomains
            if (potentialSubdomain is "api" or "www" or "app" or "admin" or "portal" or "auth" or "mail")
            {
                return null;
            }

            return potentialSubdomain;
        }

        return null;
    }
}
