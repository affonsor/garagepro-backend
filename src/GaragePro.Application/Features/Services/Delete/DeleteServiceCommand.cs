using GaragePro.Application.Common;
using MediatR;

namespace GaragePro.Application.Features.Services.Delete;

public record DeleteServiceCommand(Guid Id) : IRequest<Result<bool>>;
