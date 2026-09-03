---
name: stored-procedures-views
description: "Trabajar con Stored Procedures y Views de base de datos usando IStoredProcedureRepository e IViewRepository. Parámetros, OUTPUT, paginación y filtros."
---

# Skill: Stored Procedures y Views

Patrones avanzados para SPs y vistas. Métodos disponibles y reglas básicas: ver `data-access.instructions.md`.

---

## Parámetros OUTPUT / RETURN — DynamicParameters

Usar `DynamicParameters` de Dapper cuando el SP tiene parámetros de salida:

```csharp
using Dapper;

var dp = new DynamicParameters();
dp.Add("@OrdenId",   command.OrdenId);
dp.Add("@NuevoId",   dbType: DbType.Int32, direction: ParameterDirection.Output);
dp.Add("@Resultado", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);

await _sp.ExecuteAsync("usp_ProcesarOrden", dp);

int nuevoId     = dp.Get<int>("@NuevoId");
string resultado = dp.Get<string>("@Resultado");
```

### RETURN VALUE

```csharp
var dp = new DynamicParameters();
dp.Add("@OrdenId", command.OrdenId);
dp.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

await _sp.ExecuteAsync("usp_ValidarOrden", dp);
int returnCode = dp.Get<int>("@ReturnValue"); // 0 = éxito, -1 = error
```

---

## SP en Handler (con transacción)

```csharp
public sealed class ProcessOrderHandler : ICommandHandler<ProcessOrderCommand, ProcessResult>
{
    private readonly IStoredProcedureRepository _sp;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessOrderHandler(IStoredProcedureRepository sp, IUnitOfWork unitOfWork)
    {
        _sp         = sp;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProcessResult> Handle(ProcessOrderCommand command, CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var dp = new DynamicParameters();
            dp.Add("@OrdenId", command.OrderId);
            dp.Add("@Status",  dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

            await _sp.ExecuteAsync("usp_ProcessOrder", dp);
            await _unitOfWork.CommitAsync();

            return new ProcessResult(dp.Get<string>("@Status"));
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

## Vista con paginación y filtros

```csharp
public sealed class GetActiveProductsHandler : IQueryHandler<GetActiveProductsQuery, IEnumerable<ProductoDto>>
{
    private readonly IViewRepository _view;

    public GetActiveProductsHandler(IViewRepository view) => _view = view;

    public async Task<IEnumerable<ProductoDto>> Handle(
        GetActiveProductsQuery query, CancellationToken ct)
    {
        if (query.PageSize > 0)
        {
            return await _view.QueryPagedAsync<ProductoDto>(
                "vw_ProductosActivos",
                query.PageNumber,
                query.PageSize,
                query.CategoryId.HasValue ? new { CategoriaId = query.CategoryId } : null);
        }

        return await _view.QueryAsync<ProductoDto>("vw_ProductosActivos");
    }
}
```
