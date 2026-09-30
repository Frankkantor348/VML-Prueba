namespace VmlTest.Application.Abstractions.Health;

/// <summary>
/// Puerto que consulta la disponibilidad de la base de datos para
/// <c>GET /api/health</c>.
/// La rúbrica pide responder 200 cuando la base está arriba y 503 cuando no lo
/// está, así que la comprobación se modela como un puerto y no como código suelto
/// dentro del controller.
/// </summary>
public interface IDatabaseHealthChecker
{
    /// <summary>
    /// Intenta alcanzar la base de datos. Nunca lanza: devuelve <c>false</c> si no
    /// responde, para que el endpoint pueda contestar 503 en lugar de un error 500.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken);
}
