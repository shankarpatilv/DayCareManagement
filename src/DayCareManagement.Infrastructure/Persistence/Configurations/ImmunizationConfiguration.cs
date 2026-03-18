using DayCareManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DayCareManagement.Infrastructure.Persistence.Configurations;

public sealed class ImmunizationConfiguration : IEntityTypeConfiguration<Immunization>
{
    public void Configure(EntityTypeBuilder<Immunization> builder)
    {
        builder.ToTable("Immunizations");

        builder.HasKey(immunization => new
        {
            immunization.StudentId,
            immunization.ImmunizationId,
            immunization.ImmunizationDate
        });

        builder.Property(immunization => immunization.StudentId).IsRequired();
        builder.Property(immunization => immunization.ImmunizationId).IsRequired();
        builder.Property(immunization => immunization.ImmunizationName).IsRequired();
        builder.Property(immunization => immunization.Duration).IsRequired();
        builder.Property(immunization => immunization.ImmunizationDate).IsRequired();
        builder.Property(immunization => immunization.Status).IsRequired();

        builder.HasOne(immunization => immunization.Student)
            .WithMany(student => student.Immunizations)
            .HasForeignKey(immunization => immunization.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(immunization => new
        {
            immunization.StudentId,
            immunization.ImmunizationId,
            immunization.ImmunizationDate
        }).IsUnique();
    }
}
