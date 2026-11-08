using CRMProAI.Domain.Modules.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace CRMProAI.Infrastructure.Persistence.Seed;

public static class TenantSeed
{
    public static readonly Guid DemoTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public const string DemoTenantSlug = "northstar";
    public const string DemoTenantName = "Northstar Digital";
    public const string DemoTenantLegalName = "Northstar Digital Technologies Inc.";

    public static async Task SeedDemoTenantAsync(ApplicationDbContext context, CancellationToken ct = default)
    {
        var existing = await context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == DemoTenantId || t.Slug == DemoTenantSlug, ct);

        if (existing == null)
        {
            var settings = new TenantSettings(
                defaultTimezone: "America/New_York",
                defaultCurrency: "USD",
                defaultLocale: "en-US",
                supportEmail: "support@northstardigital.io",
                logoUrl: "https://assets.crmpro.ai/demo/northstar-logo.png",
                website: "https://northstardigital.io",
                address: "100 Innovation Way, Suite 400, Boston, MA 02110",
                industry: "Custom Software & Cloud Engineering");

            var tenant = Tenant.Create(
                DemoTenantId,
                DemoTenantName,
                DemoTenantSlug,
                DemoTenantLegalName,
                settings);

            context.Tenants.Add(tenant);
            await context.SaveChangesAsync(ct);
        }
    }
}
