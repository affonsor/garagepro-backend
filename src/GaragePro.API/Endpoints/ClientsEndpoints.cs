using GaragePro.Application.Common;
using GaragePro.Application.Features.Addresses.Add;
using GaragePro.Application.Features.Addresses.Delete;
using GaragePro.Application.Features.Addresses.Update;
using GaragePro.Application.Features.Clients;
using GaragePro.Application.Features.Clients.Create;
using GaragePro.Application.Features.Clients.Delete;
using GaragePro.Application.Features.Clients.GetAll;
using GaragePro.Application.Features.Clients.GetById;
using GaragePro.Application.Features.Clients.Update;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GaragePro.API.Endpoints;

public static class ClientsEndpoints
{
    public static IEndpointRouteBuilder MapClientsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/clients")
            .RequireAuthorization()
            .WithTags("Clients");

        group.MapGet("/", async (IMediator mediator, int pageNumber = 1, int pageSize = 20) =>
        {
            var result = await mediator.Send(new GetAllClientsQuery(pageNumber, pageSize));
            return Results.Ok(result.Value);
        })
        .WithSummary("List all clients paginated")
        .WithDescription("Returns a paginated list of clients with vehicle count. Accessible by all authenticated roles.")
        .Produces<PaginatedResult<ClientSummaryResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetClientByIdQuery(id));
            return result.Status switch
            {
                ResultStatus.Success => Results.Ok(result.Value),
                ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .WithSummary("Get client full profile")
        .WithDescription("Returns the full client profile including all addresses and associated vehicles.")
        .Produces<ClientDetailResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateClientCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.Status switch
            {
                ResultStatus.Success => Results.Created($"/api/clients/{result.Value}", new IdResponse(result.Value)),
                ResultStatus.ValidationFailure => Results.BadRequest(new { error = "Validation failed", errors = result.Errors }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .RequireAuthorization("TechnicianOrAdmin")
        .WithSummary("Create a new client")
        .WithDescription("Creates a client with at least one address. Requires Technician or Admin role.")
        .Produces<IdResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, UpdateClientCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command with { Id = id });
            return result.Status switch
            {
                ResultStatus.Success => Results.Ok(new IdResponse(result.Value)),
                ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
                ResultStatus.ValidationFailure => Results.BadRequest(new { error = "Validation failed", errors = result.Errors }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .RequireAuthorization("TechnicianOrAdmin")
        .WithSummary("Update client basic data")
        .WithDescription("Updates name, email, phone, and document. Addresses are managed via the /addresses sub-routes.")
        .Produces<IdResponse>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteClientCommand(id));
            return result.Status switch
            {
                ResultStatus.Success => Results.NoContent(),
                ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
                ResultStatus.Failure => Results.BadRequest(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .RequireAuthorization("TechnicianOrAdmin")
        .WithSummary("Delete a client")
        .WithDescription("Permanently removes the client. Fails with 400 if the client has linked vehicles.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{clientId:guid}/addresses", async (Guid clientId, AddAddressCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command with { ClientId = clientId });
            return result.Status switch
            {
                ResultStatus.Success => Results.Created($"/api/clients/{clientId}/addresses/{result.Value}", new IdResponse(result.Value)),
                ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
                ResultStatus.ValidationFailure => Results.BadRequest(new { error = "Validation failed", errors = result.Errors }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .RequireAuthorization("TechnicianOrAdmin")
        .WithSummary("Add address to client")
        .WithDescription("Adds a new address to an existing client. Requires Technician or Admin role.")
        .Produces<IdResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{clientId:guid}/addresses/{addressId:guid}", async (Guid clientId, Guid addressId, UpdateAddressCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command with { ClientId = clientId, AddressId = addressId });
            return result.Status switch
            {
                ResultStatus.Success => Results.Ok(new IdResponse(result.Value)),
                ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
                ResultStatus.ValidationFailure => Results.BadRequest(new { error = "Validation failed", errors = result.Errors }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .RequireAuthorization("TechnicianOrAdmin")
        .WithSummary("Update client address")
        .WithDescription("Updates an existing address. The address must belong to the specified client.")
        .Produces<IdResponse>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{clientId:guid}/addresses/{addressId:guid}", async (Guid clientId, Guid addressId, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteAddressCommand(clientId, addressId));
            return result.Status switch
            {
                ResultStatus.Success => Results.NoContent(),
                ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
                ResultStatus.Failure => Results.BadRequest(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .RequireAuthorization("TechnicianOrAdmin")
        .WithSummary("Delete client address")
        .WithDescription("Removes an address. Fails with 400 if it is the client's only address.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        return routes;
    }
}
