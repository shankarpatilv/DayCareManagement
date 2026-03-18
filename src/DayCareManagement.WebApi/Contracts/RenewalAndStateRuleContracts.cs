namespace DayCareManagement.WebApi.Contracts;

public sealed record RenewalDueResponse(
	int StudentId,
	string FirstName,
	string LastName,
	DateOnly RegisterDate,
	int AgeMonths,
	DateOnly AsOfDate);

public sealed record RenewalAppliedResponse(int StudentId, DateOnly RegisterDate);

public sealed record StateRuleResponse(string VaccineName, string DoseRequirement, int AgeMonths);