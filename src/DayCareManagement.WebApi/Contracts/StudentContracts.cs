namespace DayCareManagement.WebApi.Contracts;

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