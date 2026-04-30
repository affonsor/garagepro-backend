using FluentAssertions;
using GaragePro.Application.Common;
using GaragePro.Application.Features.Appointments.GetById;
using GaragePro.Core.Entities;
using GaragePro.Core.Enums;
using GaragePro.Core.Interfaces.Repositories;
using Moq;

namespace GaragePro.UnitTests.Handlers.Appointments;

public class GetAppointmentByIdHandlerTests
{
    private readonly Mock<IAppointmentRepository> _repoMock = new();
    private readonly GetAppointmentByIdHandler _handler;

    public GetAppointmentByIdHandlerTests()
    {
        _handler = new GetAppointmentByIdHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnDetailResponse_WhenAppointmentExists()
    {
        var id = Guid.NewGuid();
        var appointment = BuildAppointment(id);

        _repoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        var result = await _handler.Handle(new GetAppointmentByIdQuery(id), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(id);
        result.Value.ClientName.Should().Be("Test Client");
        result.Value.RescheduleHistory.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenAppointmentDoesNotExist()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        var result = await _handler.Handle(new GetAppointmentByIdQuery(Guid.NewGuid()), default);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    private static Appointment BuildAppointment(Guid id) => new()
    {
        Id = id,
        Status = AppointmentStatus.Scheduled,
        TotalValue = 100m,
        ClientId = Guid.NewGuid(),
        ProductId = Guid.NewGuid(),
        ServiceId = Guid.NewGuid(),
        StartAt = DateTimeOffset.UtcNow,
        ExpectedEndAt = DateTimeOffset.UtcNow.AddHours(1),
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow,
        Client = new Client { Name = "Test Client" },
        Product = new Product { Name = "Test Product" },
        Service = new Service { Name = "Test Service" },
        RescheduleHistory = new List<AppointmentRescheduleHistory>()
    };
}
