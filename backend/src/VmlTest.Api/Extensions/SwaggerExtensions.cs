namespace VmlTest.Api.Extensions;

/// <summary>
/// Swagger queda habilitado en todos los entornos a propósito: le sirve al equipo
/// evaluador para probar los tres endpoints desde el navegador, sin instalar
/// Postman ni nada parecido.
/// </summary>
public static class SwaggerExtensions
{
    public static IServiceCollection AddAppSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}
