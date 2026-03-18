namespace DayCareManagement.Domain.Entities;

public sealed class StateRule
{
    public string VaccineName { get; set; } = string.Empty;
    public string DoseRequirement { get; set; } = string.Empty;
    public int AgeMonths { get; set; }
}