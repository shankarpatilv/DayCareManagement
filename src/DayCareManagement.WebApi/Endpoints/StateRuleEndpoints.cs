using DayCareManagement.Infrastructure.Persistence;
using DayCareManagement.WebApi.Contracts;
using Microsoft.EntityFrameworkCore;

namespace DayCareManagement.WebApi.Endpoints;

public static class StateRuleEndpoints
{
	public static RouteGroupBuilder MapStateRuleEndpoints(this RouteGroupBuilder featureGroup)
	{
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

		return featureGroup;
	}
}