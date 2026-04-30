using GaragePro.Application.Common;
using GaragePro.Application.Features.Vehicles;
using GaragePro.Application.Features.Vehicles.Create;
using GaragePro.Application.Features.Vehicles.Delete;
using GaragePro.Application.Features.Vehicles.GetAll;
using GaragePro.Application.Features.Vehicles.GetById;
using GaragePro.Application.Features.Vehicles.Transfer;
using GaragePro.Application.Features.Vehicles.Update;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GaragePro.API.Endpoints;

public static class VehiclesEndpoints
{
    public static IEndpointRouteBuilder MapVehiclesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/vehicles")
            .RequireAuthorization("TechnicianOrAdmin")
            .WithTags("Vehicles");

        group.MapGet("/", async (IMediator mediator, int pageNumber = 1, int pageSize = 20, Guid? clientId = null) =>
        {
            var result = await mediator.Send(new GetAllVehiclesQuery(pageNumber, pageSize, clientId));
            return Results.Ok(result.Value);
        })
        .WithSummary("List all vehicles paginated")
        .WithDescription("Returns a paginated list of vehicles with current owner info. Optionally filter by clientId.")
        .Produces<PaginatedResult<VehicleSummaryResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetVehicleByIdQuery(id));
            return result.Status switch
            {
                ResultStatus.Success => Results.Ok(result.Value),
                ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .WithSummary("Get vehicle with transfer history")
        .WithDescription("Returns full vehicle details including the complete ownership transfer history.")
        .Produces<VehicleDetailResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateVehicleCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.Status switch
            {
                ResultStatus.Success => Results.Created($"/api/vehicles/{result.Value}", new IdResponse(result.Value)),
                ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
                ResultStatus.ValidationFailure => Results.BadRequest(new { error = "Validation failed", errors = result.Errors }),
                ResultStatus.Failure => Results.Conflict(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .WithSummary("Register a new vehicle")
        .WithDescription("Registers a vehicle under a client. License plate must be unique.")
        .Produces<IdResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", async (Guid id, UpdateVehicleCommand command, IMediator mediator) =>
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
        .WithSummary("Update vehicle data")
        .WithDescription("Updates make, model, year, color, and VIN. License plate and owner are not updatable here.")
        .Produces<IdResponse>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteVehicleCommand(id));
            return result.Status switch
            {
                ResultStatus.Success => Results.NoContent(),
                ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
                ResultStatus.Failure => Results.BadRequest(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .WithSummary("Delete a vehicle")
        .WithDescription("Permanently removes the vehicle. Fails with 400 if the vehicle has transfer history.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/transfer", async (Guid id, TransferVehicleCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command with { VehicleId = id });
            return result.Status switch
            {
                ResultStatus.Success => Results.Ok(result.Value),
                ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
                ResultStatus.ValidationFailure => Results.BadRequest(new { error = "Validation failed", errors = result.Errors }),
                ResultStatus.Failure => Results.BadRequest(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .WithSummary("Transfer vehicle to another client")
        .WithDescription("Atomically changes vehicle ownership and records the transfer. Target client must be different from current owner.")
        .Produces<TransferResponse>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        return routes;
    }
}
