using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VmlTest.Application.Abstractions.Health;
using VmlTest.Infrastructure.Persistence;

namespace VmlTest.Infrastructure.Health;

/// <summary>
/// Adaptador de <see cref="IDatabaseHealthChecker"/>: intenta abrir una conexión
/// real contra PostgreSQL.
/// </summary>
public sealed class DatabaseHealthChecker : IDatabaseHealthChecker
{
    private readonly AppDbContext _context;
    private readonly ILogger<DatabaseHealthChecker> _logger;

    public DatabaseHealthChecker(AppDbContext context, ILogger<DatabaseHealthChecker> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _context.Database.CanConnectAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // El cliente cortó la petición; no hay nada que reportar.
            return false;
        }
        catch (Exception ex)
        {
            // La base puede estar suspendida (planes gratuitos) o inalcanzable. No
            // propago la excepción: el endpoint debe responder 503, no un 500, y el
            // motivo queda en el log para diagnosticar.
            _logger.LogWarning(ex, "No fue posible conectar con PostgreSQL al verificar /api/health.");
            return false;
        }
    }
}
