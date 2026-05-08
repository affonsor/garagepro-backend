using FluentAssertions;
using GaragePro.Application.Features.Clients.GetAll;
using GaragePro.Application.Features.Products.GetAll;
using GaragePro.Application.Features.Services.GetAll;
using GaragePro.Core.Entities;
using GaragePro.Core.Enums;
using GaragePro.Core.Interfaces.Repositories;
using Moq;

namespace GaragePro.UnitTests.Handlers;

public class ReactMockContractHandlerTests
{
    [Fact]
    public async Task GetAllProductsHandler_ShouldPassReactFiltersAndMapCatalogFields()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Polidor Corte",
            Sku = "POL-001",
            Brand = "Meguiar's",
            Category = "Polidores",
            Size = "1L",
            Unit = "un",
            Description = "Produto premium",
            Cost = 80,
            Price = 145,
            Stock = 2,
            MinStock = 3,
            Supplier = "Fornecedor",
            Barcode = "789",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.GetAllAsync(1, 20, "POL", "lowStock", "Polidores"))
            .ReturnsAsync(([product], 1));

        var handler = new GetAllProductsHandler(repo.Object);
        var result = await handler.Handle(new GetAllProductsQuery(1, 20, "POL", "lowStock", "Polidores"), CancellationToken.None);

        var dto = result.Value!.Data.Single();
        dto.Sku.Should().Be("POL-001");
        dto.Stock.Should().Be(2);
        dto.MinStock.Should().Be(3);
        dto.Active.Should().BeTrue();
        repo.Verify(r => r.GetAllAsync(1, 20, "POL", "lowStock", "Polidores"), Times.Once);
    }

    [Fact]
    public async Task GetAllServicesHandler_ShouldPassReactFiltersAndMapMaterialsAndSteps()
    {
        var product = new Product { Id = Guid.NewGuid(), Name = "Microfibra", Unit = "un" };
        var service = new Service
        {
            Id = Guid.NewGuid(),
            Name = "Polimento Premium",
            Code = "POL-PRE",
            Tier = "premium",
            Category = "polimento",
            Duration = "4h",
            Price = 700,
            Cost = 100,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Materials =
            [
                new ServiceMaterial { Id = Guid.NewGuid(), ProductId = product.Id, Product = product, Quantity = 2 }
            ],
            Steps =
            [
                new ServiceStep { Id = Guid.NewGuid(), Position = 1, Description = "Lavar" },
                new ServiceStep { Id = Guid.NewGuid(), Position = 2, Description = "Polir" }
            ]
        };
        var repo = new Mock<IServiceRepository>();
        repo.Setup(r => r.GetAllAsync(1, 20, "POL", "polimento", "premium", true))
            .ReturnsAsync(([service], 1));

        var handler = new GetAllServicesHandler(repo.Object);
        var result = await handler.Handle(new GetAllServicesQuery(1, 20, "POL", "polimento", "premium", true), CancellationToken.None);

        var dto = result.Value!.Data.Single();
        dto.Code.Should().Be("POL-PRE");
        dto.Materials.Should().ContainSingle(m => m.ProductName == "Microfibra" && m.Quantity == 2);
        dto.Steps.Should().Equal("Lavar", "Polir");
        repo.Verify(r => r.GetAllAsync(1, 20, "POL", "polimento", "premium", true), Times.Once);
    }

    [Fact]
    public async Task GetAllClientsHandler_ShouldPassReactFiltersAndMapOrderMetrics()
    {
        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = "Joao Silva",
            Document = "12345678909",
            Tier = "gold",
            Birthday = new DateOnly(1985, 3, 14),
            AddressText = "Sao Paulo, SP",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Vehicles = [new Vehicle { Id = Guid.NewGuid(), LicensePlate = "ABC1D23", Make = "Honda", Model = "Civic" }],
            ServiceOrders =
            [
                new ServiceOrder
                {
                    Id = Guid.NewGuid(),
                    OrderNumber = "OS-00001",
                    Status = OrderStatus.Completed,
                    TotalPrice = 500
                }
            ]
        };
        var repo = new Mock<IClientRepository>();
        repo.Setup(r => r.GetAllAsync(1, 20, false, "Joao", "gold", 3))
            .ReturnsAsync(([client], 1));

        var handler = new GetAllClientsHandler(repo.Object);
        var result = await handler.Handle(new GetAllClientsQuery(1, 20, false, "Joao", "gold", 3), CancellationToken.None);

        var dto = result.Value!.Data.Single();
        dto.Tier.Should().Be("gold");
        dto.Birthday.Should().Be(new DateOnly(1985, 3, 14));
        dto.OrderCount.Should().Be(1);
        dto.Ltv.Should().Be(500);
        repo.Verify(r => r.GetAllAsync(1, 20, false, "Joao", "gold", 3), Times.Once);
    }
}
