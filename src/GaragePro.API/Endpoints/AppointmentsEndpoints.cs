using GaragePro.Application.Common;
using GaragePro.Application.Features.Appointments;
using GaragePro.Application.Features.Appointments.Cancel;
using GaragePro.Application.Features.Appointments.Complete;
using GaragePro.Application.Features.Appointments.Create;
using GaragePro.Application.Features.Appointments.GetAll;
using GaragePro.Application.Features.Appointments.GetById;
using GaragePro.Application.Features.Appointments.Reschedule;
using GaragePro.Core.Enums;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GaragePro.API.Endpoints;

public static class AppointmentsEndpoints
{
    public static IEndpointRouteBuilder MapAppointmentsEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/appointments")
            .RequireAuthorization()
            .WithTags("Appointments");

        group.MapGet("/", async (
            IMediator mediator,
            DateOnly? startDate = null,
            DateOnly? endDate = null,
            AppointmentStatus? status = null,
            Guid? clientId = null,
            string? search = null,
            int pageNumber = 1,
            int pageSize = 20) =>
        {
            var result = await mediator.Send(new GetAppointmentsQuery(
                startDate, endDate, status, clientId, search, pageNumber, pageSize));
            return Results.Ok(result.Value);
        })
        .WithSummary("List appointments paginated with financial summary")
        .Produces<AppointmentListResponse>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetAppointmentByIdQuery(id));
            return result.Status switch
            {
                ResultStatus.Success => Results.Ok(result.Value),
                ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .WithSummary("Get appointment details including reschedule history")
        .Produces<AppointmentDetailResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateAppointmentCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.Status switch
            {
                ResultStatus.Success => Results.Created($"/api/appointments/{result.Value}", new IdResponse(result.Value)),
                ResultStatus.ValidationFailure => Results.BadRequest(new { error = "Validation failed", errors = result.Errors }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .RequireAuthorization("TechnicianOrAdmin")
        .WithSummary("Create a new appointment")
        .Produces<IdResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/complete", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new CompleteAppointmentCommand(id));
            return result.Status switch
            {
                ResultStatus.Success => Results.Ok(new IdResponse(result.Value)),
                ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
                ResultStatus.Failure when result.Error!.Contains("modificado") =>
                    Results.Conflict(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .RequireAuthorization("TechnicianOrAdmin")
        .WithSummary("Complete a scheduled appointment")
        .Produces<IdResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/cancel", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new CancelAppointmentCommand(id));
            return result.Status switch
            {
                ResultStatus.Success => Results.Ok(new IdResponse(result.Value)),
                ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
                ResultStatus.Failure when result.Error!.Contains("modificado") =>
                    Results.Conflict(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .RequireAuthorization("TechnicianOrAdmin")
        .WithSummary("Cancel a scheduled appointment")
        .Produces<IdResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/reschedule", async (Guid id, RescheduleAppointmentCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command with { Id = id });
            return result.Status switch
            {
                ResultStatus.Success => Results.Ok(new IdResponse(result.Value)),
                ResultStatus.NotFound => Results.NotFound(new { error = result.Error }),
                ResultStatus.ValidationFailure => Results.BadRequest(new { error = "Validation failed", errors = result.Errors }),
                ResultStatus.Failure when result.Error!.Contains("modificado") =>
                    Results.Conflict(new { error = result.Error }),
                _ => Results.BadRequest(new { error = result.Error })
            };
        })
        .RequireAuthorization("TechnicianOrAdmin")
        .WithSummary("Reschedule a scheduled appointment")
        .Produces<IdResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        return routes;
    }
}
