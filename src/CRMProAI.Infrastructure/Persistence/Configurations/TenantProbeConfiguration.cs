using CRMProAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMProAI.Infrastructure.Persistence.Configurations;

public class TenantProbeConfiguration : IEntityTypeConfiguration<TenantProbe>
{
    public void Configure(EntityTypeBuilder<TenantProbe> builder)
    {
        builder.ToTable("tenant_probes");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .IsRequired();

        builder.HasIndex(e => e.TenantId);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Details)
            .HasMaxLength(1000);

        builder.Property(e => e.CreatedAtUtc)
            .IsRequired();

        builder.Property(e => e.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);
    }
}
