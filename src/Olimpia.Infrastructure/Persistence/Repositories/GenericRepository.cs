// Inicio código generado por GitHub Copilot
using System.Reflection;
using Olimpia.Domain.Common;
using Olimpia.Domain.Repositories;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace Olimpia.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación genérica de <see cref="IGenericRepository{T}"/> sobre SqlKata + Dapper.
/// Proporciona operaciones CRUD estándar para cualquier entidad que herede de <see cref="BaseEntity"/>.
///
/// Cómo usar:
///   1. Crear una clase concreta que herede de <c>GenericRepository&lt;TEntity&gt;</c>.
///   2. Si el nombre de la tabla SQL difiere de "<c>NombreEntidad + s</c>", sobreescribir <see cref="TableName"/>.
///   3. Agregar en la subclase los métodos de consulta específicos del dominio.
///
/// Ejemplo:
/// <code>
/// public class OrderRepository : GenericRepository&lt;Order&gt;, IOrderRepository
/// {
///     // La convención asigna "Orders" automáticamente; no es necesario sobreescribir TableName.
///     public OrderRepository(QueryFactory db, UnitOfWork unitOfWork) : base(db, unitOfWork) { }
///
///     public async Task&lt;IEnumerable&lt;Order&gt;&gt; GetByCustomerAsync(int customerId) =>
///         await Db.Query(TableName).Where("CustomerId", customerId)
///             .GetAsync&lt;Order&gt;(transaction: UnitOfWork.DbTransaction);
/// }
/// </code>
/// </summary>
public abstract class GenericRepository<T>(IUnitOfWork unitOfWork) : IGenericRepository<T> where T : BaseEntity
{
    /// <summary>SqlKata QueryFactory compartido con el UnitOfWork activo.</summary>
    protected readonly QueryFactory Db = new QueryFactory(unitOfWork.DbConnection, new SqlServerCompiler());

    /// <summary>UnitOfWork que provee la conexión y la transacción activa.</summary>
    protected readonly IUnitOfWork UnitOfWork = unitOfWork;

    /// <summary>
    /// Nombre de la tabla SQL. Por convención es <c>typeof(T).Name + "s"</c>.
    /// Sobreescribir en la subclase si el nombre real de la tabla es diferente.
    /// </summary>
    protected virtual string TableName => typeof(T).Name + "s";

    // ── Operaciones CRUD ──────────────────────────────────────────────────────

    // Método generado por GitHub Copilot
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        // Inicio refactorización/optimización por GitHub Copilot
        await UnitOfWork.EnsureOpenAsync().ConfigureAwait(false);
        // Fin refactorización/optimización por GitHub Copilot
        return await Db.Query(TableName)
            .Where("Id", id)
            .FirstOrDefaultAsync<T>(transaction: UnitOfWork.DbTransaction);
    }

    // Método generado por GitHub Copilot
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        // Inicio refactorización/optimización por GitHub Copilot
        await UnitOfWork.EnsureOpenAsync().ConfigureAwait(false);
        // Fin refactorización/optimización por GitHub Copilot
        return await Db.Query(TableName)
            .GetAsync<T>(transaction: UnitOfWork.DbTransaction);
    }

    // Método generado por GitHub Copilot
    public virtual async Task<int> AddAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        // Inicio refactorización/optimización por GitHub Copilot
        await UnitOfWork.EnsureOpenAsync().ConfigureAwait(false);
        // Fin refactorización/optimización por GitHub Copilot
        // Inicio código generado por GitHub Copilot
        var data = BuildInsertData(entity);
        var id   = await Db.Query(TableName)
            .InsertGetIdAsync<int>(data, transaction: UnitOfWork.DbTransaction);
        entity.Id = id;
        return id;
        // Fin código generado por GitHub Copilot
    }

    // Método generado por GitHub Copilot
    public virtual async Task<bool> UpdateAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        // Inicio refactorización/optimización por GitHub Copilot
        await UnitOfWork.EnsureOpenAsync().ConfigureAwait(false);
        // Fin refactorización/optimización por GitHub Copilot
        // Inicio código generado por GitHub Copilot
        entity.UpdatedAt = DateTime.Now;
        var data     = BuildUpdateData(entity);
        var affected = await Db.Query(TableName)
            .Where("Id", entity.Id)
            .UpdateAsync(data, transaction: UnitOfWork.DbTransaction);
        return affected > 0;
        // Fin código generado por GitHub Copilot
    }

    // Método generado por GitHub Copilot
    public virtual async Task<bool> DeleteAsync(int id)
    {
        // Inicio refactorización/optimización por GitHub Copilot
        await UnitOfWork.EnsureOpenAsync().ConfigureAwait(false);
        // Fin refactorización/optimización por GitHub Copilot
        var affected = await Db.Query(TableName)
            .Where("Id", id)
            .DeleteAsync(transaction: UnitOfWork.DbTransaction);
        return affected > 0;
    }

    // ── Helpers de reflexión ──────────────────────────────────────────────────
    // Inicio código generado por GitHub Copilot

    // Propiedades que NO se envían en un INSERT (Id es identidad; UpdatedAt no aplica al crear).
    private static readonly IReadOnlySet<string> InsertExcluded =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Id", "UpdatedAt" };

    // Propiedades que NO se envían en un UPDATE (Id va en WHERE; CreatedAt no varía nunca).
    private static readonly IReadOnlySet<string> UpdateExcluded =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Id", "CreatedAt" };

    // Método generado por GitHub Copilot
    private static Dictionary<string, object?> BuildInsertData(T entity) =>
        BuildColumnMap(entity, InsertExcluded);

    // Método generado por GitHub Copilot
    private static Dictionary<string, object?> BuildUpdateData(T entity) =>
        BuildColumnMap(entity, UpdateExcluded);

    /// <summary>
    /// Convierte las propiedades públicas de una entidad en un diccionario columna→valor
    /// apto para <c>SqlKata.Execution</c>. Excluye las columnas indicadas en <paramref name="excluded"/>.
    /// </summary>
    // Método generado por GitHub Copilot
    private static Dictionary<string, object?> BuildColumnMap(T entity, IReadOnlySet<string> excluded) =>
        typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && !excluded.Contains(p.Name))
            .ToDictionary(p => p.Name, p => p.GetValue(entity));

    // Fin código generado por GitHub Copilot

    // ── Paginación ────────────────────────────────────────────────────────────

    // Inicio código generado por GitHub Copilot
    // Método generado por GitHub Copilot
    public virtual async Task<(IEnumerable<T> Data, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        IReadOnlyList<global::Olimpia.Domain.Common.FilterCriteria>? filters,
        IReadOnlyList<global::Olimpia.Domain.Common.SortCriteria>? sortFields)
    {
        // Inicio refactorización/optimización por GitHub Copilot
        await UnitOfWork.EnsureOpenAsync().ConfigureAwait(false);
        // Fin refactorización/optimización por GitHub Copilot
        // Consulta COUNT sin paginación.
        var countQuery = Db.Query(TableName);
        if (filters != null)
            foreach (var f in filters)
                countQuery = ApplyFilter(countQuery, f);
        var totalCount = await countQuery.CountAsync<int>(transaction: UnitOfWork.DbTransaction);

        // Consulta DATA con filtros, orden y paginación.
        var dataQuery = Db.Query(TableName);
        if (filters != null)
            foreach (var f in filters)
                dataQuery = ApplyFilter(dataQuery, f);
        if (sortFields != null)
            foreach (var s in sortFields)
                dataQuery = s.Descending ? dataQuery.OrderByDesc(s.Field) : dataQuery.OrderBy(s.Field);
        dataQuery = dataQuery.Offset((pageNumber - 1) * pageSize).Limit(pageSize);
        var data = await dataQuery.GetAsync<T>(transaction: UnitOfWork.DbTransaction);

        return (data, totalCount);
    }
    // Fin código generado por GitHub Copilot

    // Inicio código generado por GitHub Copilot
    // Método generado por GitHub Copilot
    private static SqlKata.Query ApplyFilter(SqlKata.Query query, global::Olimpia.Domain.Common.FilterCriteria filter)
        => filter.Operator switch
        {
            global::Olimpia.Domain.Common.FilterOperator.Eq       => query.Where(filter.Field, filter.Value),
            global::Olimpia.Domain.Common.FilterOperator.Neq      => query.WhereNot(filter.Field, filter.Value),
            global::Olimpia.Domain.Common.FilterOperator.Gt       => query.Where(filter.Field, ">", filter.Value),
            global::Olimpia.Domain.Common.FilterOperator.Gte      => query.Where(filter.Field, ">=", filter.Value),
            global::Olimpia.Domain.Common.FilterOperator.Lt       => query.Where(filter.Field, "<", filter.Value),
            global::Olimpia.Domain.Common.FilterOperator.Lte      => query.Where(filter.Field, "<=", filter.Value),
            // Inicio refactorización/optimización por GitHub Copilot
            global::Olimpia.Domain.Common.FilterOperator.Contains => query.WhereLike(
                filter.Field,
                $"%{filter.Value.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_")}%"),
            // Fin refactorización/optimización por GitHub Copilot
            _                                                      => query
        };
    // Fin código generado por GitHub Copilot
}
// Fin código generado por GitHub Copilot
