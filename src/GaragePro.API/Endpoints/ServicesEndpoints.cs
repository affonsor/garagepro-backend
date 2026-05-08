using GaragePro.Application.Common;
using GaragePro.Application.Features.Services;
using GaragePro.Application.Features.Services.Create;
using GaragePro.Application.Features.Services.Delete;
using GaragePro.Application.Features.Services.GetAll;
using GaragePro.Application.Features.Services.GetById;
using GaragePro.Application.Features.Services.Update;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GaragePro.API.Endpoints;

public static class ServicesEndpoints
{
    public static IEndpointRouteBuilder MapServicesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/services")
            .RequireAuthorization()
            .WithTags("Services");

        group.MapGet("/active", async (IServiceRepository repo, CancellationToken ct) =>
        {
            var services = await repo.GetActiveAsync(ct);
            return Results.Ok(services.Select(ServiceResponse.From));
        })
        .WithSummary("List active services")
        .RequireAuthorization();

        group.MapGet("/", async (
            IMediator mediator,
            int pageNumber = 1,
            int pageSize = 20,
            string? search = null,
            string? category = null,
            string? tier = null,
            bool? active = null) =>
        {
            var result = await mediator.Send(new GetAllServicesQuery(pageNumber, pageSize, search, category, tier, active));
            return Results.Ok(result.Value);
        })
        .WithSummary("List all services paginated")
        .WithDescription("Returns a paginated list of services. Accessible by all authenticated roles.")
        .Produces<PaginatedResult<ServiceResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetServiceByIdQuery(id));
            return result.Status switch
            {
                ResultStatus.Success => Results.Ok(result.Value),
                ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .WithSummary("Get service by ID")
        .WithDescription("Returns a single service by its ID.")
        .Produces<ServiceResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateServiceCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.Status switch
            {
                ResultStatus.Success => Results.Created($"/api/services/{result.Value}", new IdResponse(result.Value)),
                ResultStatus.ValidationFailure => Results.BadRequest(new { error = "Validation failed", errors = result.Errors }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .RequireAuthorization("FinancialOrAdmin")
        .WithSummary("Create a new service")
        .WithDescription("Adds a service to the catalog. Requires Financial or Admin role.")
        .Produces<IdResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, UpdateServiceCommand command, IMediator mediator) =>
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
        .RequireAuthorization("FinancialOrAdmin")
        .WithSummary("Update a service")
        .WithDescription("Updates name, description, and price. Requires Financial or Admin role.")
        .Produces<IdResponse>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteServiceCommand(id));
            return result.Status switch
            {
                ResultStatus.Success => Results.NoContent(),
                ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .RequireAuthorization("FinancialOrAdmin")
        .WithSummary("Delete a service")
        .WithDescription("Permanently removes the service from the catalog. Requires Financial or Admin role.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return routes;
    }
}
