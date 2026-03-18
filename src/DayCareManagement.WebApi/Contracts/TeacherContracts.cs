namespace DayCareManagement.WebApi.Contracts;

public sealed record TeacherResponse(
	int TeacherId,
	string FirstName,
	string LastName,
	DateOnly RegisterDate,
	bool IsAssigned,
	string? ClassRoomName,
	string Email,
	int Credits);

public sealed record TeacherWriteRequest(
	int TeacherId,
	string FirstName,
	string LastName,
	DateOnly RegisterDate,
	bool IsAssigned,
	string? ClassRoomName,
	string Email,
	string Password,
	int Credits);
