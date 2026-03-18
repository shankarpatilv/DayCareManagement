using System.Security.Claims;
using DayCareManagement.Application.Abstractions;
using DayCareManagement.Infrastructure.Persistence;
using DayCareManagement.WebApi.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace DayCareManagement.WebApi.Endpoints;

public static class AuthEndpoints
{
	public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
	{
		endpoints.MapPost("/auth/login", async (
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

		endpoints.MapGet("/auth/me", [Authorize] (HttpContext context) =>
		{
			var subject = context.User.FindFirst("sub")?.Value
				?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var role = context.User.FindFirst("role")?.Value
				?? context.User.FindFirst(ClaimTypes.Role)?.Value;
			var email = context.User.FindFirst("email")?.Value
				?? context.User.FindFirst(ClaimTypes.Email)?.Value;

			return Results.Ok(new { subject, role, email });
		});

		endpoints.MapGet("/auth/teacher-only", [Authorize(Policy = "TeacherOnly")] () =>
			Results.Ok(new { message = "Teacher access granted." }));

		return endpoints;
	}
}