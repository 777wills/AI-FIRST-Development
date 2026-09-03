// Inicio código generado por GitHub Copilot
using Olimpia.Domain.Common;

namespace Olimpia.Domain.Repositories;

public interface IGenericRepository<T> where T : class
{
    // Método generado por GitHub Copilot
    Task<T?> GetByIdAsync(int id);
    // Método generado por GitHub Copilot
    Task<IEnumerable<T>> GetAllAsync();
    // Método generado por GitHub Copilot
    Task<int> AddAsync(T entity);
    // Método generado por GitHub Copilot
    Task<bool> UpdateAsync(T entity);
    // Método generado por GitHub Copilot
    Task<bool> DeleteAsync(int id);
    // Método generado por GitHub Copilot
    Task<(IEnumerable<T> Data, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        IReadOnlyList<FilterCriteria>? filters,
        IReadOnlyList<SortCriteria>? sortFields);
}
// Fin código generado por GitHub Copilot