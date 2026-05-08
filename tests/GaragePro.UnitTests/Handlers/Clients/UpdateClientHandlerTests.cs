using FluentAssertions;
using GaragePro.Application.Common;
using GaragePro.Application.Features.Clients.Update;
using GaragePro.Core.Entities;
using GaragePro.Core.Interfaces.Repositories;
using Moq;

namespace GaragePro.UnitTests.Handlers.Clients;

public class UpdateClientHandlerTests
{
    private readonly Mock<IClientRepository> _clientRepository = new();
    private readonly UpdateClientHandler _handler;

    public UpdateClientHandlerTests()
    {
        _handler = new UpdateClientHandler(_clientRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldNormalizeDocument_WhenUpdated()
    {
        var client = new Client { Id = Guid.NewGuid(), Name = "Maria", Document = "52998224725" };
        _clientRepository.Setup(r => r.GetByIdAsync(client.Id)).ReturnsAsync(client);
        _clientRepository.Setup(r => r.ExistsByDocumentAsync("04252011000110", client.Id)).ReturnsAsync(false);
        _clientRepository.Setup(r => r.UpdateAsync(client)).Returns(Task.CompletedTask);

        var result = await _handler.Handle(new UpdateClientCommand(
            client.Id, "Maria Ltda", null, null, "04.252.011/0001-10"), default);

        result.IsSuccess.Should().BeTrue();
        client.Document.Should().Be("04252011000110");
    }

    [Fact]
    public async Task Handle_ShouldReturnConflict_WhenDocumentBelongsToAnotherClient()
    {
        var client = new Client { Id = Guid.NewGuid(), Name = "Maria", Document = "52998224725" };
        _clientRepository.Setup(r => r.GetByIdAsync(client.Id)).ReturnsAsync(client);
        _clientRepository.Setup(r => r.ExistsByDocumentAsync("04252011000110", client.Id)).ReturnsAsync(true);

        var result = await _handler.Handle(new UpdateClientCommand(
            client.Id, "Maria Ltda", null, null, "04.252.011/0001-10"), default);

        result.Status.Should().Be(ResultStatus.Conflict);
        _clientRepository.Verify(r => r.UpdateAsync(It.IsAny<Client>()), Times.Never);
    }
}
