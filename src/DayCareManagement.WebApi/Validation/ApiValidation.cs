using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DayCareManagement.WebApi.Contracts;

namespace DayCareManagement.WebApi.Validation;

public static class ApiValidation
{
	private static readonly Regex Sha256HexRegex = new("^[0-9a-fA-F]{64}$", RegexOptions.Compiled);

	public static bool TryValidateStudentWriteRequest(StudentWriteRequest request, out string? validationError)
	{
		if (request.StudentId <= 0)
		{
			validationError = "StudentId must be greater than zero.";
			return false;
		}

		if (request.AgeMonths < 0)
		{
			validationError = "AgeMonths must be greater than or equal to zero.";
			return false;
		}

		if (request.GPA < 0m || request.GPA > 4m)
		{
			validationError = "GPA must be between 0.00 and 4.00.";
			return false;
		}

		if (string.IsNullOrWhiteSpace(request.FirstName) ||
			string.IsNullOrWhiteSpace(request.LastName) ||
			string.IsNullOrWhiteSpace(request.FatherName) ||
			string.IsNullOrWhiteSpace(request.MotherName) ||
			string.IsNullOrWhiteSpace(request.Address) ||
			string.IsNullOrWhiteSpace(request.PhoneNo) ||
			string.IsNullOrWhiteSpace(request.Email) ||
			string.IsNullOrWhiteSpace(request.Password))
		{
			validationError = "Student firstName, lastName, fatherName, motherName, address, phoneNo, email, and password are required.";
			return false;
		}

		validationError = null;
		return true;
	}

	public static bool TryValidateTeacherWriteRequest(TeacherWriteRequest request, out string? validationError)
	{
		if (request.TeacherId <= 0)
		{
			validationError = "TeacherId must be greater than zero.";
			return false;
		}

		if (request.Credits < 0)
		{
			validationError = "Credits must be greater than or equal to zero.";
			return false;
		}

		if (string.IsNullOrWhiteSpace(request.FirstName) ||
			string.IsNullOrWhiteSpace(request.LastName) ||
			string.IsNullOrWhiteSpace(request.Email) ||
			string.IsNullOrWhiteSpace(request.Password))
		{
			validationError = "Teacher firstName, lastName, email, and password are required.";
			return false;
		}

		validationError = null;
		return true;
	}

	public static bool TryValidateImmunizationCreateRequest(ImmunizationCreateRequest request, out string? validationError)
	{
		if (request.ImmunizationId <= 0)
		{
			validationError = "ImmunizationId must be greater than zero.";
			return false;
		}

		if (string.IsNullOrWhiteSpace(request.ImmunizationName) || string.IsNullOrWhiteSpace(request.Duration))
		{
			validationError = "ImmunizationName and Duration are required.";
			return false;
		}

		validationError = null;
		return true;
	}

	public static bool TryValidateImmunizationUpdateRequest(ImmunizationUpdateRequest request, out string? validationError)
	{
		if (string.IsNullOrWhiteSpace(request.ImmunizationName) || string.IsNullOrWhiteSpace(request.Duration))
		{
			validationError = "ImmunizationName and Duration are required.";
			return false;
		}

		validationError = null;
		return true;
	}

	public static string? NormalizeEmail(string email)
	{
		return string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
	}

	public static string NormalizePasswordForPersistence(string password)
	{
		if (string.IsNullOrWhiteSpace(password))
		{
			return password;
		}

		var candidate = password.Trim();
		if (IsSupportedStoredHash(candidate))
		{
			return candidate;
		}

		using var sha256 = SHA256.Create();
		var bytes = Encoding.UTF8.GetBytes(password);
		var hash = sha256.ComputeHash(bytes);
		return Convert.ToHexString(hash).ToLowerInvariant();
	}

	public static bool IsSupportedStoredHash(string password)
	{
		return IsSha256Hex(password) || IsBcryptHash(password);
	}

	public static bool IsSha256Hex(string value)
	{
		return !string.IsNullOrWhiteSpace(value) && Sha256HexRegex.IsMatch(value);
	}

	public static bool IsBcryptHash(string value)
	{
		return !string.IsNullOrWhiteSpace(value) && value.StartsWith("$2", StringComparison.Ordinal);
	}

	public static bool TryParseDateOnly(string value, out DateOnly parsedDate)
	{
		return DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);
	}
}