using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Services.GetById;

public record GetServiceByIdQuery(Guid Id) : IRequest<Result<ServiceResponse>>;
