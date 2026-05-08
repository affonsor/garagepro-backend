using FluentAssertions;
using GaragePro.Application.Features.Clients.GetAll;
using GaragePro.Core.Entities;
using GaragePro.Core.Interfaces.Repositories;
using Moq;

namespace GaragePro.UnitTests.Handlers.Clients;

public class GetAllClientsHandlerTests
{
    private readonly Mock<IClientRepository> _clientRepository = new();
    private readonly GetAllClientsHandler _handler;

    public GetAllClientsHandlerTests()
    {
        _handler = new GetAllClientsHandler(_clientRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldExcludeInactiveClients_ByDefault()
    {
        _clientRepository.Setup(r => r.GetAllAsync(1, 20, false))
            .ReturnsAsync(([BuildClient(isActive: true)], 1));

        var result = await _handler.Handle(new GetAllClientsQuery(), default);

        result.IsSuccess.Should().BeTrue();
        _clientRepository.Verify(r => r.GetAllAsync(1, 20, false), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldIncludeInactiveClients_WhenRequested()
    {
        _clientRepository.Setup(r => r.GetAllAsync(1, 20, true))
            .ReturnsAsync(([BuildClient(isActive: true), BuildClient(isActive: false)], 2));

        var result = await _handler.Handle(new GetAllClientsQuery(1, 20, true), default);

        result.Value!.Data.Should().Contain(c => !c.IsActive);
        _clientRepository.Verify(r => r.GetAllAsync(1, 20, true), Times.Once);
    }

    private static Client BuildClient(bool isActive) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Maria",
        Document = "52998224725",
        IsActive = isActive,
        Vehicles = [new Vehicle { Id = Guid.NewGuid(), LicensePlate = "ABC1D23" }]
    };
}
