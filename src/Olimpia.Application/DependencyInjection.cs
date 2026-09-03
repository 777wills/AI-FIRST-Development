// Inicio código generado por GitHub Copilot
using System.Reflection;
using FluentValidation;
using Cortex.Mediator.DependencyInjection;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Olimpia.Application;

public static class DependencyInjection
{
    // Método generado por GitHub Copilot
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddCortexMediator(new[] { typeof(DependencyInjection) });
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Inicio código generado por GitHub Copilot
        // Registrar configuraciones de Mapster definidas en este assembly (IRegister).
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        // Fin código generado por GitHub Copilot

        return services;
    }
}
// Fin código generado por GitHub Copilot