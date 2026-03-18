using DayCareManagement.Infrastructure.Configuration;
using DayCareManagement.Infrastructure.Persistence;
using DayCareManagement.Infrastructure.System;
using DayCareManagement.Application.Abstractions;
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
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IAuthService, JwtAuthService>();

        return services;
    }
}