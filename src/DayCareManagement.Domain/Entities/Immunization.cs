namespace DayCareManagement.Domain.Entities;

public sealed class Immunization
{
    public int StudentId { get; set; }
    public int ImmunizationId { get; set; }
    public string ImmunizationName { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public DateOnly ImmunizationDate { get; set; }
    public bool Status { get; set; }

    public Student Student { get; set; } = null!;
}
