using GaragePro.Application.Common;
using GaragePro.Application.Features.Products;
using GaragePro.Application.Features.Products.Create;
using GaragePro.Application.Features.Products.Delete;
using GaragePro.Application.Features.Products.GetAll;
using GaragePro.Application.Features.Products.GetById;
using GaragePro.Application.Features.Products.Update;
using GaragePro.Core.Entities;
using GaragePro.Core.Enums;
using GaragePro.Core.Interfaces.Repositories;
using GaragePro.Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace GaragePro.API.Endpoints;

public static class ProductsEndpoints
{
    public static IEndpointRouteBuilder MapProductsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/products")
            .RequireAuthorization()
            .WithTags("Products");

        group.MapGet("/active", async (IProductRepository repo, CancellationToken ct) =>
            Results.Ok((await repo.GetActiveAsync(ct)).Select(ProductResponse.From)))
        .WithSummary("List active products")
        .RequireAuthorization();

        group.MapGet("/", async (
            IMediator mediator,
            int pageNumber = 1,
            int pageSize = 20,
            string? search = null,
            string? status = null,
            string? category = null) =>
        {
            var result = await mediator.Send(new GetAllProductsQuery(pageNumber, pageSize, search, status, category));
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

        group.MapGet("/{id:guid}/movement-summary", async (Guid id, string? month, AppDbContext db, CancellationToken ct) =>
        {
            var productExists = await db.Products.AnyAsync(p => p.Id == id, ct);
            if (!productExists)
                return Results.NotFound(new { error = "Product not found" });

            var hasMonth = false;
            var from = default(DateTimeOffset);
            var to = default(DateTimeOffset);
            if (!string.IsNullOrWhiteSpace(month) && DateOnly.TryParse($"{month}-01", out var parsed))
            {
                hasMonth = true;
                from = new DateTimeOffset(parsed.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
                to = new DateTimeOffset(parsed.AddMonths(1).AddDays(-1).ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
            }

            IQueryable<ServiceOrderProductLine> query = db.ServiceOrderProductLines
                .Include(l => l.ServiceOrder)
                .Where(l => l.ProductId == id);

            if (hasMonth)
            {
                query = query.Where(l => l.ServiceOrder.ScheduledAt >= from && l.ServiceOrder.ScheduledAt <= to);
            }

            var sold = await query
                .Where(l => l.ServiceOrder.Status == OrderStatus.Completed)
                .SumAsync(l => l.Quantity, ct);

            var serviceLinesQuery = db.ServiceOrderServiceLines
                .Include(l => l.ServiceOrder)
                .Include(l => l.Service).ThenInclude(s => s.Materials)
                .Where(l => l.ServiceOrder.Status == OrderStatus.Completed);

            if (hasMonth)
                serviceLinesQuery = serviceLinesQuery.Where(l => l.ServiceOrder.ScheduledAt >= from && l.ServiceOrder.ScheduledAt <= to);

            var serviceLines = await serviceLinesQuery
                .ToListAsync(ct);

            var internalConsumption = serviceLines.Sum(l =>
                l.Service.Materials
                    .Where(m => m.ProductId == id)
                    .Sum(m => m.Quantity * l.Quantity));

            return Results.Ok(new ProductMovementSummaryResponse(sold, internalConsumption, null));
        })
        .WithSummary("Get product movement summary")
        .Produces<ProductMovementSummaryResponse>(StatusCodes.Status200OK)
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
