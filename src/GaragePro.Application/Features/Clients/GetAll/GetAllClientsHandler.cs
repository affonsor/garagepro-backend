using GaragePro.Application.Common;
using GaragePro.Core.Enums;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;

namespace GaragePro.Application.Features.Clients.GetAll;

public class GetAllClientsHandler(IClientRepository clientRepository) : IRequestHandler<GetAllClientsQuery, Result<PaginatedResult<ClientSummaryResponse>>>
{
    public async Task<Result<PaginatedResult<ClientSummaryResponse>>> Handle(GetAllClientsQuery request, CancellationToken cancellationToken)
    {
        var (clients, total) = await clientRepository.GetAllAsync(
            request.PageNumber,
            request.PageSize,
            request.IncludeInactive,
            request.Search,
            request.Tier,
            request.BirthdayMonth);

        var responses = clients.Select(c => new ClientSummaryResponse(
            c.Id,
            c.Name,
            c.Email,
            c.Phone,
            c.Document,
            c.IsActive,
            c.Vehicles.Count,
            c.CreatedAt,
            c.Tier,
            c.Birthday,
            c.AddressText,
            c.Notes,
            c.ServiceOrders.Count,
            c.ServiceOrders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.TotalPrice)));

        var paginated = new PaginatedResult<ClientSummaryResponse>(responses, request.PageNumber, request.PageSize, total);
        return Result<PaginatedResult<ClientSummaryResponse>>.Success(paginated);
    }
}
