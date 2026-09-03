// Inicio código generado por GitHub Copilot
using Asp.Versioning;
using Cortex.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Olimpia.Api.Extensions;
using Olimpia.Application.Common.Responses;
using Olimpia.Application.Products;
using Olimpia.Application.Products.Commands.CreateProduct;
using Olimpia.Application.Products.Queries.GetAllProducts;
using Olimpia.Application.Products.Queries.GetProductById;

namespace Olimpia.Api.Controllers.V1;

// [Authorize] a nivel de controlador exige token válido emitido por OpenIddict.
// Cada endpoint puede requerir además un scope específico mediante [Authorize(Policy = "...")].
// Usar [AllowAnonymous] en un método para eximirlo completamente.
[Authorize]
[ApiVersion("1.0")]
public sealed class ProductController : ApiController
{
    private readonly IMediator _mediator;

    // Método generado por GitHub Copilot
    public ProductController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Crea un producto en el catálogo.
    /// </summary>
    /// <remarks>
    /// Requiere scope 'products.write'. El nombre del producto debe ser único.
    /// Las excepciones de negocio son traducidas a <c>ProblemDetails</c> por el middleware global.
    /// </remarks>
    /// <param name="command">Datos del producto a crear.</param>
    /// <returns>Identificador del producto recién creado.</returns>
    /// <response code="200">Producto creado exitosamente.</response>
    /// <response code="400">Validación fallida o argumento inválido.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">Sin scope 'products.write'.</response>
    /// <response code="409">Ya existe un producto con ese nombre.</response>
    /// <response code="500">Error interno.</response>
    [MapToApiVersion("1.0")]
    [HttpPost]
    [Authorize(Policy = "products.write")]
    [ProducesResponseType(typeof(CreateProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(CreateProductCommand command)
    {
        var id = await _mediator.SendAsync(command);
        return Ok(new CreateProductResponse(id));
    }

    /// <summary>
    /// Lista productos paginados, filtrados y ordenados.
    /// </summary>
    /// <remarks>
    /// Requiere scope 'products.read'. Soporta filtros dinámicos con sintaxis <c>campo[operador]=valor</c>
    /// (ej. <c>?name[contains]=lap&amp;price[gte]=500</c>) y ordenamiento con <c>?sort=-createdAt,name</c>
    /// (prefijo <c>-</c> para descendente).
    ///
    /// Filtros permitidos: <c>name[contains]</c>, <c>price[eq|gt|gte|lt|lte]</c>,
    /// <c>stock[eq|gt|gte|lt|lte]</c>, <c>createdAt[gt|lt]</c>.
    /// Campos para <c>sort</c>: <c>name</c>, <c>price</c>, <c>stock</c>, <c>createdAt</c>.
    /// Orden por defecto: <c>-createdAt</c>.
    /// </remarks>
    /// <param name="pageNumber">Página solicitada (1-based).</param>
    /// <param name="pageSize">Tamaño de página (máximo 100).</param>
    /// <param name="sort">Lista de campos de ordenamiento separados por coma.</param>
    /// <returns>Envelope paginado con la lista de productos y metadatos.</returns>
    /// <response code="200">Listado de productos.</response>
    /// <response code="400">Parámetros de paginación, filtros u ordenamiento inválidos.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">Sin scope 'products.read'.</response>
    /// <response code="500">Error interno.</response>
    // Inicio refactorización/optimización por GitHub Copilot
    [MapToApiVersion("1.0")]
    [HttpGet]
    [Authorize(Policy = "products.read")]
    [PaginatedEndpoint(
        AllowedFilters = "name[contains],price[eq],price[gt],price[gte],price[lt],price[lte],stock[eq],stock[gt],stock[gte],stock[lt],stock[lte],createdAt[gt],createdAt[lt]",
        AllowedSortFields = "name,price,stock,createdAt",
        DefaultSort = "-createdAt")]
    [ProducesResponseType(typeof(PagedEnvelope<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? sort = null)
    {
        var filters = QueryStringFilterParser.ParseFilters(HttpContext.Request.Query);
        var sortFields = QueryStringFilterParser.ParseSortFields(sort);
        var query = new GetAllProductsQuery(pageNumber, pageSize, filters, sortFields.Count > 0 ? sortFields : null);
        var result = await _mediator.SendQueryAsync(query);
        var envelope = PagedEnvelope<ProductDto>.FromPagedResult(result);
        return Ok(envelope);
    }
    // Fin refactorización/optimización por GitHub Copilot

    /// <summary>
    /// Obtiene un producto por su identificador.
    /// </summary>
    /// <remarks>
    /// Requiere scope 'products.read'. Si el producto no existe, el middleware traduce
    /// <c>KeyNotFoundException</c> a <c>404 NotFound</c> con <c>ProblemDetails</c>.
    /// </remarks>
    /// <param name="id">Identificador único del producto.</param>
    /// <returns>El producto solicitado.</returns>
    /// <response code="200">Producto encontrado.</response>
    /// <response code="400">Identificador inválido.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">Sin scope 'products.read'.</response>
    /// <response code="404">No existe un producto con el identificador indicado.</response>
    /// <response code="500">Error interno.</response>
    // Inicio código generado por GitHub Copilot
    [MapToApiVersion("1.0")]
    [HttpGet("{id:int}")]
    [Authorize(Policy = "products.read")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetProductByIdQuery(id);
        var result = await _mediator.SendQueryAsync(query);
        return Ok(result);
    }
    // Fin código generado por GitHub Copilot
}
// Fin código generado por GitHub Copilot
