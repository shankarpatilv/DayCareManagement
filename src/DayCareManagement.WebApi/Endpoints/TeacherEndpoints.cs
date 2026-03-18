using DayCareManagement.Infrastructure.Persistence;
using DayCareManagement.WebApi.Contracts;
using DayCareManagement.WebApi.Validation;
using Microsoft.EntityFrameworkCore;

namespace DayCareManagement.WebApi.Endpoints;

public static class TeacherEndpoints
{
	public static RouteGroupBuilder MapTeacherEndpoints(this RouteGroupBuilder featureGroup)
	{
		featureGroup.MapGet("/teachers", async (DayCareManagementDbContext dbContext, CancellationToken cancellationToken) =>
		{
			var teachers = await dbContext.Teachers
				.AsNoTracking()
				.OrderBy(teacher => teacher.TeacherId)
				.Select(teacher => new TeacherResponse(
					teacher.TeacherId,
					teacher.FirstName,
					teacher.LastName,
					teacher.RegisterDate,
					teacher.IsAssigned,
					teacher.ClassRoomName,
					teacher.Email,
					teacher.Credits))
				.ToListAsync(cancellationToken);

			return Results.Ok(teachers);
		});

		featureGroup.MapGet("/teachers/{teacherId:int}", async (int teacherId, DayCareManagementDbContext dbContext, CancellationToken cancellationToken) =>
		{
			var teacher = await dbContext.Teachers
				.AsNoTracking()
				.Where(entity => entity.TeacherId == teacherId)
				.Select(entity => new TeacherResponse(
					entity.TeacherId,
					entity.FirstName,
					entity.LastName,
					entity.RegisterDate,
					entity.IsAssigned,
					entity.ClassRoomName,
					entity.Email,
					entity.Credits))
				.SingleOrDefaultAsync(cancellationToken);

			return teacher is null ? Results.NotFound() : Results.Ok(teacher);
		});

		featureGroup.MapPost("/teachers", async (TeacherWriteRequest request, DayCareManagementDbContext dbContext, CancellationToken cancellationToken) =>
		{
			if (!ApiValidation.TryValidateTeacherWriteRequest(request, out var validationError))
			{
				return Results.BadRequest(new { error = validationError });
			}

			var normalizedEmail = ApiValidation.NormalizeEmail(request.Email);
			if (normalizedEmail is null)
			{
				return Results.BadRequest(new { error = "Email is required." });
			}

			var idExists = await dbContext.Teachers
				.AsNoTracking()
				.AnyAsync(entity => entity.TeacherId == request.TeacherId, cancellationToken);
			if (idExists)
			{
				return Results.Conflict(new { error = $"Teacher with id '{request.TeacherId}' already exists." });
			}

			var emailExists = await dbContext.Teachers
				.AsNoTracking()
				.AnyAsync(entity => entity.Email.ToLower() == normalizedEmail, cancellationToken);
			if (emailExists)
			{
				return Results.Conflict(new { error = "Teacher email already exists." });
			}

			var teacher = new DayCareManagement.Domain.Entities.Teacher
			{
				TeacherId = request.TeacherId,
				FirstName = request.FirstName.Trim(),
				LastName = request.LastName.Trim(),
				RegisterDate = request.RegisterDate,
				IsAssigned = request.IsAssigned,
				ClassRoomName = string.IsNullOrWhiteSpace(request.ClassRoomName) ? null : request.ClassRoomName.Trim(),
				Email = normalizedEmail,
				Password = ApiValidation.NormalizePasswordForPersistence(request.Password),
				Credits = request.Credits
			};

			dbContext.Teachers.Add(teacher);
			await dbContext.SaveChangesAsync(cancellationToken);

			var response = new TeacherResponse(
				teacher.TeacherId,
				teacher.FirstName,
				teacher.LastName,
				teacher.RegisterDate,
				teacher.IsAssigned,
				teacher.ClassRoomName,
				teacher.Email,
				teacher.Credits);

			return Results.Created($"/teachers/{teacher.TeacherId}", response);
		}).RequireAuthorization("TeacherOnly");

		featureGroup.MapPut("/teachers/{teacherId:int}", async (int teacherId, TeacherWriteRequest request, DayCareManagementDbContext dbContext, CancellationToken cancellationToken) =>
		{
			if (teacherId != request.TeacherId)
			{
				return Results.BadRequest(new { error = "Route teacherId must match request teacherId." });
			}

			if (!ApiValidation.TryValidateTeacherWriteRequest(request, out var validationError))
			{
				return Results.BadRequest(new { error = validationError });
			}

			var normalizedEmail = ApiValidation.NormalizeEmail(request.Email);
			if (normalizedEmail is null)
			{
				return Results.BadRequest(new { error = "Email is required." });
			}

			var teacher = await dbContext.Teachers
				.SingleOrDefaultAsync(entity => entity.TeacherId == teacherId, cancellationToken);
			if (teacher is null)
			{
				return Results.NotFound();
			}

			var emailTakenByAnotherTeacher = await dbContext.Teachers
				.AsNoTracking()
				.AnyAsync(entity => entity.TeacherId != teacherId && entity.Email.ToLower() == normalizedEmail, cancellationToken);
			if (emailTakenByAnotherTeacher)
			{
				return Results.Conflict(new { error = "Teacher email already exists." });
			}

			teacher.FirstName = request.FirstName.Trim();
			teacher.LastName = request.LastName.Trim();
			teacher.RegisterDate = request.RegisterDate;
			teacher.IsAssigned = request.IsAssigned;
			teacher.ClassRoomName = string.IsNullOrWhiteSpace(request.ClassRoomName) ? null : request.ClassRoomName.Trim();
			teacher.Email = normalizedEmail;
			teacher.Password = ApiValidation.NormalizePasswordForPersistence(request.Password);
			teacher.Credits = request.Credits;

			await dbContext.SaveChangesAsync(cancellationToken);
			return Results.NoContent();
		}).RequireAuthorization("TeacherOnly");

		featureGroup.MapDelete("/teachers/{teacherId:int}", async (int teacherId, DayCareManagementDbContext dbContext, CancellationToken cancellationToken) =>
		{
			var teacher = await dbContext.Teachers
				.SingleOrDefaultAsync(entity => entity.TeacherId == teacherId, cancellationToken);
			if (teacher is null)
			{
				return Results.NotFound();
			}

			dbContext.Teachers.Remove(teacher);
			await dbContext.SaveChangesAsync(cancellationToken);
			return Results.NoContent();
		}).RequireAuthorization("TeacherOnly");

		return featureGroup;
	}
}
