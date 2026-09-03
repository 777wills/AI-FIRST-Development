// Inicio código generado por GitHub Copilot
using Mapster;
using Olimpia.Application.Products.Queries;

namespace Olimpia.Application.Products.Mappings;

/// <summary>
/// Configuración de mapeo Mapster para la entidad Product.
/// Esta clase es descubierta automáticamente por TypeAdapterConfig.GlobalSettings.Scan().
/// </summary>
public sealed class ProductMappingConfig : IRegister
{
    // Método generado por GitHub Copilot
    public void Register(TypeAdapterConfig config)
    {
        // Mapeo de entidad a DTO de consulta.
        config.NewConfig<global::Olimpia.Domain.Entities.Product, ProductDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Price, src => src.Price)
            .Map(dest => dest.Stock, src => src.Stock)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt);
    }
}
// Fin código generado por GitHub Copilot
