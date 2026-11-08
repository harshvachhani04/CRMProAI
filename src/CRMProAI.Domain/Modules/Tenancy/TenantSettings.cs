using CRMProAI.Domain.Common;

namespace CRMProAI.Domain.Modules.Tenancy;

public class TenantSettings : ValueObject
{
    public string DefaultTimezone { get; private set; } = "UTC";
    public string DefaultCurrency { get; private set; } = "USD";
    public string DefaultLocale { get; private set; } = "en-US";
    public string? SupportEmail { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? Website { get; private set; }
    public string? Address { get; private set; }
    public string? Industry { get; private set; }

    private TenantSettings() { }

    public TenantSettings(
        string defaultTimezone,
        string defaultCurrency,
        string defaultLocale,
        string? supportEmail = null,
        string? logoUrl = null,
        string? website = null,
        string? address = null,
        string? industry = null)
    {
        DefaultTimezone = string.IsNullOrWhiteSpace(defaultTimezone) ? "UTC" : defaultTimezone.Trim();
        DefaultCurrency = string.IsNullOrWhiteSpace(defaultCurrency) ? "USD" : defaultCurrency.Trim().ToUpperInvariant();
        DefaultLocale = string.IsNullOrWhiteSpace(defaultLocale) ? "en-US" : defaultLocale.Trim();
        SupportEmail = string.IsNullOrWhiteSpace(supportEmail) ? null : supportEmail.Trim().ToLowerInvariant();
        LogoUrl = string.IsNullOrWhiteSpace(logoUrl) ? null : logoUrl.Trim();
        Website = string.IsNullOrWhiteSpace(website) ? null : website.Trim();
        Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
        Industry = string.IsNullOrWhiteSpace(industry) ? null : industry.Trim();
    }

    public static TenantSettings Default() => new("UTC", "USD", "en-US");

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return DefaultTimezone;
        yield return DefaultCurrency;
        yield return DefaultLocale;
        yield return SupportEmail;
        yield return LogoUrl;
        yield return Website;
        yield return Address;
        yield return Industry;
    }
}
