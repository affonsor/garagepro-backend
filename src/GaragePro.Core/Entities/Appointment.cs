using GaragePro.Core.Enums;

namespace GaragePro.Core.Entities;

public class Appointment
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public Guid ProductId { get; set; }
    public Guid ServiceId { get; set; }
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset ExpectedEndAt { get; set; }
    public AppointmentStatus Status { get; set; }
    public bool IsRescheduled { get; set; }
    public int RescheduleCount { get; set; }
    public decimal ProductValueSnapshot { get; set; }
    public decimal ServiceValueSnapshot { get; set; }
    public decimal TotalValue { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Client Client { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public Service Service { get; set; } = null!;
    public ICollection<AppointmentRescheduleHistory> RescheduleHistory { get; set; } = [];
}
