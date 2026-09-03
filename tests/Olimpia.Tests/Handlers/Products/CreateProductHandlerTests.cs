// Inicio código generado por GitHub Copilot
using FluentAssertions;
using Moq;
using Olimpia.Application.Products.Commands.CreateProduct;
using Olimpia.Domain.Repositories;

namespace Olimpia.Tests.Handlers.Products;

[TestClass]
public sealed class CreateProductHandlerTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateProductHandler _handler;

    // Método generado por GitHub Copilot
    public CreateProductHandlerTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateProductHandler(_productRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    // Método generado por GitHub Copilot
    [TestMethod]
    public async Task Handle_Should_ReturnId_WhenProductIsValid()
    {
        // Arrange
        var command = new CreateProductCommand("Test Product", "Description", 100, 10);

        _productRepositoryMock.Setup(x => x.GetByNameAsync(command.Name))
            .ReturnsAsync((global::Olimpia.Domain.Entities.Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);
        _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<global::Olimpia.Domain.Entities.Product>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Once);
    }

    // Método generado por GitHub Copilot
    [TestMethod]
    public async Task Handle_Should_ThrowInvalidOperationException_WhenProductAlreadyExists()
    {
        // Arrange
        var command = new CreateProductCommand("Existing Product", "Description", 100, 10);

        _productRepositoryMock.Setup(x => x.GetByNameAsync(command.Name))
            .ReturnsAsync(new global::Olimpia.Domain.Entities.Product());

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("El producto ya existe.");
        _productRepositoryMock.Verify(x => x.AddAsync(It.IsAny<global::Olimpia.Domain.Entities.Product>()), Times.Never);
    }
}
// Fin código generado por GitHub Copilot
