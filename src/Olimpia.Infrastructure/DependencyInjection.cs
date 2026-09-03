// Inicio código generado por GitHub Copilot
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Olimpia.Application.Contracts;
using Olimpia.Domain.Repositories;
using Olimpia.Infrastructure.Configuration;
using Olimpia.Infrastructure.Http;
using Olimpia.Infrastructure.Persistence;
using Olimpia.Infrastructure.Persistence.Decorators;
using Olimpia.Infrastructure.Persistence.Repositories;

namespace Olimpia.Infrastructure;

public static class DependencyInjection
{
    // Método generado por GitHub Copilot
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {        ArgumentNullException.ThrowIfNull(configuration);
        // ── Configuración de Opciones ──────────────────────────────────────────
        // Inicio código generado por GitHub Copilot
        // Registrar opciones de retry para repositorios y clientes HTTP
        services.Configure<RepositoryRetryOptions>(configuration.GetSection("Repository"));
        services.Configure<HttpClientRetryOptions>(configuration.GetSection("HttpClient"));
        // Fin código generado por GitHub Copilot

        // ── Base de datos ──────────────────────────────────────────────────────
        // Inicio refactorización/optimización por GitHub Copilot
        // UnitOfWork recibe la cadena de conexión directamente; la conexión se abre de forma lazy.
        // QueryFactory ya no se registra en DI — cada repositorio lo crea internamente.
        services.AddScoped<IUnitOfWork>(_ =>
            new UnitOfWork(configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection no configurada.")));
        // Fin refactorización/optimización por GitHub Copilot

        // Auto-registro: detecta todas las interfaces de Olimpia.Domain.Repositories
        // y registra sus implementaciones concretas sin necesidad de líneas manuales.
        RegisterRepositories(services);

        // Inicio código generado por GitHub Copilot
        // Aplicar decoradores de reintentos con Polly a los repositorios
        bool retryEnabled = configuration.GetValue<bool>("Repository:RetryEnabled", true);
        if (retryEnabled)
        {
            DecorateRepositoriesWithRetry(services);
        }
        // Fin código generado por GitHub Copilot

        // ── Redis Cache ────────────────────────────────────────────────────────
        // Inicio código generado por GitHub Copilot
        bool redisCacheEnabled = configuration.GetValue<bool>("RedisCache:Enabled", false);
        if (redisCacheEnabled)
        {
            var connectionString = configuration.GetValue<string>("RedisCache:ConnectionString");
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = connectionString;
                    options.InstanceName = configuration.GetValue<string>("RedisCache:InstanceName", "OlimpiaPrefix_");
                });
            }
        }
        // Fin código generado por GitHub Copilot

        // ── HTTP / Token Propagation ───────────────────────────────────────────
        // Inicio código generado por GitHub Copilot
        services.AddHttpContextAccessor();
        services.AddTransient<BearerTokenPropagationHandler>();

        // Registrar PollyRetryHandler para reintentos HTTP
        bool httpRetryEnabled = configuration.GetValue("HttpClient:RetryEnabled", true);
        if (httpRetryEnabled)
        {
            services.AddTransient<PollyRetryHandler>();
        }

        // Registrar IExternalApiClient con handlers (retry y token propagation)
        var clientBuilder = services.AddHttpClient<IExternalApiClient, ExternalApiClient>();

        if (httpRetryEnabled)
        {
            clientBuilder.AddHttpMessageHandler<PollyRetryHandler>();
        }

        clientBuilder.AddHttpMessageHandler<BearerTokenPropagationHandler>();

        RegisterNamedExternalClients(services, configuration, httpRetryEnabled);

        // ── Logging de Requests ────────────────────────────────────────────────
        // Inicio código generado por GitHub Copilot
        // Registrar servicio de logging de requests (siempre disponible)
        // Nota: AddLoggingInfrastructure se invoca en Program.cs (no duplicar aquí)
        services.AddScoped<IRequestLoggingService, RequestLoggingService>();
        // Fin código generado por GitHub Copilot

        return services;
    }

    // Método generado por GitHub Copilot
    /// <summary>
    /// Escanea los ensamblados de Domain e Infrastructure y registra automáticamente
    /// todos los repositorios concretos como servicios Scoped.
    ///
    /// Reglas de descubrimiento:
    /// <list type="bullet">
    ///   <item>Interfaces en el namespace <c>Olimpia.Domain.Repositories</c>.</item>
    ///   <item>Excluye <see cref="IUnitOfWork"/> y <see cref="IGenericRepository{T}"/>.</item>
    ///   <item>Primera clase concreta del ensamblado de Infrastructure que la implemente.</item>
    /// </list>
    ///
    /// Para añadir un nuevo repositorio basta con declarar su interfaz en Domain
    /// y crear su implementación en Infrastructure (heredando de <see cref="GenericRepository{T}"/>).
    /// </summary>
    private static void RegisterRepositories(IServiceCollection services)
    {
        // Inicio código generado por GitHub Copilot
        var domainAssembly = typeof(IGenericRepository<>).Assembly;
        var infraAssembly  = typeof(UnitOfWork).Assembly;
        var genericRepoDef = typeof(IGenericRepository<>);

        var repoInterfaces = domainAssembly.GetTypes()
            .Where(t => t.IsInterface
                     && t.Namespace == "Olimpia.Domain.Repositories"
                     && t != typeof(IUnitOfWork)
                     && !(t.IsGenericTypeDefinition && t.GetGenericTypeDefinition() == genericRepoDef));

        foreach (var repoInterface in repoInterfaces)
        {
            var impl = infraAssembly.GetTypes()
                .FirstOrDefault(t => t.IsClass && !t.IsAbstract && repoInterface.IsAssignableFrom(t));

            if (impl is not null)
                services.TryAddScoped(repoInterface, impl);
        }
        // Fin código generado por GitHub Copilot
    }

    // Método generado por GitHub Copilot
    /// <summary>
    /// Lee la sección <c>"ExternalApis"</c> de la configuración y registra un
    /// <see cref="HttpClient"/> con nombre por cada entrada encontrada, adjuntando
    /// <see cref="BearerTokenPropagationHandler"/> para el relay automático del token.
    /// </summary>
    private static void RegisterNamedExternalClients(
        IServiceCollection services,
        IConfiguration configuration,
        bool httpRetryEnabled)
    {
        // Inicio código generado por GitHub Copilot
        var section = configuration.GetSection("ExternalApis");
        if (!section.Exists()) return;

        foreach (var child in section.GetChildren())
        {
            var clientName = child.Key;
            var baseUrl    = child["BaseUrl"];

            var clientBuilder = services.AddHttpClient(clientName, client =>
            {
                if (!string.IsNullOrWhiteSpace(baseUrl))
                    client.BaseAddress = new Uri(baseUrl);
            });

            // Aplicar PollyRetryHandler primero (si está habilitado)
            if (httpRetryEnabled)
            {
                clientBuilder.AddHttpMessageHandler<PollyRetryHandler>();
            }

            // Luego BearerTokenPropagationHandler (solo si el cliente lo requiere)
            var propagateToken = bool.TryParse(child["PropagateToken"], out var propagate) && propagate;
            if (propagateToken)
            {
                clientBuilder.AddHttpMessageHandler<BearerTokenPropagationHandler>();
            }
        }
        // Fin código generado por GitHub Copilot
    }

    // Método generado por GitHub Copilot
    /// <summary>
    /// Decora todos los repositorios con el patrón Retry usando Polly.
    /// Aplica GenericRepositoryRetryDecorator a todos los IGenericRepository,
    /// StoredProcedureRetryDecorator a IStoredProcedureRepository,
    /// y ViewRepositoryRetryDecorator a IViewRepository.
    /// Los decoradores reciben RepositoryRetryOptions desde la configuración.
    /// </summary>
    private static void DecorateRepositoriesWithRetry(IServiceCollection services)
    {
        // Inicio código generado por GitHub Copilot
        // Decorar IStoredProcedureRepository
        services.Decorate<IStoredProcedureRepository>((inner, sp) =>
        {
            var logger = sp.GetRequiredService<ILogger<StoredProcedureRetryDecorator>>();
            var options = sp.GetRequiredService<IOptions<RepositoryRetryOptions>>();
            return new StoredProcedureRetryDecorator(inner, logger, options);
        });

        // Decorar IViewRepository
        services.Decorate<IViewRepository>((inner, sp) =>
        {
            var logger = sp.GetRequiredService<ILogger<ViewRepositoryRetryDecorator>>();
            var options = sp.GetRequiredService<IOptions<RepositoryRetryOptions>>();
            return new ViewRepositoryRetryDecorator(inner, logger, options);
        });

        // Decorar todos los IGenericRepository<T> concretos
        var genericRepoType = typeof(IGenericRepository<>);
        var decoratorType = typeof(GenericRepositoryRetryDecorator<>);

        var registeredServices = services
            .Where(sd => sd.ServiceType.IsGenericType &&
                        sd.ServiceType.GetGenericTypeDefinition() == genericRepoType)
            .ToList();

        foreach (var serviceDescriptor in registeredServices)
        {
            var entityType = serviceDescriptor.ServiceType.GetGenericArguments()[0];
            var concreteDecoratorType = decoratorType.MakeGenericType(entityType);

            services.Decorate(serviceDescriptor.ServiceType, (inner, sp) =>
            {
                var loggerType = typeof(ILogger<>).MakeGenericType(concreteDecoratorType);
                var logger = sp.GetRequiredService(loggerType);
                var options = sp.GetRequiredService<IOptions<RepositoryRetryOptions>>();
                return Activator.CreateInstance(concreteDecoratorType, inner, logger, options)!;
            });
        }
        // Fin código generado por GitHub Copilot
    }
}
// Fin código generado por GitHub Copilot

