using DayCareManagement.Application.Abstractions;
using DayCareManagement.Application.Renewals;
using DayCareManagement.Infrastructure.Configuration;
using DayCareManagement.Infrastructure.System;
using DayCareManagement.Infrastructure;
using DayCareManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
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

var featureGroup = app.MapGroup(string.Empty)
	.RequireAuthorization();

featureGroup.MapGet("/students", async (DayCareManagementDbContext dbContext, CancellationToken cancellationToken) =>
{
	var students = await dbContext.Students
		.AsNoTracking()
		.OrderBy(student => student.StudentId)
		.Select(student => new StudentResponse(
			student.StudentId,
			student.FirstName,
			student.LastName,
			student.RegisterDate,
			student.AgeMonths,
			student.FatherName,
			student.MotherName,
			student.Address,
			student.PhoneNo,
			student.GPA,
			student.Email))
		.ToListAsync(cancellationToken);

	return Results.Ok(students);
});

featureGroup.MapGet("/students/{studentId:int}", async (int studentId, DayCareManagementDbContext dbContext, CancellationToken cancellationToken) =>
{
	var student = await dbContext.Students
		.AsNoTracking()
		.Where(student => student.StudentId == studentId)
		.Select(student => new StudentResponse(
			student.StudentId,
			student.FirstName,
			student.LastName,
			student.RegisterDate,
			student.AgeMonths,
			student.FatherName,
			student.MotherName,
			student.Address,
			student.PhoneNo,
			student.GPA,
			student.Email))
		.SingleOrDefaultAsync(cancellationToken);

	return student is null ? Results.NotFound() : Results.Ok(student);
});

featureGroup.MapPost("/students", async (StudentWriteRequest request, DayCareManagementDbContext dbContext, CancellationToken cancellationToken) =>
{
	if (!ApiValidation.TryValidateStudentWriteRequest(request, out var validationError))
	{
		return Results.BadRequest(new { error = validationError });
	}

	var normalizedEmail = ApiValidation.NormalizeEmail(request.Email);
	if (normalizedEmail is null)
	{
		return Results.BadRequest(new { error = "Email is required." });
	}

	var idExists = await dbContext.Students
		.AsNoTracking()
		.AnyAsync(student => student.StudentId == request.StudentId, cancellationToken);
	if (idExists)
	{
		return Results.Conflict(new { error = $"Student with id '{request.StudentId}' already exists." });
	}

	var emailExists = await dbContext.Students
		.AsNoTracking()
		.AnyAsync(student => student.Email.ToLower() == normalizedEmail, cancellationToken);
	if (emailExists)
	{
		return Results.Conflict(new { error = "Student email already exists." });
	}

	var student = new DayCareManagement.Domain.Entities.Student
	{
		StudentId = request.StudentId,
		FirstName = request.FirstName.Trim(),
		LastName = request.LastName.Trim(),
		RegisterDate = request.RegisterDate,
		AgeMonths = request.AgeMonths,
		FatherName = request.FatherName.Trim(),
		MotherName = request.MotherName.Trim(),
		Address = request.Address.Trim(),
		PhoneNo = request.PhoneNo.Trim(),
		GPA = request.GPA,
		Email = normalizedEmail,
		Password = ApiValidation.NormalizePasswordForPersistence(request.Password)
	};

	dbContext.Students.Add(student);
	await dbContext.SaveChangesAsync(cancellationToken);

	var response = new StudentResponse(
		student.StudentId,
		student.FirstName,
		student.LastName,
		student.RegisterDate,
		student.AgeMonths,
		student.FatherName,
		student.MotherName,
		student.Address,
		student.PhoneNo,
		student.GPA,
		student.Email);

	return Results.Created($"/students/{student.StudentId}", response);
}).RequireAuthorization("TeacherOnly");

featureGroup.MapPut("/students/{studentId:int}", async (int studentId, StudentWriteRequest request, DayCareManagementDbContext dbContext, CancellationToken cancellationToken) =>
{
	if (studentId != request.StudentId)
	{
		return Results.BadRequest(new { error = "Route studentId must match request studentId." });
	}

	if (!ApiValidation.TryValidateStudentWriteRequest(request, out var validationError))
	{
		return Results.BadRequest(new { error = validationError });
	}

	var normalizedEmail = ApiValidation.NormalizeEmail(request.Email);
	if (normalizedEmail is null)
	{
		return Results.BadRequest(new { error = "Email is required." });
	}

	var student = await dbContext.Students
		.SingleOrDefaultAsync(entity => entity.StudentId == studentId, cancellationToken);
	if (student is null)
	{
		return Results.NotFound();
	}

	var emailTakenByAnotherStudent = await dbContext.Students
		.AsNoTracking()
		.AnyAsync(entity => entity.StudentId != studentId && entity.Email.ToLower() == normalizedEmail, cancellationToken);
	if (emailTakenByAnotherStudent)
	{
		return Results.Conflict(new { error = "Student email already exists." });
	}

	student.FirstName = request.FirstName.Trim();
	student.LastName = request.LastName.Trim();
	student.RegisterDate = request.RegisterDate;
	student.AgeMonths = request.AgeMonths;
	student.FatherName = request.FatherName.Trim();
	student.MotherName = request.MotherName.Trim();
	student.Address = request.Address.Trim();
	student.PhoneNo = request.PhoneNo.Trim();
	student.GPA = request.GPA;
	student.Email = normalizedEmail;
	student.Password = ApiValidation.NormalizePasswordForPersistence(request.Password);

	await dbContext.SaveChangesAsync(cancellationToken);
	return Results.NoContent();
}).RequireAuthorization("TeacherOnly");

featureGroup.MapGet("/students/{studentId:int}/immunizations", async (int studentId, DayCareManagementDbContext dbContext, CancellationToken cancellationToken) =>
{
	var studentExists = await dbContext.Students
		.AsNoTracking()
		.AnyAsync(student => student.StudentId == studentId, cancellationToken);
	if (!studentExists)
	{
		return Results.NotFound();
	}

	var immunizations = await dbContext.Immunizations
		.AsNoTracking()
		.Where(immunization => immunization.StudentId == studentId)
		.OrderBy(immunization => immunization.ImmunizationId)
		.ThenBy(immunization => immunization.ImmunizationDate)
		.Select(immunization => new ImmunizationResponse(
			immunization.StudentId,
			immunization.ImmunizationId,
			immunization.ImmunizationName,
			immunization.Duration,
			immunization.ImmunizationDate,
			immunization.Status))
		.ToListAsync(cancellationToken);

	return Results.Ok(immunizations);
});

featureGroup.MapPost("/students/{studentId:int}/immunizations", async (int studentId, ImmunizationCreateRequest request, DayCareManagementDbContext dbContext, CancellationToken cancellationToken) =>
{
	if (!ApiValidation.TryValidateImmunizationCreateRequest(request, out var validationError))
	{
		return Results.BadRequest(new { error = validationError });
	}

	var studentExists = await dbContext.Students
		.AsNoTracking()
		.AnyAsync(student => student.StudentId == studentId, cancellationToken);
	if (!studentExists)
	{
		return Results.NotFound();
	}

	var alreadyExists = await dbContext.Immunizations
		.AsNoTracking()
		.AnyAsync(immunization =>
			immunization.StudentId == studentId &&
			immunization.ImmunizationId == request.ImmunizationId &&
			immunization.ImmunizationDate == request.ImmunizationDate,
			cancellationToken);
	if (alreadyExists)
	{
		return Results.Conflict(new { error = "Immunization record already exists." });
	}

	var immunization = new DayCareManagement.Domain.Entities.Immunization
	{
		StudentId = studentId,
		ImmunizationId = request.ImmunizationId,
		ImmunizationName = request.ImmunizationName.Trim(),
		Duration = request.Duration.Trim(),
		ImmunizationDate = request.ImmunizationDate,
		Status = request.Status
	};

	dbContext.Immunizations.Add(immunization);
	await dbContext.SaveChangesAsync(cancellationToken);

	var response = new ImmunizationResponse(
		immunization.StudentId,
		immunization.ImmunizationId,
		immunization.ImmunizationName,
		immunization.Duration,
		immunization.ImmunizationDate,
		immunization.Status);

	return Results.Created($"/students/{studentId}/immunizations", response);
}).RequireAuthorization("TeacherOnly");

featureGroup.MapPut("/students/{studentId:int}/immunizations/{immunizationId:int}/{immunizationDate}", async (
	int studentId,
	int immunizationId,
	string immunizationDate,
	ImmunizationUpdateRequest request,
	DayCareManagementDbContext dbContext,
	CancellationToken cancellationToken) =>
{
	if (!ApiValidation.TryParseDateOnly(immunizationDate, out var parsedImmunizationDate))
	{
		return Results.BadRequest(new { error = "Route immunizationDate must be in yyyy-MM-dd format." });
	}

	if (!ApiValidation.TryValidateImmunizationUpdateRequest(request, out var validationError))
	{
		return Results.BadRequest(new { error = validationError });
	}

	var immunization = await dbContext.Immunizations
		.SingleOrDefaultAsync(entity =>
			entity.StudentId == studentId &&
			entity.ImmunizationId == immunizationId &&
			entity.ImmunizationDate == parsedImmunizationDate,
			cancellationToken);
	if (immunization is null)
	{
		return Results.NotFound();
	}

	immunization.ImmunizationName = request.ImmunizationName.Trim();
	immunization.Duration = request.Duration.Trim();
	immunization.Status = request.Status;

	await dbContext.SaveChangesAsync(cancellationToken);
	return Results.NoContent();
}).RequireAuthorization("TeacherOnly");

featureGroup.MapDelete("/students/{studentId:int}/immunizations/{immunizationId:int}/{immunizationDate}", async (
	int studentId,
	int immunizationId,
	string immunizationDate,
	DayCareManagementDbContext dbContext,
	CancellationToken cancellationToken) =>
{
	if (!ApiValidation.TryParseDateOnly(immunizationDate, out var parsedImmunizationDate))
	{
		return Results.BadRequest(new { error = "Route immunizationDate must be in yyyy-MM-dd format." });
	}

	var immunization = await dbContext.Immunizations
		.SingleOrDefaultAsync(entity =>
			entity.StudentId == studentId &&
			entity.ImmunizationId == immunizationId &&
			entity.ImmunizationDate == parsedImmunizationDate,
			cancellationToken);
	if (immunization is null)
	{
		return Results.NotFound();
	}

	dbContext.Immunizations.Remove(immunization);
	await dbContext.SaveChangesAsync(cancellationToken);
	return Results.NoContent();
}).RequireAuthorization("TeacherOnly");

featureGroup.MapGet("/renewals/due", async (
	DayCareManagementDbContext dbContext,
	IClock clock,
	CancellationToken cancellationToken) =>
{
	var asOfDate = DateOnly.FromDateTime(clock.UtcNow.Date);

	var students = await dbContext.Students
		.AsNoTracking()
		.OrderBy(student => student.StudentId)
		.Select(student => new
		{
			student.StudentId,
			student.FirstName,
			student.LastName,
			student.RegisterDate,
			student.AgeMonths
		})
		.ToListAsync(cancellationToken);

	var dueStudents = students
		.Where(student => RenewalPolicy.IsDue(student.RegisterDate, asOfDate))
		.Select(student => new RenewalDueResponse(
			student.StudentId,
			student.FirstName,
			student.LastName,
			student.RegisterDate,
			student.AgeMonths,
			asOfDate))
		.ToList();

	return Results.Ok(dueStudents);
});

featureGroup.MapPost("/renewals/{studentId:int}", async (
	int studentId,
	DayCareManagementDbContext dbContext,
	IClock clock,
	CancellationToken cancellationToken) =>
{
	var student = await dbContext.Students
		.SingleOrDefaultAsync(entity => entity.StudentId == studentId, cancellationToken);
	if (student is null)
	{
		return Results.NotFound();
	}

	student.RegisterDate = DateOnly.FromDateTime(clock.UtcNow.Date);
	await dbContext.SaveChangesAsync(cancellationToken);

	return Results.Ok(new RenewalAppliedResponse(student.StudentId, student.RegisterDate));
}).RequireAuthorization("TeacherOnly");

featureGroup.MapGet("/state-rules", async (int? ageMonths, DayCareManagementDbContext dbContext, CancellationToken cancellationToken) =>
{
	if (ageMonths is < 0)
	{
		return Results.BadRequest(new { error = "ageMonths must be greater than or equal to zero." });
	}

	var query = dbContext.StateRules.AsNoTracking();
	if (ageMonths is not null)
	{
		query = query.Where(rule => rule.AgeMonths == ageMonths.Value);
	}

	var rules = await query
		.OrderBy(rule => rule.AgeMonths)
		.ThenBy(rule => rule.VaccineName)
		.ThenBy(rule => rule.DoseRequirement)
		.Select(rule => new StateRuleResponse(rule.VaccineName, rule.DoseRequirement, rule.AgeMonths))
		.ToListAsync(cancellationToken);

	return Results.Ok(rules);
});

app.Run();

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(string Token, string Role, int SubjectId);

public sealed record StudentResponse(
	int StudentId,
	string FirstName,
	string LastName,
	DateOnly RegisterDate,
	int AgeMonths,
	string FatherName,
	string MotherName,
	string Address,
	string PhoneNo,
	decimal GPA,
	string Email);

public sealed record StudentWriteRequest(
	int StudentId,
	string FirstName,
	string LastName,
	DateOnly RegisterDate,
	int AgeMonths,
	string FatherName,
	string MotherName,
	string Address,
	string PhoneNo,
	decimal GPA,
	string Email,
	string Password);

public sealed record ImmunizationResponse(
	int StudentId,
	int ImmunizationId,
	string ImmunizationName,
	string Duration,
	DateOnly ImmunizationDate,
	bool Status);

public sealed record ImmunizationCreateRequest(
	int ImmunizationId,
	string ImmunizationName,
	string Duration,
	DateOnly ImmunizationDate,
	bool Status);

public sealed record ImmunizationUpdateRequest(
	string ImmunizationName,
	string Duration,
	bool Status);

public sealed record RenewalDueResponse(
	int StudentId,
	string FirstName,
	string LastName,
	DateOnly RegisterDate,
	int AgeMonths,
	DateOnly AsOfDate);

public sealed record RenewalAppliedResponse(int StudentId, DateOnly RegisterDate);

public sealed record StateRuleResponse(string VaccineName, string DoseRequirement, int AgeMonths);

public static class ApiValidation
{
	private static readonly Regex Sha256HexRegex = new("^[0-9a-fA-F]{64}$", RegexOptions.Compiled);

	public static bool TryValidateStudentWriteRequest(StudentWriteRequest request, out string? validationError)
	{
		if (request.StudentId <= 0)
		{
			validationError = "StudentId must be greater than zero.";
			return false;
		}

		if (request.AgeMonths < 0)
		{
			validationError = "AgeMonths must be greater than or equal to zero.";
			return false;
		}

		if (request.GPA < 0m || request.GPA > 4m)
		{
			validationError = "GPA must be between 0.00 and 4.00.";
			return false;
		}

		if (string.IsNullOrWhiteSpace(request.FirstName) ||
			string.IsNullOrWhiteSpace(request.LastName) ||
			string.IsNullOrWhiteSpace(request.FatherName) ||
			string.IsNullOrWhiteSpace(request.MotherName) ||
			string.IsNullOrWhiteSpace(request.Address) ||
			string.IsNullOrWhiteSpace(request.PhoneNo) ||
			string.IsNullOrWhiteSpace(request.Email) ||
			string.IsNullOrWhiteSpace(request.Password))
		{
			validationError = "Student firstName, lastName, fatherName, motherName, address, phoneNo, email, and password are required.";
			return false;
		}

		validationError = null;
		return true;
	}

	public static bool TryValidateImmunizationCreateRequest(ImmunizationCreateRequest request, out string? validationError)
	{
		if (request.ImmunizationId <= 0)
		{
			validationError = "ImmunizationId must be greater than zero.";
			return false;
		}

		if (string.IsNullOrWhiteSpace(request.ImmunizationName) || string.IsNullOrWhiteSpace(request.Duration))
		{
			validationError = "ImmunizationName and Duration are required.";
			return false;
		}

		validationError = null;
		return true;
	}

	public static bool TryValidateImmunizationUpdateRequest(ImmunizationUpdateRequest request, out string? validationError)
	{
		if (string.IsNullOrWhiteSpace(request.ImmunizationName) || string.IsNullOrWhiteSpace(request.Duration))
		{
			validationError = "ImmunizationName and Duration are required.";
			return false;
		}

		validationError = null;
		return true;
	}

	public static string? NormalizeEmail(string email)
	{
		return string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
	}

	public static string NormalizePasswordForPersistence(string password)
	{
		if (string.IsNullOrWhiteSpace(password))
		{
			return password;
		}

		var candidate = password.Trim();
		if (IsSupportedStoredHash(candidate))
		{
			return candidate;
		}

		using var sha256 = SHA256.Create();
		var bytes = Encoding.UTF8.GetBytes(password);
		var hash = sha256.ComputeHash(bytes);
		return Convert.ToHexString(hash).ToLowerInvariant();
	}

	public static bool IsSupportedStoredHash(string password)
	{
		return IsSha256Hex(password) || IsBcryptHash(password);
	}

	public static bool IsSha256Hex(string value)
	{
		return !string.IsNullOrWhiteSpace(value) && Sha256HexRegex.IsMatch(value);
	}

	public static bool IsBcryptHash(string value)
	{
		return !string.IsNullOrWhiteSpace(value) && value.StartsWith("$2", StringComparison.Ordinal);
	}

	public static bool TryParseDateOnly(string value, out DateOnly parsedDate)
	{
		return DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);
	}
}
