# 📊 Acceso a Datos - Olimpia API

Documentación completa de la capa de acceso a datos usando **Dapper**, **SqlKata** y el patrón **Repository**.

---

## Tecnología Stack

| Herramienta | Versión | Propósito |
|-------------|---------|----------|
| `Dapper` | latest | Micro-ORM ligero para mapeo O/R |
| `SqlKata` | 4.0.1 | Query Builder fluido (sin SQL crudo) |
| `SqlKata.Execution` | 4.0.1 | Extensiones Dapper para SqlKata |
| `Microsoft.Data.SqlClient` | latest | Driver SQL Server |

---

## 1. QueryFactory y UnitOfWork

### QueryFactory

`QueryFactory` es la puerta de entrada a las consultas con SqlKata. **Se registra como Scoped**, compartiendo la misma conexión que `UnitOfWork`.

```csharp
// Olimpia.Infrastructure/Persistence/UnitOfWork.cs
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly SqlConnection _connection;
    public QueryFactory Db { get; private set; }

    public SqlConnection DbConnection => _connection;
    public SqlTransaction? DbTransaction { get; private set; }

    public UnitOfWork(IConfiguration configuration)
    {
        var connString = configuration.GetConnectionString("DefaultConnection");
        _connection = new SqlConnection(connString);
    }

    public async Task BeginTransactionAsync()
    {
        if (_connection.State == ConnectionState.Closed)
            await _connection.OpenAsync();

        DbTransaction = _connection.BeginTransaction();
        
        // QueryFactory usa el mismo DbTransaction
        Db = new QueryFactory(_connection, new SqlServerCompiler());
    }

    public async Task CommitAsync()
    {
        try
        {
            DbTransaction?.Commit();
        }
        finally
        {
            DbTransaction?.Dispose();
            DbTransaction = null;
            if (_connection.State == ConnectionState.Open)
                _connection.Close();
        }
    }

    public async Task RollbackAsync()
    {
        try
        {
            DbTransaction?.Rollback();
        }
        finally
        {
            DbTransaction?.Dispose();
            DbTransaction = null;
            if (_connection.State == ConnectionState.Open)
                _connection.Close();
        }
    }
}

// Registración en DependencyInjection.cs
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped(provider =>
{
    var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
    var connString = configuration.GetConnectionString("DefaultConnection");
    var connection = new SqlConnection(connString);
    return new QueryFactory(connection, new SqlServerCompiler());
});
```

---

## 2. SqlKata - Query Builder Fluido

### Inserción (INSERT)

```csharp
// Inicio código generado por GitHub Copilot

// Insertar simple
var id = await Db.Query("Products")
    .InsertGetIdAsync<int>(new
    {
        Name = "Laptop",
        Price = 1500m,
        CreatedAt = DateTime.UtcNow
    }, transaction: _unitOfWork.DbTransaction);

// Insertar múltiples
var ids = await Db.Query("Products")
    .InsertAsync(new[]
    {
        new { Name = "Product 1", Price = 100m },
        new { Name = "Product 2", Price = 200m }
    }, transaction: _unitOfWork.DbTransaction);

// Fin código generado por GitHub Copilot
```

**SQL generado:**
```sql
INSERT INTO Products (Name, Price, CreatedAt) VALUES ('Laptop', 1500.00, '2024-01-15T10:30:00Z')
SELECT CAST(SCOPE_IDENTITY() as int)
```

### Lectura (SELECT)

```csharp
// Obtener un registro
var product = await Db.Query("Products")
    .Where("Id", 5)
    .FirstOrDefaultAsync<Product>(transaction: _unitOfWork.DbTransaction);

// Obtener múltiples
var products = await Db.Query("Products")
    .Where("Status", "Active")
    .GetAsync<Product>(transaction: _unitOfWork.DbTransaction);

// Con ORDER BY
var products = await Db.Query("Products")
    .OrderByDesc("CreatedAt")
    .Limit(10)
    .GetAsync<Product>(transaction: _unitOfWork.DbTransaction);

// Con WHERE complejo
var products = await Db.Query("Products")
    .Where("Price", ">", 100)
    .Where("Stock", ">=", 5)
    .WhereIn("Status", new[] { "Active", "OnSale" })
    .GetAsync<Product>(transaction: _unitOfWork.DbTransaction);
```

**SQL generado:**
```sql
SELECT * FROM Products 
WHERE Price > 100 AND Stock >= 5 AND Status IN ('Active', 'OnSale')
ORDER BY CreatedAt DESC
LIMIT 10
```

### Actualización (UPDATE)

```csharp
// Inicio código generado por GitHub Copilot
var affected = await Db.Query("Products")
    .Where("Id", 5)
    .UpdateAsync(new
    {
        Name = "Updated Laptop",
        Price = 1600m,
        UpdatedAt = DateTime.UtcNow
    }, transaction: _unitOfWork.DbTransaction);

// Update condicional
var affected = await Db.Query("Products")
    .Where("Status", "Discontinued")
    .UpdateAsync(new { IsVisible = false }, transaction: _unitOfWork.DbTransaction);

// Fin código generado por GitHub Copilot
```

**SQL generado:**
```sql
UPDATE Products SET Name = 'Updated Laptop', Price = 1600.00, UpdatedAt = '2024-01-15T10:35:00Z'
WHERE Id = 5
```

### Eliminación (DELETE)

```csharp
// Eliminar por id
var affected = await Db.Query("Products")
    .Where("Id", 5)
    .DeleteAsync(transaction: _unitOfWork.DbTransaction);

// Soft delete (marcar como inactivo)
var affected = await Db.Query("Products")
    .Where("Id", 5)
    .UpdateAsync(new { IsDeleted = true, DeletedAt = DateTime.UtcNow });

// Eliminar condicional
var affected = await Db.Query("Products")
    .Where("CreatedAt", "<", DateTime.UtcNow.AddYears(-1))
    .DeleteAsync(transaction: _unitOfWork.DbTransaction);
```

### Agregaciones

```csharp
// COUNT
var total = await Db.Query("Products")
    .Where("Status", "Active")
    .CountAsync<int>(transaction: _unitOfWork.DbTransaction);

// SUM
var totalRevenue = await Db.Query("Orders")
    .SelectRaw("SUM(Total) as TotalRevenue")
    .FirstOrDefaultAsync<dynamic>(transaction: _unitOfWork.DbTransaction);

// AVG
var avgPrice = await Db.Query("Products")
    .SelectRaw("AVG(Price) as AveragePrice")
    .FirstOrDefaultAsync<dynamic>(transaction: _unitOfWork.DbTransaction);
```

### JOINS

```csharp
// Inicio código generado por GitHub Copilot
var results = await Db.Query("Products as p")
    .Join("Categories as c", "p.CategoryId", "c.Id")
    .Select("p.Id", "p.Name", "c.CategoryName")
    .Where("p.Status", "Active")
    .GetAsync<ProductWithCategoryDto>(transaction: _unitOfWork.DbTransaction);

// LEFT JOIN
var results = await Db.Query("Orders as o")
    .LeftJoin("Customers as c", "o.CustomerId", "c.Id")
    .Select("o.Id", "o.OrderDate", "c.Name as CustomerName")
    .GetAsync<OrderDto>(transaction: _unitOfWork.DbTransaction);

// Fin código generado por GitHub Copilot
```

---

## 3. GenericRepository<T>

### Métodos CRUD Automáticos

```csharp
// Interfaz en Domain
public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<int> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<(IEnumerable<T> Data, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        IReadOnlyList<FilterCriteria>? filters,
        IReadOnlyList<SortCriteria>? sortFields);
}

// Implementación base en Infrastructure
public abstract class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly QueryFactory Db;
    protected readonly UnitOfWork UnitOfWork;
    protected virtual string TableName => typeof(T).Name + "s";

    public virtual async Task<T?> GetByIdAsync(int id) =>
        await Db.Query(TableName)
            .Where("Id", id)
            .FirstOrDefaultAsync<T>(transaction: UnitOfWork.DbTransaction);

    public virtual async Task<IEnumerable<T>> GetAllAsync() =>
        await Db.Query(TableName)
            .GetAsync<T>(transaction: UnitOfWork.DbTransaction);

    public virtual async Task<int> AddAsync(T entity)
    {
        var id = await Db.Query(TableName)
            .InsertGetIdAsync<int>(
                BuildInsertData(entity),
                transaction: UnitOfWork.DbTransaction);
        entity.Id = id;
        return id;
    }

    public virtual async Task<bool> UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        var affected = await Db.Query(TableName)
            .Where("Id", entity.Id)
            .UpdateAsync(BuildUpdateData(entity), transaction: UnitOfWork.DbTransaction);
        return affected > 0;
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        var affected = await Db.Query(TableName)
            .Where("Id", id)
            .DeleteAsync(transaction: UnitOfWork.DbTransaction);
        return affected > 0;
    }

    // Helpers privados via reflexión
    protected virtual Dictionary<string, object?> BuildInsertData(T entity)
    {
        var data = new Dictionary<string, object?>();
        foreach (var prop in typeof(T).GetProperties())
        {
            if (prop.Name == "Id" || prop.Name == "UpdatedAt") continue;
            data[prop.Name] = prop.GetValue(entity);
        }
        return data;
    }

    protected virtual Dictionary<string, object?> BuildUpdateData(T entity)
    {
        var data = new Dictionary<string, object?>();
        foreach (var prop in typeof(T).GetProperties())
        {
            if (prop.Name == "Id" || prop.Name == "CreatedAt") continue;
            data[prop.Name] = prop.GetValue(entity);
        }
        return data;
    }
}
```

### Ejemplo de Repositorio Concreto

```csharp
// IProductRepository en Domain
public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetByNameAsync(string name);
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
    Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);
}

// ProductRepository en Infrastructure
public sealed class ProductRepository : GenericRepository<Product>, IProductRepository
{
    // TableName = "Products" por convención (no sobreescribir si es correcto)
    public ProductRepository(QueryFactory db, UnitOfWork unitOfWork) : base(db, unitOfWork) { }

    public async Task<Product?> GetByNameAsync(string name) =>
        await Db.Query(TableName)
            .Where("Name", name)
            .FirstOrDefaultAsync<Product>(transaction: UnitOfWork.DbTransaction);

    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId) =>
        await Db.Query(TableName)
            .Where("CategoryId", categoryId)
            .GetAsync<Product>(transaction: UnitOfWork.DbTransaction);

    public async Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice) =>
        await Db.Query(TableName)
            .WhereBetween("Price", minPrice, maxPrice)
            .GetAsync<Product>(transaction: UnitOfWork.DbTransaction);
}
```

### Uso en Handlers

```csharp
public sealed class CreateProductHandler : ICommandHandler<CreateProductCommand, int>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<int> Handle(CreateProductCommand command, CancellationToken ct)
    {
        // Usar métodos CRUD base
        var product = new Product { Name = command.Name, Price = command.Price };
        var id = await _repository.AddAsync(product);
        
        // O usar métodos específicos del repositorio
        var existing = await _repository.GetByNameAsync(command.Name);
        if (existing != null)
            throw new InvalidOperationException("Producto ya existe");

        await _unitOfWork.CommitAsync();
        return id;
    }
}
```

---

### Paginación con GetPagedAsync

`GetPagedAsync` ejecuta dos consultas SQL separadas: `COUNT` (total de registros) y `DATA` (página solicitada con OFFSET/FETCH). Los filtros y el ordenamiento se aplican en ambas.

#### Tipos del Dominio (Domain/Common/)

| Tipo | Kind | Descripción |
|------|------|-------------|
| `FilterOperator` | `enum` | `Eq`, `Neq`, `Gt`, `Gte`, `Lt`, `Lte`, `Contains` |
| `FilterCriteria` | `sealed record` | `(string Field, FilterOperator Operator, string Value)` |
| `SortCriteria` | `sealed record` | `(string Field, bool Descending)` |

#### Implementación en Infrastructure

```csharp
public virtual async Task<(IEnumerable<T> Data, int TotalCount)> GetPagedAsync(
    int pageNumber,
    int pageSize,
    IReadOnlyList<FilterCriteria>? filters,
    IReadOnlyList<SortCriteria>? sortFields)
{
    // Consulta COUNT (aplica mismos filtros, sin paginación)
    var countQuery = Db.Query(TableName);
    if (filters != null)
        foreach (var f in filters)
            countQuery = ApplyFilter(countQuery, f);
    var totalCount = await countQuery.CountAsync<int>(transaction: UnitOfWork.DbTransaction);

    // Consulta DATA (filtros + orden + OFFSET/FETCH)
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
```

#### Helper ApplyFilter

`ApplyFilter` traduce un `FilterCriteria` a la API fluida de SqlKata. Para `Contains` escapa `%` y `_` antes de aplicar `WhereLike`.

```csharp
private static SqlKata.Query ApplyFilter(SqlKata.Query query, FilterCriteria filter)
    => filter.Operator switch
    {
        FilterOperator.Eq       => query.Where(filter.Field, filter.Value),
        FilterOperator.Neq      => query.WhereNot(filter.Field, filter.Value),
        FilterOperator.Gt       => query.Where(filter.Field, ">", filter.Value),
        FilterOperator.Gte      => query.Where(filter.Field, ">=", filter.Value),
        FilterOperator.Lt       => query.Where(filter.Field, "<", filter.Value),
        FilterOperator.Lte      => query.Where(filter.Field, "<=", filter.Value),
        FilterOperator.Contains => query.WhereLike(
            filter.Field,
            $"%{filter.Value.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_")}%"),
        _                       => query
    };
```

#### QueryStringFilterParser (Api/Extensions/)

Convierte la query string HTTP al modelo de dominio antes de construir la Query CQRS:

```
GET /api/v1/products?name[contains]=Laptop&price[gte]=100&sort=name,-price&pageNumber=1&pageSize=25
```

```csharp
var (filters, sortFields, pageNumber, pageSize) = QueryStringFilterParser.Parse(HttpContext.Request.Query);
var query = new GetAllProductsQuery(pageNumber, pageSize, filters, sortFields);
```

| Parámetro | Formato | Ejemplo |
|-----------|---------|---------|
| Filtro | `campo[operador]=valor` | `name[contains]=Laptop` |
| Orden | `sort=campo1,-campo2` (prefijo `-` = DESC) | `sort=name,-price` |
| Página | `pageNumber=N&pageSize=N` | `pageNumber=2&pageSize=10` |

---

## 4. Stored Procedures

### Interfaz

```csharp
public interface IStoredProcedureRepository
{
    Task<int> ExecuteAsync(string procedureName, object? parameters = null);
    Task<IEnumerable<T>> QueryAsync<T>(string procedureName, object? parameters = null);
    Task<T?> QuerySingleAsync<T>(string procedureName, object? parameters = null);
}
```

### Implementación

```csharp
public sealed class StoredProcedureRepository : IStoredProcedureRepository
{
    private readonly QueryFactory _db;
    private readonly IUnitOfWork _unitOfWork;

    public StoredProcedureRepository(QueryFactory db, IUnitOfWork unitOfWork)
    {
        _db = db;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> ExecuteAsync(string procedureName, object? parameters = null)
    {
        using var connection = _db.Connection;
        if (connection.State == ConnectionState.Closed)
            await connection.OpenAsync();

        return await connection.ExecuteAsync(
            procedureName,
            parameters,
            transaction: _unitOfWork.DbTransaction,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string procedureName, object? parameters = null)
    {
        using var connection = _db.Connection;
        if (connection.State == ConnectionState.Closed)
            await connection.OpenAsync();

        return await connection.QueryAsync<T>(
            procedureName,
            parameters,
            transaction: _unitOfWork.DbTransaction,
            commandType: CommandType.StoredProcedure);
    }

    public async Task<T?> QuerySingleAsync<T>(string procedureName, object? parameters = null)
    {
        using var connection = _db.Connection;
        if (connection.State == ConnectionState.Closed)
            await connection.OpenAsync();

        return await connection.QueryFirstOrDefaultAsync<T>(
            procedureName,
            parameters,
            transaction: _unitOfWork.DbTransaction,
            commandType: CommandType.StoredProcedure);
    }
}
```

### Uso en Handlers

```csharp
// Parámetros simples
var items = await _sp.QueryAsync<SalesItemDto>("usp_GetSalesByMonth", 
    new { Month = 1, Year = 2024 });

// Parámetros con OUTPUT
var dp = new DynamicParameters();
dp.Add("@Total", dbType: DbType.Decimal, direction: ParameterDirection.Output);
dp.Add("@Count", dbType: DbType.Int32, direction: ParameterDirection.Output);
await _sp.ExecuteAsync("usp_CalculateStats", dp);

var total = dp.Get<decimal>("@Total");
var count = dp.Get<int>("@Count");
```

---

## 5. Views (Vistas de Base de Datos)

### Interfaz

```csharp
public interface IViewRepository
{
    Task<IEnumerable<T>> QueryAsync<T>(string viewName, object? filters = null);
    Task<T?> QuerySingleAsync<T>(string viewName, object? filters = null);
    Task<IEnumerable<T>> QueryPagedAsync<T>(string viewName, int pageNumber, int pageSize, object? filters = null);
}
```

### Implementación

```csharp
public sealed class ViewRepository : IViewRepository
{
    private readonly QueryFactory _db;
    private readonly IUnitOfWork _unitOfWork;

    public ViewRepository(QueryFactory db, IUnitOfWork unitOfWork)
    {
        _db = db;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string viewName, object? filters = null)
    {
        var query = _db.Query(viewName);
        
        if (filters != null)
        {
            foreach (var prop in filters.GetType().GetProperties())
            {
                query = query.Where(prop.Name, prop.GetValue(filters));
            }
        }

        return await query.GetAsync<T>(transaction: _unitOfWork.DbTransaction);
    }

    public async Task<T?> QuerySingleAsync<T>(string viewName, object? filters = null)
    {
        var query = _db.Query(viewName);
        
        if (filters != null)
        {
            foreach (var prop in filters.GetType().GetProperties())
            {
                query = query.Where(prop.Name, prop.GetValue(filters));
            }
        }

        return await query.FirstOrDefaultAsync<T>(transaction: _unitOfWork.DbTransaction);
    }

    public async Task<IEnumerable<T>> QueryPagedAsync<T>(
        string viewName, 
        int pageNumber, 
        int pageSize, 
        object? filters = null)
    {
        var query = _db.Query(viewName);
        
        if (filters != null)
        {
            foreach (var prop in filters.GetType().GetProperties())
            {
                query = query.Where(prop.Name, prop.GetValue(filters));
            }
        }

        var skip = (pageNumber - 1) * pageSize;
        return await query
            .Skip(skip)
            .Limit(pageSize)
            .GetAsync<T>(transaction: _unitOfWork.DbTransaction);
    }
}
```

### Uso

```csharp
// Consulta simple
var stats = await _view.QueryAsync<ProductStatsDto>("vw_ProductStats");

// Con filtros
var byCategory = await _view.QueryAsync<ProductStatsDto>("vw_ProductStats",
    new { CategoryId = 5 });

// Paginada
var page = await _view.QueryPagedAsync<ProductStatsDto>(
    "vw_ProductStats",
    pageNumber: 1,
    pageSize: 20,
    filters: new { Status = "Active" });
```

---

## 6. Transacciones

### Dentro de un Handler

```csharp
public sealed class TransferInventoryHandler : ICommandHandler<TransferInventoryCommand, bool>
{
    private readonly IProductRepository _productRepo;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<bool> Handle(TransferInventoryCommand command, CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Restar stock del origen
            var source = await _productRepo.GetByIdAsync(command.SourceProductId);
            if (source.Stock < command.Quantity)
                throw new InvalidOperationException("Stock insuficiente");

            source.Stock -= command.Quantity;
            await _productRepo.UpdateAsync(source);

            // Sumar stock al destino
            var dest = await _productRepo.GetByIdAsync(command.DestProductId);
            dest.Stock += command.Quantity;
            await _productRepo.UpdateAsync(dest);

            // Commit
            await _unitOfWork.CommitAsync();
            return true;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}
```

---

## Próximos Pasos

- **[PATTERNS.md](PATTERNS.md)** - Patrones CQRS y Repository
- **[RESILIENCE.md](RESILIENCE.md)** - Reintentos HTTP
