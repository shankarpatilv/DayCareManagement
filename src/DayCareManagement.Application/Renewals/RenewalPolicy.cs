namespace DayCareManagement.Application.Renewals;

public static class RenewalPolicy
{
    public static bool IsDue(DateOnly registerDate, DateOnly asOfDate)
    {
        return registerDate <= asOfDate.AddYears(-1);
    }
}