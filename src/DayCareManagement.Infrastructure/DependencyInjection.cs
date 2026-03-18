using DayCareManagement.Infrastructure.Configuration;
using DayCareManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DayCareManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDayCarePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = ConnectionStringResolver.ResolveOrThrow(
            configuration.GetConnectionString("DefaultConnection"));

        services.AddDbContext<DayCareManagementDbContext>(options => options.UseNpgsql(connectionString));

        return services;
    }
}