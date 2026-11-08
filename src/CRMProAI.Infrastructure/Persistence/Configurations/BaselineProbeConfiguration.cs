using CRMProAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMProAI.Infrastructure.Persistence.Configurations;

public class BaselineProbeConfiguration : IEntityTypeConfiguration<BaselineProbe>
{
    public void Configure(EntityTypeBuilder<BaselineProbe> builder)
    {
        builder.ToTable("baseline_probes");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.CreatedAtUtc)
            .IsRequired();

        builder.Property(e => e.IsArchived)
            .IsRequired()
            .HasDefaultValue(false);
    }
}
