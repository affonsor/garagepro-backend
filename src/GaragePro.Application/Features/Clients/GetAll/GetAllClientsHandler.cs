using GaragePro.Application.Common;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Clients.GetAll;

public class GetAllClientsHandler(IClientRepository clientRepository) : IRequestHandler<GetAllClientsQuery, Result<PaginatedResult<ClientSummaryResponse>>>
{
    public async Task<Result<PaginatedResult<ClientSummaryResponse>>> Handle(GetAllClientsQuery request, CancellationToken cancellationToken)
    {
        var (clients, total) = await clientRepository.GetAllAsync(request.PageNumber, request.PageSize);

        var responses = clients.Select(c => new ClientSummaryResponse(
            c.Id, c.Name, c.Email, c.Phone, c.Vehicles.Count, c.CreatedAt));

        var paginated = new PaginatedResult<ClientSummaryResponse>(responses, request.PageNumber, request.PageSize, total);
        return Result<PaginatedResult<ClientSummaryResponse>>.Success(paginated);
    }
}
