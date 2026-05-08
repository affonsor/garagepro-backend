namespace GaragePro.Core.Entities;

public class ServiceStep
{
    public Guid Id { get; set; }
    public Guid ServiceId { get; set; }
    public int Position { get; set; }
    public string Description { get; set; } = string.Empty;

    public Service Service { get; set; } = null!;
}
