namespace VmlTest.Infrastructure.Security;

/// <summary>
/// Configuración del token JWT. Se enlaza desde la sección "Jwt" de la
/// configuración: el secreto llega por variable de entorno (en el servidor) o por
/// user-secrets (en local), nunca escrito en el repositorio.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>Nombre de la sección en appsettings / variables de entorno.</summary>
    public const string SectionName = "Jwt";

    /// <summary>Longitud mínima exigida al secreto para que la firma HS256 tenga sentido.</summary>
    public const int LongitudMinimaSecreto = 32;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string Secret { get; set; } = string.Empty;

    /// <summary>Minutos que el token permanece válido.</summary>
    public int ExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Comprueba que la configuración sirva. Se llama al arrancar para fallar rápido
    /// con un mensaje claro en vez de reventar en el primer login.
    /// </summary>
    public void EnsureIsValid()
    {
        if (string.IsNullOrWhiteSpace(Secret) || Secret.Length < LongitudMinimaSecreto)
        {
            throw new InvalidOperationException(
                $"Falta la clave 'Jwt:Secret' o tiene menos de {LongitudMinimaSecreto} caracteres. " +
                "En local configúrela con 'dotnet user-secrets set \"Jwt:Secret\" \"...\"' y en el " +
                "servidor con una variable de entorno.");
        }

        if (string.IsNullOrWhiteSpace(Issuer))
        {
            throw new InvalidOperationException("Falta el emisor 'Jwt:Issuer' en la configuración.");
        }

        if (string.IsNullOrWhiteSpace(Audience))
        {
            throw new InvalidOperationException("Falta la audiencia 'Jwt:Audience' en la configuración.");
        }

        if (ExpirationMinutes <= 0)
        {
            throw new InvalidOperationException("'Jwt:ExpirationMinutes' debe ser mayor que cero.");
        }
    }
}
