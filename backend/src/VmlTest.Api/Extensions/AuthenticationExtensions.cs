using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using VmlTest.Infrastructure.Security;

namespace VmlTest.Api.Extensions;

/// <summary>
/// Configura la lectura del JWT que emite la propia API.
/// El alcance de la prueba son tres endpoints públicos, así que hoy no hay nada
/// protegido; pero el token que devuelve el login es un JWT real, firmado y
/// verificable con esta misma configuración.
/// </summary>
public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // AddInfrastructure ya validó esta sección y falla al arrancar si falta el
        // secreto, por eso aquí puedo usarla directamente.
        var jwt = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>() ?? new JwtOptions();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                    ValidateLifetime = true,
                    // Tolero un desfase pequeño de reloj entre el servidor que firma
                    // y el que valida.
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        return services;
    }
}
