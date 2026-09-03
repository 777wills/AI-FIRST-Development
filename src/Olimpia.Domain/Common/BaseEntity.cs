// Inicio código generado por GitHub Copilot
namespace Olimpia.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}
// Fin código generado por GitHub Copilot