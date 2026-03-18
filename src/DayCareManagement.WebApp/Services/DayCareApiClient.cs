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

	public async Task<StudentResponse> CreateStudentAsync(StudentWriteRequest request, CancellationToken cancellationToken = default)
	{
		using var response = await httpClient.PostAsJsonAsync("/students", request, cancellationToken);
		await EnsureSuccessAsync(response, cancellationToken);
		var student = await response.Content.ReadFromJsonAsync<StudentResponse>(cancellationToken);
		return student ?? throw new InvalidOperationException("Unexpected empty response while creating student.");
	}

	public async Task UpdateStudentAsync(int studentId, StudentWriteRequest request, CancellationToken cancellationToken = default)
	{
		using var response = await httpClient.PutAsJsonAsync($"/students/{studentId}", request, cancellationToken);
		await EnsureSuccessAsync(response, cancellationToken);
	}

	public async Task DeleteStudentAsync(int studentId, CancellationToken cancellationToken = default)
	{
		using var response = await httpClient.DeleteAsync($"/students/{studentId}", cancellationToken);
		await EnsureSuccessAsync(response, cancellationToken);
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

	public async Task<ImmunizationResponse> CreateStudentImmunizationAsync(int studentId, ImmunizationCreateRequest request, CancellationToken cancellationToken = default)
	{
		using var response = await httpClient.PostAsJsonAsync($"/students/{studentId}/immunizations", request, cancellationToken);
		await EnsureSuccessAsync(response, cancellationToken);
		var immunization = await response.Content.ReadFromJsonAsync<ImmunizationResponse>(cancellationToken);
		return immunization ?? throw new InvalidOperationException("Unexpected empty response while creating immunization.");
	}

	public async Task UpdateStudentImmunizationAsync(
		int studentId,
		int immunizationId,
		DateOnly immunizationDate,
		ImmunizationUpdateRequest request,
		CancellationToken cancellationToken = default)
	{
		using var response = await httpClient.PutAsJsonAsync(
			$"/students/{studentId}/immunizations/{immunizationId}/{immunizationDate:yyyy-MM-dd}",
			request,
			cancellationToken);
		await EnsureSuccessAsync(response, cancellationToken);
	}

	public async Task DeleteStudentImmunizationAsync(
		int studentId,
		int immunizationId,
		DateOnly immunizationDate,
		CancellationToken cancellationToken = default)
	{
		using var response = await httpClient.DeleteAsync(
			$"/students/{studentId}/immunizations/{immunizationId}/{immunizationDate:yyyy-MM-dd}",
			cancellationToken);
		await EnsureSuccessAsync(response, cancellationToken);
	}

	public async Task<IReadOnlyList<TeacherResponse>> GetTeachersAsync(CancellationToken cancellationToken = default)
	{
		var teachers = await ReadAsJsonAsync<List<TeacherResponse>>("/teachers", allowNotFound: false, cancellationToken);
		return teachers ?? [];
	}

	public Task<TeacherResponse?> GetTeacherByIdAsync(int teacherId, CancellationToken cancellationToken = default)
	{
		return ReadAsJsonAsync<TeacherResponse>($"/teachers/{teacherId}", allowNotFound: true, cancellationToken);
	}

	public async Task<TeacherResponse> CreateTeacherAsync(TeacherWriteRequest request, CancellationToken cancellationToken = default)
	{
		using var response = await httpClient.PostAsJsonAsync("/teachers", request, cancellationToken);
		await EnsureSuccessAsync(response, cancellationToken);
		var teacher = await response.Content.ReadFromJsonAsync<TeacherResponse>(cancellationToken);
		return teacher ?? throw new InvalidOperationException("Unexpected empty response while creating teacher.");
	}

	public async Task UpdateTeacherAsync(int teacherId, TeacherWriteRequest request, CancellationToken cancellationToken = default)
	{
		using var response = await httpClient.PutAsJsonAsync($"/teachers/{teacherId}", request, cancellationToken);
		await EnsureSuccessAsync(response, cancellationToken);
	}

	public async Task DeleteTeacherAsync(int teacherId, CancellationToken cancellationToken = default)
	{
		using var response = await httpClient.DeleteAsync($"/teachers/{teacherId}", cancellationToken);
		await EnsureSuccessAsync(response, cancellationToken);
	}

	public async Task<IReadOnlyList<RenewalDueResponse>> GetRenewalsDueAsync(CancellationToken cancellationToken = default)
	{
		var renewals = await ReadAsJsonAsync<List<RenewalDueResponse>>("/renewals/due", allowNotFound: false, cancellationToken);
		return renewals ?? [];
	}

	public async Task<RenewalAppliedResponse> ApplyRenewalAsync(int studentId, CancellationToken cancellationToken = default)
	{
		using var response = await httpClient.PostAsync($"/renewals/{studentId}", content: null, cancellationToken);
		await EnsureSuccessAsync(response, cancellationToken);
		var renewal = await response.Content.ReadFromJsonAsync<RenewalAppliedResponse>(cancellationToken);
		return renewal ?? throw new InvalidOperationException("Unexpected empty response while applying renewal.");
	}

	public async Task<IReadOnlyList<StateRuleResponse>> GetStateRulesAsync(int? ageMonths = null, CancellationToken cancellationToken = default)
	{
		var route = ageMonths is null ? "/state-rules" : $"/state-rules?ageMonths={ageMonths.Value}";
		var rules = await ReadAsJsonAsync<List<StateRuleResponse>>(route, allowNotFound: false, cancellationToken);
		return rules ?? [];
	}

	private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
	{
		if (response.StatusCode == HttpStatusCode.Unauthorized)
		{
			throw new UnauthorizedAccessException("Authentication is required for this resource.");
		}

		if (response.IsSuccessStatusCode)
		{
			return;
		}

		var apiError = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(cancellationToken);
		var errorMessage = string.IsNullOrWhiteSpace(apiError?.Error)
			? $"Request failed with status {(int)response.StatusCode} ({response.StatusCode})."
			: apiError.Error;

		throw new InvalidOperationException(errorMessage);
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

		await EnsureSuccessAsync(response, cancellationToken);
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

public sealed record RenewalDueResponse(
	int StudentId,
	string FirstName,
	string LastName,
	DateOnly RegisterDate,
	int AgeMonths,
	DateOnly AsOfDate);

public sealed record RenewalAppliedResponse(int StudentId, DateOnly RegisterDate);

public sealed record StateRuleResponse(string VaccineName, string DoseRequirement, int AgeMonths);

public sealed record ApiErrorResponse(string? Error);