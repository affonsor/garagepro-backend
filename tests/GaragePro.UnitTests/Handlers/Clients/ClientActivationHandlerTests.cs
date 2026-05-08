using FluentAssertions;
using GaragePro.Application.Common;
using GaragePro.Application.Features.Clients.Delete;
using GaragePro.Application.Features.Clients.Reactivate;
using GaragePro.Core.Entities;
using GaragePro.Core.Enums;
using GaragePro.Core.Interfaces.Repositories;
using Moq;

namespace GaragePro.UnitTests.Handlers.Clients;

public class ClientActivationHandlerTests
{
    private readonly Mock<IClientRepository> _clientRepository = new();

    [Fact]
    public async Task Delete_ShouldDeactivateClient_WhenClientIsActive()
    {
        var client = new Client { Id = Guid.NewGuid(), Name = "Maria", Document = "52998224725", IsActive = true };
        _clientRepository.Setup(r => r.GetByIdAsync(client.Id)).ReturnsAsync(client);
        _clientRepository.Setup(r => r.UpdateAsync(client)).Returns(Task.CompletedTask);
        var handler = new DeleteClientHandler(_clientRepository.Object);

        var result = await handler.Handle(new DeleteClientCommand(client.Id), default);

        result.IsSuccess.Should().BeTrue();
        client.IsActive.Should().BeFalse();
        _clientRepository.Verify(r => r.UpdateAsync(client), Times.Once);
    }

    [Fact]
    public async Task Delete_ShouldBeIdempotent_WhenClientIsAlreadyInactive()
    {
        var client = new Client { Id = Guid.NewGuid(), Name = "Maria", Document = "52998224725", IsActive = false };
        _clientRepository.Setup(r => r.GetByIdAsync(client.Id)).ReturnsAsync(client);
        var handler = new DeleteClientHandler(_clientRepository.Object);

        var result = await handler.Handle(new DeleteClientCommand(client.Id), default);

        result.IsSuccess.Should().BeTrue();
        _clientRepository.Verify(r => r.UpdateAsync(It.IsAny<Client>()), Times.Never);
    }

    [Fact]
    public async Task Reactivate_ShouldActivateClient_WhenInvariantsAreValid()
    {
        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = "Maria",
            Document = "52998224725",
            IsActive = false,
            Addresses = [new Address { Id = Guid.NewGuid(), Type = AddressType.Residential }]
        };
        _clientRepository.Setup(r => r.GetByIdAsync(client.Id)).ReturnsAsync(client);
        _clientRepository.Setup(r => r.CountVehiclesByClientIdAsync(client.Id)).ReturnsAsync(1);
        _clientRepository.Setup(r => r.UpdateAsync(client)).Returns(Task.CompletedTask);
        var handler = new ReactivateClientHandler(_clientRepository.Object);

        var result = await handler.Handle(new ReactivateClientCommand(client.Id), default);

        result.IsSuccess.Should().BeTrue();
        client.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Reactivate_ShouldReturnFailure_WhenClientHasNoVehicles()
    {
        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = "Maria",
            Document = "52998224725",
            IsActive = false,
            Addresses = [new Address { Id = Guid.NewGuid(), Type = AddressType.Residential }]
        };
        _clientRepository.Setup(r => r.GetByIdAsync(client.Id)).ReturnsAsync(client);
        _clientRepository.Setup(r => r.CountVehiclesByClientIdAsync(client.Id)).ReturnsAsync(0);
        var handler = new ReactivateClientHandler(_clientRepository.Object);

        var result = await handler.Handle(new ReactivateClientCommand(client.Id), default);

        result.Status.Should().Be(ResultStatus.Failure);
        _clientRepository.Verify(r => r.UpdateAsync(It.IsAny<Client>()), Times.Never);
    }
}
