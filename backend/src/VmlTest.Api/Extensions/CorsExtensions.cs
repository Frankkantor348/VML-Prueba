namespace VmlTest.Api.Extensions;

/// <summary>
/// CORS de la API. El frontend Angular y la app Flutter viven en otros dominios,
/// así que hay que autorizarlos de forma explícita. Nunca uso AllowAnyOrigin: los
/// orígenes salen de configuración (variable de entorno en el servidor).
/// </summary>
public static class CorsExtensions
{
    /// <summary>Nombre de la política, para aplicarla luego en el pipeline.</summary>
    public const string PolicyName = "FrontendPermitido";

    private const string ClaveDeConfiguracion = "Cors:AllowedOrigins";

    public static IServiceCollection AddAppCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Acepto una lista separada por comas porque es lo más cómodo de escribir
        // como variable de entorno: Cors__AllowedOrigins=https://a.com,https://b.com
        var origenes = configuration[ClaveDeConfiguracion]
            ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? [];

        services.AddCors(options =>
            options.AddPolicy(PolicyName, policy => policy
                .WithOrigins(origenes)
                .AllowAnyHeader()
                .AllowAnyMethod()));

        return services;
    }
}
