using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using DayCareManagement.Application.Abstractions;
using DayCareManagement.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DayCareManagement.Infrastructure.System;

public sealed class JwtAuthService(IOptions<JwtOptions> options) : IAuthService
{
    private static readonly Regex Sha256HexRegex = new("^[0-9a-fA-F]{64}$", RegexOptions.Compiled);
    private readonly JwtOptions _jwtOptions = options.Value;

    public LoginResolution ResolveUser(AuthCredentialRecord? student, AuthCredentialRecord? teacher)
    {
        if (student is not null && teacher is not null)
        {
            return new LoginResolution(LoginResolutionStatus.Ambiguous, null, null, null);
        }

        if (student is not null)
        {
            return new LoginResolution(
                LoginResolutionStatus.Resolved,
                AuthRole.Student,
                student.SubjectId,
                student.PasswordHash);
        }

        if (teacher is not null)
        {
            return new LoginResolution(
                LoginResolutionStatus.Resolved,
                AuthRole.Teacher,
                teacher.SubjectId,
                teacher.PasswordHash);
        }

        return new LoginResolution(LoginResolutionStatus.NotFound, null, null, null);
    }

    public PasswordVerificationStatus VerifyPassword(string incomingPassword, string storedPassword)
    {
        if (string.IsNullOrWhiteSpace(incomingPassword) || string.IsNullOrWhiteSpace(storedPassword))
        {
            return PasswordVerificationStatus.Unsupported;
        }

        if (Sha256HexRegex.IsMatch(storedPassword))
        {
            using var sha256 = SHA256.Create();
            var incomingBytes = Encoding.UTF8.GetBytes(incomingPassword);
            var computedHashBytes = sha256.ComputeHash(incomingBytes);
            var computedHashHex = Convert.ToHexString(computedHashBytes);

            return string.Equals(computedHashHex, storedPassword, StringComparison.OrdinalIgnoreCase)
                ? PasswordVerificationStatus.Verified
                : PasswordVerificationStatus.Invalid;
        }

        if (storedPassword.StartsWith("$2", StringComparison.Ordinal))
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(incomingPassword, storedPassword)
                    ? PasswordVerificationStatus.Verified
                    : PasswordVerificationStatus.Invalid;
            }
            catch
            {
                return PasswordVerificationStatus.Unsupported;
            }
        }

        return PasswordVerificationStatus.Unsupported;
    }

    public string IssueToken(int subjectId, AuthRole role, string email)
    {
        var signingKeyValidationError = JwtSigningKeyPolicy.GetValidationError(_jwtOptions.SigningKey);
        if (string.IsNullOrWhiteSpace(_jwtOptions.Issuer) ||
            string.IsNullOrWhiteSpace(_jwtOptions.Audience) ||
            signingKeyValidationError is not null)
        {
            throw new InvalidOperationException("JWT options are not configured correctly.");
        }

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subjectId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Role, role.ToString())
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiresMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}