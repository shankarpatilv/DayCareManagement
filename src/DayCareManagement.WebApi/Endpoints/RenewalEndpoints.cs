using DayCareManagement.Application.Abstractions;
using DayCareManagement.Application.Renewals;
using DayCareManagement.Infrastructure.Persistence;
using DayCareManagement.WebApi.Contracts;
using Microsoft.EntityFrameworkCore;

namespace DayCareManagement.WebApi.Endpoints;

public static class RenewalEndpoints
{
	public static RouteGroupBuilder MapRenewalEndpoints(this RouteGroupBuilder featureGroup)
	{
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

		return featureGroup;
	}
}