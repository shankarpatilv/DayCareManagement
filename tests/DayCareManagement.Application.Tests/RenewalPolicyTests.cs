using DayCareManagement.Application.Renewals;
using Xunit;

namespace DayCareManagement.Application.Tests;

public sealed class RenewalPolicyTests
{
    [Fact]
    public void IsDue_ReturnsFalse_WhenRegisterDateIsWithinOneYear()
    {
        var asOfDate = new DateOnly(2026, 3, 18);
        var registerDate = new DateOnly(2025, 3, 19);

        var result = RenewalPolicy.IsDue(registerDate, asOfDate);

        Assert.False(result);
    }

    [Fact]
    public void IsDue_ReturnsTrue_OnOneYearBoundary()
    {
        var asOfDate = new DateOnly(2026, 3, 18);
        var registerDate = new DateOnly(2025, 3, 18);

        var result = RenewalPolicy.IsDue(registerDate, asOfDate);

        Assert.True(result);
    }

    [Fact]
    public void IsDue_ReturnsTrue_WhenRegisterDateIsOlderThanOneYear()
    {
        var asOfDate = new DateOnly(2026, 3, 18);
        var registerDate = new DateOnly(2023, 12, 10);

        var result = RenewalPolicy.IsDue(registerDate, asOfDate);

        Assert.True(result);
    }

    [Fact]
    public void IsDue_HandlesLeapYearBoundary()
    {
        var asOfDate = new DateOnly(2025, 3, 1);
        var registerDate = new DateOnly(2024, 2, 29);

        var result = RenewalPolicy.IsDue(registerDate, asOfDate);

        Assert.True(result);
    }
}
