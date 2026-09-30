using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace VmlTest.Infrastructure.Persistence;

/// <summary>
/// Aplica las migraciones pendientes al arrancar la API.
/// Es cómodo para el evaluador, que no tiene que ejecutar nada a mano sobre la
/// base desplegada, y es seguro porque las migraciones son idempotentes.
/// </summary>
public static class DatabaseMigrationExtensions
{
    private const int IntentosMaximos = 5;
    private static readonly TimeSpan EsperaEntreIntentos = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Ejecuta las migraciones con reintentos. Los planes gratuitos suspenden la
    /// base por inactividad, así que el primer intento puede fallar por timeout.
    /// Si al final no lo consigue, deja el error en el log y permite que la API
    /// arranque igual: así /api/health responde 503 y se ve el problema real en
    /// lugar de dejar el servicio caído sin explicación.
    /// </summary>
    public static async Task MigrateDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        for (var intento = 1; intento <= IntentosMaximos; intento++)
        {
            try
            {
                await context.Database.MigrateAsync(cancellationToken);
                logger.LogInformation("Esquema de base de datos verificado y actualizado.");
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "No se pudieron aplicar las migraciones (intento {Intento} de {Total}).",
                    intento,
                    IntentosMaximos);

                if (intento == IntentosMaximos)
                {
                    logger.LogError(
                        "La API arranca sin poder verificar el esquema. Revise la cadena de conexión: " +
                        "/api/health responderá 503 mientras la base no esté disponible.");
                    return;
                }

                await Task.Delay(EsperaEntreIntentos, cancellationToken);
            }
        }
    }
}
