using Npgsql;

namespace VmlTest.Infrastructure.Persistence;

/// <summary>
/// Normaliza la cadena de conexión que entregan los proveedores de nube.
/// </summary>
/// <remarks>
/// Render (y otros proveedores de estilo Heroku) publican la base como una URI:
/// <code>postgres://usuario:clave@servidor:5432/base</code>
/// mientras que Npgsql solo entiende el formato clave-valor:
/// <code>Host=servidor;Port=5432;Database=base;Username=usuario;Password=clave</code>
///
/// La conversión vive aquí para que el mismo binario sirva en local (formato de
/// docker-compose o user-secrets) y en la nube (URI del proveedor), sin ramas en
/// el código de configuración.
/// </remarks>
public static class PostgresConnectionString
{
    private const int PuertoPorDefecto = 5432;

    private static readonly string[] PrefijosUri = ["postgres://", "postgresql://"];

    /// <summary>
    /// Devuelve una cadena que Npgsql pueda usar. Si ya viene en formato
    /// clave-valor, la deja intacta.
    /// </summary>
    public static string Normalize(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        if (!EsUri(connectionString))
        {
            return connectionString;
        }

        var uri = new Uri(connectionString);

        // El usuario y la clave pueden venir con caracteres escapados, así que hay
        // que decodificarlos antes de pasarlos a Npgsql.
        var credenciales = uri.UserInfo.Split(':', 2);

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? PuertoPorDefecto : uri.Port,
            Database = Uri.UnescapeDataString(uri.AbsolutePath).TrimStart('/'),
            Username = Uri.UnescapeDataString(credenciales[0]),
            Password = credenciales.Length > 1 ? Uri.UnescapeDataString(credenciales[1]) : string.Empty
        };

        // Los proveedores gestionados añaden `?sslmode=require` a la URI. Si viene,
        // se respeta; si no, dejo el valor por defecto de Npgsql (Prefer), que sirve
        // tanto para una base local sin TLS como para una red privada del proveedor.
        if (LeerModoDeSsl(uri) is { } modoDeSsl)
        {
            builder.SslMode = modoDeSsl;
        }

        return builder.ConnectionString;
    }

    private static bool EsUri(string connectionString)
        => PrefijosUri.Any(prefijo => connectionString.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase));

    private static SslMode? LeerModoDeSsl(Uri uri)
    {
        // uri.Query llega como "?sslmode=require&otra=cosa".
        foreach (var parametro in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var partes = parametro.Split('=', 2);
            if (partes.Length == 2 && partes[0].Equals("sslmode", StringComparison.OrdinalIgnoreCase))
            {
                // Npgsql entiende los mismos nombres que PostgreSQL: disable,
                // allow, prefer, require, verify-ca y verify-full.
                return Enum.TryParse<SslMode>(partes[1], ignoreCase: true, out var modo) ? modo : null;
            }
        }

        return null;
    }
}
