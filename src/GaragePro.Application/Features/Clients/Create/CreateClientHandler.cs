using GaragePro.Application.Common;
using GaragePro.Core.Entities;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Clients.Create;

public class CreateClientHandler(IClientRepository clientRepository) : IRequestHandler<CreateClientCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Document = request.Document,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Addresses = request.Addresses.Select(a => new Address
            {
                Id = Guid.NewGuid(),
                Type = a.Type,
                Street = a.Street,
                Number = a.Number,
                Complement = a.Complement,
                District = a.District,
                City = a.City,
                State = a.State,
                ZipCode = a.ZipCode,
                CreatedAt = DateTimeOffset.UtcNow
            }).ToList()
        };

        await clientRepository.CreateAsync(client);
        return Result<Guid>.Success(client.Id);
    }
}
