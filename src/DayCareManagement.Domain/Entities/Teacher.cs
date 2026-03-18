namespace DayCareManagement.Domain.Entities;

public sealed class Teacher
{
    public int TeacherId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly RegisterDate { get; set; }
    public bool IsAssigned { get; set; }
    public string? ClassRoomName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int Credits { get; set; }
}