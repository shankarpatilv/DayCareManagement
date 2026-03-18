using System.Text;
using DayCareManagement.Application.Abstractions;
using DayCareManagement.Infrastructure;
using DayCareManagement.Infrastructure.Configuration;
using DayCareManagement.Infrastructure.System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace DayCareManagement.WebApi.Configuration;

public static class WebApiServiceCollectionExtensions
{
	public static IServiceCollection AddWebApiServices(this IServiceCollection services, IConfiguration configuration)
	{
		var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
			?? throw new InvalidOperationException("JWT settings are missing.");

		var jwtSigningKeyValidationError = JwtSigningKeyPolicy.GetValidationError(jwtOptions.SigningKey);
		if (jwtSigningKeyValidationError is not null)
		{
			throw new InvalidOperationException($"{jwtSigningKeyValidationError} Configure a secure key via environment variables or user-secrets before starting the API.");
		}

		services.AddEndpointsApiExplorer();
		services.AddSingleton<IClock, SystemClock>();
		services.AddDayCarePersistence(configuration);

		services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateIssuerSigningKey = true,
					ValidateLifetime = true,
					ValidIssuer = jwtOptions.Issuer,
					ValidAudience = jwtOptions.Audience,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey))
				};
			});

		services.AddAuthorization(options =>
		{
			options.AddPolicy("StudentOnly", policy => policy.RequireRole(AuthRole.Student.ToString()));
			options.AddPolicy("TeacherOnly", policy => policy.RequireRole(AuthRole.Teacher.ToString()));
		});

		return services;
	}
}