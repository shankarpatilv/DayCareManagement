using System;
using System.IO;
using DayCareManagement.Infrastructure.Configuration;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace DayCareManagement.Application.Tests;

public sealed class ConnectionStringResolverTests
{
    [Fact]
    public void ResolveOrThrow_Prefers_DaycareManagementConnectionString()
    {
        using var scope = new ResolverTestScope();

        Environment.SetEnvironmentVariable("DAYCAREMANAGEMENT_CONNECTIONSTRING", "Primary");
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Secondary");

        var resolved = ConnectionStringResolver.ResolveOrThrow("Configured");

        Assert.Equal("Primary", resolved);
    }

    [Fact]
    public void ResolveOrThrow_FallsBack_To_DefaultConnectionEnvironmentVariable()
    {
        using var scope = new ResolverTestScope();

        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Secondary");

        var resolved = ConnectionStringResolver.ResolveOrThrow("Configured");

        Assert.Equal("Secondary", resolved);
    }

    [Fact]
    public void ResolveOrThrow_FallsBack_To_ConfiguredValue()
    {
        using var scope = new ResolverTestScope();

        var resolved = ConnectionStringResolver.ResolveOrThrow("Configured");

        Assert.Equal("Configured", resolved);
    }

    [Fact]
    public void ResolveOrThrow_Throws_When_NoSourceAvailable()
    {
        using var scope = new ResolverTestScope();

        var exception = Assert.Throws<InvalidOperationException>(() => ConnectionStringResolver.ResolveOrThrow());

        Assert.Contains("Database connection string is not configured.", exception.Message);
    }

    private sealed class ResolverTestScope : IDisposable
    {
        private readonly string? _previousPrimary;
        private readonly string? _previousSecondary;
        private readonly string _previousCurrentDirectory;
        private readonly string _temporaryDirectory;

        public ResolverTestScope()
        {
            _previousPrimary = Environment.GetEnvironmentVariable("DAYCAREMANAGEMENT_CONNECTIONSTRING");
            _previousSecondary = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

            Environment.SetEnvironmentVariable("DAYCAREMANAGEMENT_CONNECTIONSTRING", null);
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);

            _previousCurrentDirectory = Directory.GetCurrentDirectory();
            _temporaryDirectory = Path.Combine(Path.GetTempPath(), $"dcm-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(_temporaryDirectory);
            Directory.SetCurrentDirectory(_temporaryDirectory);
        }

        public void Dispose()
        {
            Directory.SetCurrentDirectory(_previousCurrentDirectory);
            Environment.SetEnvironmentVariable("DAYCAREMANAGEMENT_CONNECTIONSTRING", _previousPrimary);
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _previousSecondary);

            if (Directory.Exists(_temporaryDirectory))
            {
                Directory.Delete(_temporaryDirectory, recursive: true);
            }
        }
    }
}
