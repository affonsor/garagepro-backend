using GaragePro.Application.Common;
using GaragePro.Core.Entities;
using GaragePro.Core.Interfaces.Repositories;
using GaragePro.Core.ValueObjects;
using MediatR;

namespace GaragePro.Application.Features.Clients.Create;

public class CreateClientHandler(
    IClientRepository clientRepository,
    IVehicleRepository vehicleRepository) : IRequestHandler<CreateClientCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        if (!Document.TryCreate(request.Document, out var document))
            return Result<Guid>.ValidationFailure(["Document must be a valid CPF or CNPJ"]);

        var addresses = request.Addresses ?? [];
        var requestedVehicles = request.Vehicles ?? [];

        if (addresses.Count == 0)
            return Result<Guid>.ValidationFailure(["At least one address is required"]);

        if (requestedVehicles.Count == 0)
            return Result<Guid>.ValidationFailure(["At least one vehicle is required"]);

        if (await clientRepository.ExistsByDocumentAsync(document!.Value))
            return Result<Guid>.Conflict("Document already registered");

        var vehicles = requestedVehicles
            .Select(v => new
            {
                LicensePlate = NormalizeLicensePlate(v.LicensePlate),
                v.Make,
                v.Model,
                v.Type,
                v.Year,
                v.Color,
                v.VIN
            })
            .ToList();

        if (vehicles.GroupBy(v => v.LicensePlate).Any(g => g.Count() > 1))
            return Result<Guid>.Conflict("License plate repeated in request");

        foreach (var vehicle in vehicles)
        {
            if (await vehicleRepository.ExistsByLicensePlateAsync(vehicle.LicensePlate))
                return Result<Guid>.Conflict("License plate already registered");
        }

        var now = DateTimeOffset.UtcNow;
        var clientId = Guid.NewGuid();

        var client = new Client
        {
            Id = clientId,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Document = document.Value,
            Tier = NormalizeTier(request.Tier),
            Birthday = request.Birthday,
            AddressText = request.AddressText,
            Notes = request.Notes,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now,
            Addresses = addresses.Select(a => new Address
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                Type = a.Type,
                Street = a.Street,
                Number = a.Number,
                Complement = a.Complement,
                District = a.District,
                City = a.City,
                State = a.State,
                ZipCode = a.ZipCode,
                CreatedAt = now
            }).ToList(),
            Vehicles = vehicles.Select(v => new Vehicle
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                LicensePlate = v.LicensePlate,
                Make = v.Make,
                Model = v.Model,
                Type = v.Type,
                Year = v.Year,
                Color = v.Color,
                VIN = v.VIN,
                CreatedAt = now,
                UpdatedAt = now
            }).ToList()
        };

        await clientRepository.CreateAsync(client);
        return Result<Guid>.Success(client.Id);
    }

    private static string NormalizeLicensePlate(string licensePlate) =>
        licensePlate.Trim().ToUpperInvariant();

    private static string NormalizeTier(string? tier) =>
        string.IsNullOrWhiteSpace(tier) ? "standard" : tier.Trim();
}
