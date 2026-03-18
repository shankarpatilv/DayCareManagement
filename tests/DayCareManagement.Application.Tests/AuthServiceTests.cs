using System.Security.Cryptography;
using System.Text;
using DayCareManagement.Application.Abstractions;
using DayCareManagement.Infrastructure.Configuration;
using DayCareManagement.Infrastructure.System;
using Microsoft.Extensions.Options;
using Xunit;

namespace DayCareManagement.Application.Tests;

public sealed class AuthServiceTests
{
    private static readonly JwtAuthService AuthService = new(
        Options.Create(new JwtOptions
        {
            Issuer = "daycare-tests",
            Audience = "daycare-tests",
            SigningKey = "this-is-a-test-signing-key-with-32-chars",
            ExpiresMinutes = 60
        }));

    [Fact]
    public void VerifyPassword_ReturnsVerified_ForMatchingSha256Hex()
    {
        const string password = "P@ssw0rd123";
        var storedHash = ComputeSha256Hex(password);

        var result = AuthService.VerifyPassword(password, storedHash);

        Assert.Equal(PasswordVerificationStatus.Verified, result);
    }

    [Fact]
    public void VerifyPassword_ReturnsVerified_ForMatchingBcrypt()
    {
        const string password = "Another#StrongPassword";
        var storedHash = BCrypt.Net.BCrypt.HashPassword(password);

        var result = AuthService.VerifyPassword(password, storedHash);

        Assert.Equal(PasswordVerificationStatus.Verified, result);
    }

    [Fact]
    public void VerifyPassword_ReturnsUnsupported_ForMalformedBcryptValue()
    {
        var result = AuthService.VerifyPassword("password", "$2bad-value");

        Assert.Equal(PasswordVerificationStatus.Unsupported, result);
    }

    [Fact]
    public void VerifyPassword_ReturnsUnsupported_ForNonHashValue()
    {
        var result = AuthService.VerifyPassword("password", "plaintext-password");

        Assert.Equal(PasswordVerificationStatus.Unsupported, result);
    }

    [Fact]
    public void ResolveUser_ReturnsAmbiguous_WhenStudentAndTeacherBothMatchEmail()
    {
        var student = new AuthCredentialRecord(11, ComputeSha256Hex("student-password"));
        var teacher = new AuthCredentialRecord(77, ComputeSha256Hex("teacher-password"));

        var result = AuthService.ResolveUser(student, teacher);

        Assert.Equal(LoginResolutionStatus.Ambiguous, result.Status);
        Assert.Null(result.Role);
        Assert.Null(result.SubjectId);
        Assert.Null(result.PasswordHash);
    }

    [Fact]
    public void ResolveUser_ReturnsNotFound_WhenNoMatches()
    {
        var result = AuthService.ResolveUser(student: null, teacher: null);

        Assert.Equal(LoginResolutionStatus.NotFound, result.Status);
    }

    private static string ComputeSha256Hex(string password)
    {
        using var sha256 = SHA256.Create();
        return Convert.ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(password))).ToLowerInvariant();
    }
}