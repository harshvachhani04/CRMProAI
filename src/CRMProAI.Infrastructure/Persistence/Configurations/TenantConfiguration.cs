using CRMProAI.Domain.Modules.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMProAI.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Slug)
            .IsRequired()
            .HasMaxLength(63);

        builder.HasIndex(e => e.Slug)
            .IsUnique()
            .HasDatabaseName("ix_tenants_slug");

        builder.Property(e => e.LegalName)
            .HasMaxLength(250);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.OwnsOne(e => e.Settings, sb =>
        {
            sb.Property(s => s.DefaultTimezone)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValue("UTC");

            sb.Property(s => s.DefaultCurrency)
                .IsRequired()
                .HasMaxLength(10)
                .HasDefaultValue("USD");

            sb.Property(s => s.DefaultLocale)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("en-US");

            sb.Property(s => s.SupportEmail)
                .HasMaxLength(256);

            sb.Property(s => s.LogoUrl)
                .HasMaxLength(1000);

            sb.Property(s => s.Website)
                .HasMaxLength(500);

            sb.Property(s => s.Address)
                .HasMaxLength(500);

            sb.Property(s => s.Industry)
                .HasMaxLength(100);
        });

        builder.Property(e => e.CreatedAtUtc)
            .IsRequired();

        builder.Property(e => e.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);
    }
}
