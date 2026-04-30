using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Addresses.Delete;

public record DeleteAddressCommand(Guid ClientId, Guid AddressId) : IRequest<Result<bool>>;
