// Inicio código generado por GitHub Copilot
namespace Olimpia.Domain.Repositories;

/// <summary>
/// Contrato para ejecutar procedimientos almacenados de SQL Server.
/// No está ligado a ninguna entidad específica; opera sobre cualquier SP del esquema.
/// </summary>
/// <remarks>
/// Los parámetros se pasan como un objeto anónimo o como <c>Dapper.DynamicParameters</c>:
/// <code>
/// // Objeto anónimo (parámetros de entrada simples)
/// var rows = await _sp.QueryAsync&lt;ProductoDto&gt;("usp_GetProductosByCategoria", new { CategoriaId = 5 });
///
/// // DynamicParameters (parámetros de salida / retorno)
/// var dp = new DynamicParameters();
/// dp.Add("@NuevoId", dbType: DbType.Int32, direction: ParameterDirection.Output);
/// await _sp.ExecuteAsync("usp_InsertarPedido", dp);
/// int nuevoId = dp.Get&lt;int&gt;("@NuevoId");
/// </code>
/// </remarks>
public interface IStoredProcedureRepository
{
    // Método generado por GitHub Copilot
    /// <summary>
    /// Ejecuta un procedimiento almacenado que no devuelve filas (INSERT / UPDATE / DELETE / lógica de negocio).
    /// </summary>
    /// <param name="procedureName">Nombre del SP, por ejemplo <c>"usp_ArchivarPedido"</c>.</param>
    /// <param name="parameters">Parámetros del SP (objeto anónimo o <c>DynamicParameters</c>). Opcional.</param>
    /// <returns>Número de filas afectadas.</returns>
    Task<int> ExecuteAsync(string procedureName, object? parameters = null);

    // Método generado por GitHub Copilot
    /// <summary>
    /// Ejecuta un procedimiento almacenado y devuelve una colección de <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Tipo al que se mapea cada fila del resultado.</typeparam>
    /// <param name="procedureName">Nombre del SP.</param>
    /// <param name="parameters">Parámetros del SP. Opcional.</param>
    /// <returns>Colección (puede estar vacía) de <typeparamref name="T"/>.</returns>
    Task<IEnumerable<T>> QueryAsync<T>(string procedureName, object? parameters = null);

    // Método generado por GitHub Copilot
    /// <summary>
    /// Ejecuta un procedimiento almacenado y devuelve la primera fila como <typeparamref name="T"/>,
    /// o <c>null</c> si el resultado está vacío.
    /// </summary>
    /// <typeparam name="T">Tipo al que se mapea la primera fila del resultado.</typeparam>
    /// <param name="procedureName">Nombre del SP.</param>
    /// <param name="parameters">Parámetros del SP. Opcional.</param>
    /// <returns>Primera fila mapeada o <c>default(T)</c> si no hay filas.</returns>
    Task<T?> QuerySingleAsync<T>(string procedureName, object? parameters = null);
}
// Fin código generado por GitHub Copilot
