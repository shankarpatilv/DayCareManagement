using DayCareManagement.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DayCareManagement.Infrastructure.Persistence;

public sealed class DayCareManagementDbContextFactory : IDesignTimeDbContextFactory<DayCareManagementDbContext>
{
    public DayCareManagementDbContext CreateDbContext(string[] args)
    {
        var connectionString = ConnectionStringResolver.ResolveOrThrow();

        var optionsBuilder = new DbContextOptionsBuilder<DayCareManagementDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new DayCareManagementDbContext(optionsBuilder.Options);
    }
}
