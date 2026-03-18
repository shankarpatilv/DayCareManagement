using System.Net;
using System.Net.Http.Json;

namespace DayCareManagement.WebApp.Services;

public sealed class DayCareApiClient(HttpClient httpClient)
{
	public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
	{
		using var response = await httpClient.PostAsJsonAsync("/auth/login", request, cancellationToken);
		if (response.StatusCode == HttpStatusCode.Unauthorized)
		{
			return null;
		}

		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
	}

	public async Task<IReadOnlyList<StudentResponse>> GetStudentsAsync(CancellationToken cancellationToken = default)
	{
		var students = await ReadAsJsonAsync<List<StudentResponse>>("/students", allowNotFound: false, cancellationToken);
		return students ?? [];
	}

	public Task<StudentResponse?> GetStudentByIdAsync(int studentId, CancellationToken cancellationToken = default)
	{
		return ReadAsJsonAsync<StudentResponse>($"/students/{studentId}", allowNotFound: true, cancellationToken);
	}

	public async Task<IReadOnlyList<ImmunizationResponse>> GetStudentImmunizationsAsync(int studentId, CancellationToken cancellationToken = default)
	{
		var immunizations = await ReadAsJsonAsync<List<ImmunizationResponse>>($"/students/{studentId}/immunizations", allowNotFound: true, cancellationToken);
		return immunizations ?? [];
	}

	private async Task<T?> ReadAsJsonAsync<T>(string url, bool allowNotFound, CancellationToken cancellationToken)
	{
		using var response = await httpClient.GetAsync(url, cancellationToken);
		if (response.StatusCode == HttpStatusCode.Unauthorized)
		{
			throw new UnauthorizedAccessException("Authentication is required for this resource.");
		}

		if (allowNotFound && response.StatusCode == HttpStatusCode.NotFound)
		{
			return default;
		}

		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
	}
}

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

public sealed record ImmunizationResponse(
	int StudentId,
	int ImmunizationId,
	string ImmunizationName,
	string Duration,
	DateOnly ImmunizationDate,
	bool Status);