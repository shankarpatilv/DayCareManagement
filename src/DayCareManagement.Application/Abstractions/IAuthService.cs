namespace DayCareManagement.Application.Abstractions;

public interface IAuthService
{
    LoginResolution ResolveUser(AuthCredentialRecord? student, AuthCredentialRecord? teacher);

    PasswordVerificationStatus VerifyPassword(string incomingPassword, string storedPassword);

    string IssueToken(int subjectId, AuthRole role, string email);
}

public enum AuthRole
{
    Student,
    Teacher
}

public enum LoginResolutionStatus
{
    NotFound,
    Ambiguous,
    Resolved
}

public enum PasswordVerificationStatus
{
    Verified,
    Invalid,
    Unsupported
}

public sealed record AuthCredentialRecord(int SubjectId, string PasswordHash);

public sealed record LoginResolution(
    LoginResolutionStatus Status,
    AuthRole? Role,
    int? SubjectId,
    string? PasswordHash);