// Inicio código generado por GitHub Copilot
using Cortex.Mediator;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Olimpia.Api.Controllers.V1;
using Olimpia.Application.Common.Pagination;
using Olimpia.Application.Products;
using Olimpia.Application.Products.Queries.GetProductById;

namespace Olimpia.Tests.Handlers.Products;

[TestClass]
public sealed class ProductControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ProductController _controller;

    public ProductControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new ProductController(_mediatorMock.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // Inicio refactorización/optimización por GitHub Copilot
    [TestMethod]
    public async Task GetAll_Should_ReturnOk_When_QueryIsValid()
    {
        // Arrange
        var products = new List<ProductDto>
        {
            new(1, "Laptop", "Gaming laptop", 1500m, 10, DateTime.Now, null)
        };
        var pagedResult = PagedResult<ProductDto>.Create(products, 1, 25, 1);

        _mediatorMock
            .Setup(x => x.SendQueryAsync<PagedResult<ProductDto>>(
                It.IsAny<Cortex.Mediator.Queries.IQuery<PagedResult<ProductDto>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetAll(pageNumber: 1, pageSize: 25, sort: null);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result;
        ok.StatusCode.Should().Be(200);
    }
    // Fin refactorización/optimización por GitHub Copilot

    // Inicio refactorización/optimización por GitHub Copilot
    [TestMethod]
    public async Task GetAll_Should_PropagateArgumentException_When_MediatorThrows()
    {
        // El controller ya no maneja excepciones: las propaga al ExceptionMiddleware,
        // que las mapea a 400 Bad Request con ProblemDetails tipado.

        // Arrange
        _mediatorMock
            .Setup(x => x.SendQueryAsync<PagedResult<ProductDto>>(
                It.IsAny<Cortex.Mediator.Queries.IQuery<PagedResult<ProductDto>>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Filtro inválido."));

        // Act
        Func<Task> act = async () => await _controller.GetAll(pageNumber: 1, pageSize: 25, sort: null);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("Filtro inválido.");
    }
    // Fin refactorización/optimización por GitHub Copilot

    [TestMethod]
    public async Task GetById_Should_ReturnOk_When_ProductExists()
    {
        // Arrange
        var product = new ProductDto(1, "Laptop", "Gaming laptop", 1500m, 10, DateTime.Now, null);

        _mediatorMock
            .Setup(x => x.SendQueryAsync<ProductDto>(
                It.IsAny<Cortex.Mediator.Queries.IQuery<ProductDto>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result;
        ok.StatusCode.Should().Be(200);
        ok.Value.Should().BeEquivalentTo(product);
    }

    [TestMethod]
    public async Task GetById_Should_PropagateKeyNotFoundException_When_ProductNotFound()
    {
        // El controller ya no maneja excepciones: las propaga al ExceptionMiddleware,
        // que mapea KeyNotFoundException a 404 Not Found con ProblemDetails tipado.

        // Arrange
        _mediatorMock
            .Setup(x => x.SendQueryAsync<ProductDto>(
                It.IsAny<Cortex.Mediator.Queries.IQuery<ProductDto>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException("Producto no encontrado."));

        // Act
        Func<Task> act = async () => await _controller.GetById(99);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Producto no encontrado.");
    }

    [TestMethod]
    public async Task GetById_Should_PropagateArgumentException_When_IdIsInvalid()
    {
        // El controller ya no maneja excepciones: las propaga al ExceptionMiddleware,
        // que mapea ArgumentException a 400 Bad Request con ProblemDetails tipado.

        // Arrange
        _mediatorMock
            .Setup(x => x.SendQueryAsync<ProductDto>(
                It.IsAny<Cortex.Mediator.Queries.IQuery<ProductDto>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("El Id no es válido."));

        // Act
        Func<Task> act = async () => await _controller.GetById(-1);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("El Id no es válido.");
    }
}
// Fin código generado por GitHub Copilot
