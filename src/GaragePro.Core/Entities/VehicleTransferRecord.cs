namespace GaragePro.Core.Entities;

public class VehicleTransferRecord
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public Guid FromClientId { get; set; }
    public Guid ToClientId { get; set; }
    public DateTimeOffset TransferredAt { get; set; }
    public string? Notes { get; set; }
    public Client FromClient { get; set; } = null!;
    public Client ToClient { get; set; } = null!;
}
