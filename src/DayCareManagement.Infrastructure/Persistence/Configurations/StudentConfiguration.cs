using DayCareManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DayCareManagement.Infrastructure.Persistence.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(student => student.StudentId);
        builder.Property(student => student.StudentId).ValueGeneratedNever();

        builder.Property(student => student.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(student => student.LastName).HasMaxLength(100).IsRequired();
        builder.Property(student => student.FatherName).HasMaxLength(150).IsRequired();
        builder.Property(student => student.MotherName).HasMaxLength(150).IsRequired();
        builder.Property(student => student.Address).HasMaxLength(300).IsRequired();
        builder.Property(student => student.PhoneNo).HasMaxLength(15).IsRequired();
        builder.Property(student => student.Email).HasMaxLength(320).IsRequired();
        builder.Property(student => student.Password).HasMaxLength(255).IsRequired();
        builder.Property(student => student.GPA).HasPrecision(3, 2).IsRequired();
        builder.Property(student => student.RegisterDate).IsRequired();

    }
}