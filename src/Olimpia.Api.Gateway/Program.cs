// Inicio código generado por GitHub Copilot
using Microsoft.IdentityModel.Tokens;
using MMLib.Ocelot.Provider.AppConfiguration;
using MMLib.SwaggerForOcelot.DependencyInjection;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddOcelotWithSwaggerSupport(options =>
    {
        // Fusiona todos los archivos ocelot.*.json de Configuration/ en tiempo de arranque.
        options.Folder = "Configuration";
    });

bool allowAuthValidator = builder.Configuration.GetValue<bool>("AllowAutenticationValidator");
if (allowAuthValidator)
{
    // El Gateway valida que exista un Bearer token sin verificar su firma.
    // La validación real (firma, audience, issuer, lifetime) ocurre en el API downstream.
    // UseSecurityTokenValidators fuerza el handler legacy (JwtSecurityTokenHandler)
    // que respeta SignatureValidator; JsonWebTokenHandler (.NET 8+) lo ignora.
    builder.Services.AddAuthentication()
        .AddJwtBearer("Bearer", options =>
        {
            options.UseSecurityTokenValidators = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer           = false,
                ValidateAudience         = false,
                ValidateLifetime         = false,
                ValidateActor            = false,
                ValidateSignatureLast    = false,
                ValidateTokenReplay      = false,
                ValidateWithLKG          = false,
                ValidateIssuerSigningKey = false,
                RequireSignedTokens      = false,
                SignatureValidator       = (token, _) =>
                    new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(token),
            };
        });
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("GatewayPolicy", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

bool allowSwagger = builder.Configuration.GetValue<bool>("AllowSwaggerForOcelot");
if (allowSwagger)
{
    builder.Services.AddSwaggerForOcelot(builder.Configuration);
}

builder.Services
    .AddOcelot(builder.Configuration)
    .AddAppConfiguration();

var app = builder.Build();

app.UseCors("GatewayPolicy");
app.UseRouting();

if (allowSwagger)
{
    app.UseSwaggerForOcelotUI(options =>
    {
        options.PathToSwaggerGenerator = "/swagger/docs";
    });
}

app.UseAuthentication();
app.UseAuthorization();

await app.UseOcelot();
await app.RunAsync();
// Fin código generado por GitHub Copilot
