// Inicio código generado por GitHub Copilot
namespace Olimpia.Domain.Repositories;

/// <summary>
/// Contrato para consultar vistas de SQL Server.
/// No está ligado a ninguna entidad específica; opera sobre cualquier vista del esquema.
/// </summary>
/// <remarks>
/// Los parámetros se pasan como un objeto anónimo para filtros dinámicos:
/// <code>
/// // Consulta simple de una vista
/// var productos = await _view.QueryAsync&lt;ProductoDto&gt;("vw_ProductosActivos");
///
/// // Consulta con filtros (WHERE agregado dinámicamente con SqlKata)
/// var productos = await _view.QueryAsync&lt;ProductoDto&gt;("vw_ProductosActivos", new { CategoriaId = 5 });
/// </code>
/// </remarks>
public interface IViewRepository
{
    // Método generado por GitHub Copilot
    /// <summary>
    /// Consulta una vista de base de datos y devuelve una colección de <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Tipo al que se mapea cada fila del resultado.</typeparam>
    /// <param name="viewName">Nombre de la vista, por ejemplo <c>"vw_ProductosActivos"</c>.</param>
    /// <param name="filters">Filtros opcionales que se aplicarán como cláusulas WHERE. Opcional.</param>
    /// <returns>Colección (puede estar vacía) de <typeparamref name="T"/>.</returns>
    Task<IEnumerable<T>> QueryAsync<T>(string viewName, object? filters = null);

    // Método generado por GitHub Copilot
    /// <summary>
    /// Consulta una vista de base de datos y devuelve la primera fila como <typeparamref name="T"/>,
    /// o <c>null</c> si el resultado está vacío.
    /// </summary>
    /// <typeparam name="T">Tipo al que se mapea la primera fila del resultado.</typeparam>
    /// <param name="viewName">Nombre de la vista.</param>
    /// <param name="filters">Filtros opcionales que se aplicarán como cláusulas WHERE. Opcional.</param>
    /// <returns>Primera fila mapeada o <c>default(T)</c> si no hay filas.</returns>
    Task<T?> QuerySingleAsync<T>(string viewName, object? filters = null);

    // Método generado por GitHub Copilot
    /// <summary>
    /// Consulta una vista de base de datos con paginación.
    /// </summary>
    /// <typeparam name="T">Tipo al que se mapea cada fila del resultado.</typeparam>
    /// <param name="viewName">Nombre de la vista.</param>
    /// <param name="pageNumber">Número de página (base 1).</param>
    /// <param name="pageSize">Tamaño de la página.</param>
    /// <param name="filters">Filtros opcionales que se aplicarán como cláusulas WHERE. Opcional.</param>
    /// <returns>Colección paginada de <typeparamref name="T"/>.</returns>
    Task<IEnumerable<T>> QueryPagedAsync<T>(string viewName, int pageNumber, int pageSize, object? filters = null);
}
// Fin código generado por GitHub Copilot
