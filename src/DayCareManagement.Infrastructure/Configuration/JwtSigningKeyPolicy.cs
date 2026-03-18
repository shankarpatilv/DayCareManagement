using System.Text.RegularExpressions;

namespace DayCareManagement.Infrastructure.Configuration;

public static class JwtSigningKeyPolicy
{
    private static readonly HashSet<string> KnownPlaceholderKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "replace-with-secure-32-plus-character-secret",
        "your-256-bit-secret",
        "your-super-secret-key",
        "your-secret-key",
        "default-jwt-signing-key-change-me",
        "change-me-to-a-secure-random-jwt-signing-key",
        "changemechangemechangemechangeme"
    };

    private static readonly Regex ReplaceDirectivePattern = new(
        "^(replace|change)(-|_|\\s)?(me|this|with).+",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex TutorialPlaceholderPattern = new(
        "^(your|default|placeholder)(-|_|\\s).*(secret|signing|jwt|key).*$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string? GetValidationError(string? signingKey)
    {
        if (string.IsNullOrWhiteSpace(signingKey))
        {
            return "Jwt:SigningKey is missing.";
        }

        var trimmed = signingKey.Trim();
        if (trimmed.Length < 32)
        {
            return "Jwt:SigningKey must be at least 32 characters long.";
        }

        if (KnownPlaceholderKeys.Contains(trimmed) ||
            ReplaceDirectivePattern.IsMatch(trimmed) ||
            TutorialPlaceholderPattern.IsMatch(trimmed))
        {
            return "Jwt:SigningKey uses a placeholder/default value and must be replaced with a unique secret.";
        }

        return null;
    }
}