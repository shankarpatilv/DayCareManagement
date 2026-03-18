using DayCareManagement.Infrastructure.Persistence;
using DayCareManagement.WebApi.Contracts;
using DayCareManagement.WebApi.Validation;
using Microsoft.EntityFrameworkCore;

namespace DayCareManagement.WebApi.Endpoints;

public static class StudentImmunizationEndpoints
{
	public static RouteGroupBuilder MapStudentAndImmunizationEndpoints(this RouteGroupBuilder featureGroup)
	{
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

		return featureGroup;
	}
}