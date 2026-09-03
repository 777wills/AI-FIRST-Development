// Inicio código generado por GitHub Copilot
using Cortex.Mediator.Commands;
using Olimpia.Domain.Repositories;

namespace Olimpia.Application.Products.Commands.CreateProduct;

public sealed class CreateProductHandler : ICommandHandler<CreateProductCommand, int>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    // Método generado por GitHub Copilot
    public CreateProductHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    // Método generado por GitHub Copilot
    public async Task<int> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var existingProduct = await _productRepository.GetByNameAsync(command.Name);
        if (existingProduct is not null)
        {
            throw new InvalidOperationException("El producto ya existe.");
        }

        var product = new global::Olimpia.Domain.Entities.Product(command.Name, command.Description, command.Price, command.Stock);

        try
        {
            await _unitOfWork.BeginTransactionAsync();
            await _productRepository.AddAsync(product);
            await _unitOfWork.CommitAsync();

            return product.Id;
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}
// Fin código generado por GitHub Copilot
