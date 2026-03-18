namespace DayCareManagement.WebApi.Contracts;

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(string Token, string Role, int SubjectId);