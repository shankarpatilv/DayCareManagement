using DayCareManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DayCareManagement.Infrastructure.Persistence.Configurations;

public sealed class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("Teachers");

        builder.HasKey(teacher => teacher.TeacherId);
        builder.Property(teacher => teacher.TeacherId).ValueGeneratedNever();

        builder.Property(teacher => teacher.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(teacher => teacher.LastName).HasMaxLength(100).IsRequired();
        builder.Property(teacher => teacher.Email).HasMaxLength(320).IsRequired();
        builder.Property(teacher => teacher.Password).HasMaxLength(255).IsRequired();
        builder.Property(teacher => teacher.ClassRoomName).HasMaxLength(100);
        builder.Property(teacher => teacher.RegisterDate).IsRequired();

    }
}