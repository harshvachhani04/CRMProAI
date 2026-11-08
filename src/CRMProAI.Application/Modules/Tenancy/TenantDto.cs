namespace CRMProAI.Application.Modules.Tenancy;

public record TenantSettingsDto(
    string DefaultTimezone,
    string DefaultCurrency,
    string DefaultLocale,
    string? SupportEmail,
    string? LogoUrl,
    string? Website,
    string? Address,
    string? Industry);

public record TenantDto(
    Guid Id,
    string Name,
    string Slug,
    string? LegalName,
    string Status,
    TenantSettingsDto Settings,
    DateTimeOffset CreatedAtUtc);
