using GaragePro.Application.Common;
using GaragePro.Application.Features.Users;
using GaragePro.Application.Features.Users.Create;
using GaragePro.Application.Features.Users.Delete;
using GaragePro.Application.Features.Users.GetAll;
using GaragePro.Application.Features.Users.GetById;
using GaragePro.Application.Features.Users.Update;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GaragePro.API.Endpoints;

public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/users")
            .RequireAuthorization("AdminOnly")
            .WithTags("Users");

        group.MapGet("/", async (IMediator mediator, int pageNumber = 1, int pageSize = 20) =>
        {
            var result = await mediator.Send(new GetAllUsersQuery(pageNumber, pageSize));
            return Results.Ok(result.Value);
        })
        .WithSummary("List all users paginated")
        .WithDescription("Returns a paginated list of all system users. Requires Admin role.")
        .Produces<PaginatedResult<UserResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetUserByIdQuery(id));
            return result.Status switch
            {
                ResultStatus.Success => Results.Ok(result.Value),
                ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .WithSummary("Get user by ID")
        .WithDescription("Returns full user details by ID. Requires Admin role.")
        .Produces<UserResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateUserCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.Status switch
            {
                ResultStatus.Success => Results.Created($"/api/users/{result.Value}", new IdResponse(result.Value)),
                ResultStatus.ValidationFailure => Results.BadRequest(new { error = "Validation failed", errors = result.Errors }),
                ResultStatus.Failure => Results.Conflict(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .WithSummary("Create a new user")
        .WithDescription("Creates a system user with the given roles. Email must be unique. Requires Admin role.")
        .Produces<IdResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", async (Guid id, UpdateUserCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command with { Id = id });
            return result.Status switch
            {
                ResultStatus.Success => Results.Ok(new IdResponse(result.Value)),
                ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
                ResultStatus.ValidationFailure => Results.BadRequest(new { error = "Validation failed", errors = result.Errors }),
                ResultStatus.Failure => Results.Conflict(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .WithSummary("Update user data")
        .WithDescription("Updates name, email, and roles. Password changes are not supported here. Requires Admin role.")
        .Produces<IdResponse>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteUserCommand(id));
            return result.Status switch
            {
                ResultStatus.Success => Results.NoContent(),
                ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .WithSummary("Delete a user")
        .WithDescription("Permanently removes the user from the system. Requires Admin role.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);

        return routes;
    }
}
