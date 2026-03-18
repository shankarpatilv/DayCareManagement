using DayCareManagement.Infrastructure.Configuration;
using Xunit;

namespace DayCareManagement.Application.Tests;

public sealed class JwtSigningKeyPolicyTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("short-key")]
    [InlineData("replace-with-secure-32-plus-character-secret")]
    [InlineData("change-me-to-a-secure-random-jwt-signing-key")]
    [InlineData("your-jwt-signing-key-please-replace-this-value")]
    public void GetValidationError_ReturnsError_ForMissingShortOrPlaceholderValues(string? signingKey)
    {
        var result = JwtSigningKeyPolicy.GetValidationError(signingKey);

        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("u2gS!6v8Qx#1mZ9@kD4$wN7^pT3&hJ5*")]
    [InlineData("tenant-default-route-secret-2026-09-17-prod")]
    public void GetValidationError_ReturnsNull_ForValidNonPlaceholderValues(string signingKey)
    {
        var result = JwtSigningKeyPolicy.GetValidationError(signingKey);

        Assert.Null(result);
    }
}