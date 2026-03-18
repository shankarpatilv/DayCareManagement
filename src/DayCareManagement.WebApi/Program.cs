using DayCareManagement.Application.Abstractions;
using DayCareManagement.Infrastructure.Configuration;
using DayCareManagement.Infrastructure.System;
using DayCareManagement.Infrastructure;
using DayCareManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
	?? throw new InvalidOperationException("JWT settings are missing.");

var jwtSigningKeyValidationError = JwtSigningKeyPolicy.GetValidationError(jwtOptions.SigningKey);
if (jwtSigningKeyValidationError is not null)
{
	throw new InvalidOperationException($"{jwtSigningKeyValidationError} Configure a secure key via environment variables or user-secrets before starting the API.");
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddDayCarePersistence(builder.Configuration);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("StudentOnly", policy => policy.RequireRole(AuthRole.Student.ToString()));
	options.AddPolicy("TeacherOnly", policy => policy.RequireRole(AuthRole.Teacher.ToString()));
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.MapPost("/auth/login", async (
	LoginRequest request,
	DayCareManagementDbContext dbContext,
	IAuthService authService,
	CancellationToken cancellationToken) =>
{
	if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
	{
		return Results.BadRequest(new { error = "Email and password are required." });
	}

	var normalizedEmail = request.Email.Trim().ToLowerInvariant();

	var student = await dbContext.Students
		.AsNoTracking()
		.Where(student => student.Email.ToLower() == normalizedEmail)
		.Select(student => new AuthCredentialRecord(student.StudentId, student.Password))
		.SingleOrDefaultAsync(cancellationToken);

	var teacher = await dbContext.Teachers
		.AsNoTracking()
		.Where(teacher => teacher.Email.ToLower() == normalizedEmail)
		.Select(teacher => new AuthCredentialRecord(teacher.TeacherId, teacher.Password))
		.SingleOrDefaultAsync(cancellationToken);

	var resolution = authService.ResolveUser(student, teacher);
	if (resolution.Status != LoginResolutionStatus.Resolved ||
		resolution.SubjectId is null ||
		resolution.Role is null ||
		string.IsNullOrWhiteSpace(resolution.PasswordHash))
	{
		return Results.Unauthorized();
	}

	var verificationStatus = authService.VerifyPassword(request.Password, resolution.PasswordHash);
	if (verificationStatus != PasswordVerificationStatus.Verified)
	{
		return Results.Unauthorized();
	}

	var token = authService.IssueToken(resolution.SubjectId.Value, resolution.Role.Value, normalizedEmail);
	return Results.Ok(new LoginResponse(token, resolution.Role.Value.ToString(), resolution.SubjectId.Value));
});

app.MapGet("/auth/me", [Authorize] (HttpContext context) =>
{
	var subject = context.User.FindFirst("sub")?.Value
		?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
	var role = context.User.FindFirst("role")?.Value
		?? context.User.FindFirst(ClaimTypes.Role)?.Value;
	var email = context.User.FindFirst("email")?.Value
		?? context.User.FindFirst(ClaimTypes.Email)?.Value;

	return Results.Ok(new { subject, role, email });
});

app.MapGet("/auth/teacher-only", [Authorize(Policy = "TeacherOnly")] () =>
	Results.Ok(new { message = "Teacher access granted." }));

app.Run();

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(string Token, string Role, int SubjectId);
