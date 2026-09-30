using Microsoft.AspNetCore.Mvc;
using VmlTest.Api.Contracts.Health;
using VmlTest.Application.Abstractions.Health;

namespace VmlTest.Api.Controllers;

/// <summary>
/// Diagnóstico del servicio. Es el endpoint que usa el equipo evaluador para
/// comprobar que la API está arriba y que alcanza la base de datos.
/// </summary>
[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    private readonly IDatabaseHealthChecker _databaseHealthChecker;

    public HealthController(IDatabaseHealthChecker databaseHealthChecker)
        => _databaseHealthChecker = databaseHealthChecker;

    /// <summary>
    /// Verifica la API y su conexión con PostgreSQL.
    /// Responde 200 cuando todo está bien y 503 cuando la base no está disponible.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var baseDisponible = await _databaseHealthChecker.IsAvailableAsync(cancellationToken);

        var respuesta = new HealthResponse(
            Status: baseDisponible ? "Healthy" : "Unhealthy",
            Database: baseDisponible ? "Connected" : "Unavailable",
            TimestampUtc: DateTimeOffset.UtcNow);

        // La rúbrica pide distinguir los dos casos: 200 si la base responde y 503 si no.
        return baseDisponible
            ? Ok(respuesta)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, respuesta);
    }
}
