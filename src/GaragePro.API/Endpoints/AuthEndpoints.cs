using GaragePro.Application.Common;
using GaragePro.Application.Features.Auth.Login;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GaragePro.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/login", async (LoginCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.Status switch
            {
                ResultStatus.Success => Results.Ok(result.Value),
                ResultStatus.ValidationFailure => Results.BadRequest(new { error = "Validation failed", errors = result.Errors }),
                _ => Results.Unauthorized()
            };
        })
        .WithSummary("Authenticate user and receive JWT token")
        .WithDescription("Validates credentials and returns a JWT Bearer token valid for the configured expiry period.")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        return routes;
    }
}
