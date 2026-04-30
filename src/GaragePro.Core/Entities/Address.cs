using GaragePro.Core.Enums;

namespace GaragePro.Core.Entities;

public class Address
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public AddressType Type { get; set; }
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string? Complement { get; set; }
    public string District { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
