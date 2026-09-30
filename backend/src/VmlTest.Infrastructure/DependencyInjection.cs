using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VmlTest.Application.Abstractions.Health;
using VmlTest.Application.Abstractions.Persistence;
using VmlTest.Application.Abstractions.Security;
using VmlTest.Infrastructure.Health;
using VmlTest.Infrastructure.Persistence;
using VmlTest.Infrastructure.Persistence.Repositories;
using VmlTest.Infrastructure.Security;

namespace VmlTest.Infrastructure;

/// <summary>
/// Registro de los adaptadores de salida (persistencia, hashing, tokens y estado
/// de la base). Lo llama la capa Api, que es donde vive el composition root.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var cadenaDeConexion = configuration.GetConnectionString("Default");

        if (string.IsNullOrWhiteSpace(cadenaDeConexion))
        {
            throw new InvalidOperationException(
                "Falta la cadena de conexión 'ConnectionStrings:Default'. En local configúrela con " +
                "'dotnet user-secrets set \"ConnectionStrings:Default\" \"...\"' y en el servidor con " +
                "una variable de entorno.");
        }

        // Los proveedores de nube publican la base como URI (postgres://...); el
        // normalizador la traduce al formato clave-valor que entiende Npgsql.
        var cadenaParaNpgsql = PostgresConnectionString.Normalize(cadenaDeConexion);

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(cadenaParaNpgsql));

        // Valido el JWT aquí, al arrancar, para fallar rápido con un mensaje claro
        // en lugar de descubrir el problema en el primer login.
        var jwtOptions = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>() ?? new JwtOptions();

        jwtOptions.EnsureIsValid();
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        // El hasher y el generador de tokens no guardan estado, así que pueden ser
        // singleton. El repositorio y el contexto son por petición.
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IUserRepository, EfCoreUserRepository>();
        services.AddScoped<IDatabaseHealthChecker, DatabaseHealthChecker>();

        return services;
    }
}
