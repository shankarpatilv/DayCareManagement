using DayCareManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DayCareManagement.Infrastructure.Persistence;

public sealed class DayCareManagementDbContext(DbContextOptions<DayCareManagementDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Immunization> Immunizations => Set<Immunization>();
    public DbSet<StateRule> StateRules => Set<StateRule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DayCareManagementDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
