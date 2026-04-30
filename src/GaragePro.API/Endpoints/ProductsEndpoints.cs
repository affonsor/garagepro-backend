using GaragePro.Application.Common;
using GaragePro.Application.Features.Products;
using GaragePro.Application.Features.Products.Create;
using GaragePro.Application.Features.Products.Delete;
using GaragePro.Application.Features.Products.GetAll;
using GaragePro.Application.Features.Products.GetById;
using GaragePro.Application.Features.Products.Update;
using GaragePro.Core.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GaragePro.API.Endpoints;

public static class ProductsEndpoints
{
    public static IEndpointRouteBuilder MapProductsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/products")
            .RequireAuthorization()
            .WithTags("Products");

        group.MapGet("/active", async (IProductRepository repo, CancellationToken ct) =>
            Results.Ok(await repo.GetActiveAsync(ct)))
        .WithSummary("List active products")
        .RequireAuthorization();

        group.MapGet("/", async (IMediator mediator, int pageNumber = 1, int pageSize = 20) =>
        {
            var result = await mediator.Send(new GetAllProductsQuery(pageNumber, pageSize));
            return Results.Ok(result.Value);
        })
        .WithSummary("List all products paginated")
        .WithDescription("Returns a paginated list of products. Accessible by all authenticated roles.")
        .Produces<PaginatedResult<ProductResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetProductByIdQuery(id));
            return result.Status switch
            {
                ResultStatus.Success => Results.Ok(result.Value),
                ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .WithSummary("Get product by ID")
        .WithDescription("Returns a single product by its ID.")
        .Produces<ProductResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateProductCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.Status switch
            {
                ResultStatus.Success => Results.Created($"/api/products/{result.Value}", new IdResponse(result.Value)),
                ResultStatus.ValidationFailure => Results.BadRequest(new { error = "Validation failed", errors = result.Errors }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .RequireAuthorization("FinancialOrAdmin")
        .WithSummary("Create a new product")
        .WithDescription("Adds a product to the catalog. Requires Financial or Admin role.")
        .Produces<IdResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, UpdateProductCommand command, IMediator mediator) =>
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
        .WithSummary("Update a product")
        .WithDescription("Updates name, description, and price. Requires Financial or Admin role.")
        .Produces<IdResponse>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteProductCommand(id));
            return result.Status switch
            {
                ResultStatus.Success => Results.NoContent(),
                ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .RequireAuthorization("FinancialOrAdmin")
        .WithSummary("Delete a product")
        .WithDescription("Permanently removes the product from the catalog. Requires Financial or Admin role.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return routes;
    }
}
