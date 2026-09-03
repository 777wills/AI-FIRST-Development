// Inicio código generado por GitHub Copilot
namespace Olimpia.Api.Extensions;

/// <summary>
/// Extensiones para cargar secretos de Docker/Kubernetes/Podman desde archivos.
/// Los secretos se montan en /run/secrets/ y se referencian con variables _File.
/// </summary>
public static class SecretsConfigurationExtensions
{
    /// <summary>
    /// Carga secretos desde archivos montados en el contenedor.
    /// Busca variables de configuración que terminen en "_File" y lee su contenido.
    /// </summary>
    /// <param name="builder">IConfigurationBuilder</param>
    /// <returns>IConfigurationBuilder para encadenar llamadas</returns>
    public static IConfigurationBuilder AddDockerSecrets(this IConfigurationBuilder builder)
    {
        var config = builder.Build();
        var secretsToAdd = new Dictionary<string, string?>();
        
        foreach (var kvp in config.AsEnumerable())
        {
            // Buscar claves que terminen en _File
            if (kvp.Key.EndsWith("_File", StringComparison.OrdinalIgnoreCase) && 
                !string.IsNullOrEmpty(kvp.Value))
            {
                try
                {
                    if (File.Exists(kvp.Value))
                    {
                        // Leer contenido del archivo secreto
                        var secretValue = File.ReadAllText(kvp.Value).Trim();
                        
                        // Remover sufijo _File para obtener la clave real
                        var actualKey = kvp.Key.Substring(0, kvp.Key.Length - 5);
                        
                        secretsToAdd[actualKey] = secretValue;
                        
                        Console.WriteLine($"✅ Secreto cargado: {actualKey} desde {kvp.Value}");
                    }
                    else
                    {
                        Console.WriteLine($"⚠️  Archivo de secreto no encontrado: {kvp.Value}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error al cargar secreto desde {kvp.Value}: {ex.Message}");
                }
            }
        }
        
        // Agregar secretos a la configuración
        if (secretsToAdd.Any())
        {
            builder.AddInMemoryCollection(secretsToAdd);
            Console.WriteLine($"✅ Total de secretos cargados: {secretsToAdd.Count}");
        }
        
        return builder;
    }
    
    /// <summary>
    /// Carga secretos desde Kubernetes montados como volúmenes.
    /// Kubernetes monta secretos en /var/run/secrets/kubernetes.io/serviceaccount/
    /// </summary>
    /// <param name="builder">IConfigurationBuilder</param>
    /// <param name="secretsPath">Ruta base de los secretos (por defecto: /run/secrets)</param>
    /// <returns>IConfigurationBuilder para encadenar llamadas</returns>
    public static IConfigurationBuilder AddKubernetesSecrets(
        this IConfigurationBuilder builder,
        string secretsPath = "/run/secrets")
    {
        if (!Directory.Exists(secretsPath))
        {
            Console.WriteLine($"⚠️  Directorio de secretos no encontrado: {secretsPath}");
            return builder;
        }
        
        var secretsToAdd = new Dictionary<string, string?>();
        var secretFiles = Directory.GetFiles(secretsPath);
        
        foreach (var secretFile in secretFiles)
        {
            try
            {
                var fileName = Path.GetFileName(secretFile);
                var secretValue = File.ReadAllText(secretFile).Trim();
                
                // Convertir nombre de archivo a formato de configuración
                // Ejemplo: db-connection-string -> ConnectionStrings:DefaultConnection
                var configKey = ConvertFileNameToConfigKey(fileName);
                
                secretsToAdd[configKey] = secretValue;
                Console.WriteLine($"✅ Secreto de Kubernetes cargado: {configKey}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al cargar secreto {secretFile}: {ex.Message}");
            }
        }
        
        if (secretsToAdd.Any())
        {
            builder.AddInMemoryCollection(secretsToAdd);
            Console.WriteLine($"✅ Total de secretos de Kubernetes cargados: {secretsToAdd.Count}");
        }
        
        return builder;
    }
    
    private static string ConvertFileNameToConfigKey(string fileName)
    {
        // Mapeo de nombres de archivo comunes a claves de configuración
        return fileName.ToLowerInvariant() switch
        {
            "db-connection-string" => "ConnectionStrings:DefaultConnection",
            "jwt-authority" => "Jwt:Authority",
            "redis-connection-string" => "RedisCache:ConnectionString",
            _ => fileName.Replace("-", ":")
        };
    }
}
// Fin código generado por GitHub Copilot
