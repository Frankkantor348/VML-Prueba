namespace VmlTest.Api.Contracts.Health;

/// <summary>
/// Respuesta de <c>GET /api/health</c>.
/// </summary>
/// <param name="Status">"Healthy" si la API y la base responden; "Unhealthy" si no.</param>
/// <param name="Database">Estado de PostgreSQL: "Connected" o "Unavailable".</param>
/// <param name="TimestampUtc">Momento de la verificación, en UTC.</param>
public sealed record HealthResponse(string Status, string Database, DateTimeOffset TimestampUtc);
