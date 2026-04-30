namespace GaragePro.Core.Entities;

public class AppointmentRescheduleHistory
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public DateTimeOffset PreviousStartAt { get; set; }
    public DateTimeOffset PreviousExpectedEndAt { get; set; }
    public DateTimeOffset NewStartAt { get; set; }
    public DateTimeOffset NewExpectedEndAt { get; set; }
    public string? Reason { get; set; }
    public Guid ChangedByUserId { get; set; }
    public DateTimeOffset ChangedAt { get; set; }

    public Appointment Appointment { get; set; } = null!;
    public User ChangedBy { get; set; } = null!;
}
