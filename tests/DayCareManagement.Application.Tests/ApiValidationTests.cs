using System.Security.Cryptography;
using System.Text;
using DayCareManagement.WebApi.Validation;
using Xunit;

namespace DayCareManagement.Application.Tests;

public sealed class ApiValidationTests
{
    [Fact]
    public void NormalizePasswordForPersistence_ReturnsSha256InputAsIs()
    {
        var sha256Hex = ComputeSha256Hex("P@ssw0rd123");

        var normalized = ApiValidation.NormalizePasswordForPersistence(sha256Hex);

        Assert.Equal(sha256Hex, normalized);
    }

    [Fact]
    public void NormalizePasswordForPersistence_ReturnsBcryptInputAsIs()
    {
        const string password = "Strong#Password1";
        var bcryptHash = BCrypt.Net.BCrypt.HashPassword(password);

        var normalized = ApiValidation.NormalizePasswordForPersistence(bcryptHash);

        Assert.Equal(bcryptHash, normalized);
    }

    [Fact]
    public void NormalizePasswordForPersistence_HashesPlaintextToSha256LowerHex()
    {
        const string plaintext = "Plaintext#Password";
        var expected = ComputeSha256Hex(plaintext);

        var normalized = ApiValidation.NormalizePasswordForPersistence(plaintext);

        Assert.Equal(expected, normalized);
    }

    [Fact]
    public void TryParseDateOnly_AcceptsExactIsoDate()
    {
        var success = ApiValidation.TryParseDateOnly("2026-03-18", out var parsedDate);

        Assert.True(success);
        Assert.Equal(new DateOnly(2026, 3, 18), parsedDate);
    }

    [Theory]
    [InlineData("03/18/2026")]
    [InlineData("2026-3-18")]
    [InlineData("2026-03-18T00:00:00")]
    [InlineData("20260318")]
    public void TryParseDateOnly_RejectsNonExactIsoDate(string value)
    {
        var success = ApiValidation.TryParseDateOnly(value, out _);

        Assert.False(success);
    }

    private static string ComputeSha256Hex(string input)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
