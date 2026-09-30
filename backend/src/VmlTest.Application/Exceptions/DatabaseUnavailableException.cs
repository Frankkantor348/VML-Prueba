namespace VmlTest.Application.Exceptions;

/// <summary>
/// Se lanza cuando un adaptador de salida no puede alcanzar su recurso; hoy, la
/// base de datos.
/// </summary>
/// <remarks>
/// Vive en la capa de aplicación y no en el dominio porque no es una regla del
/// negocio, sino un fallo de infraestructura. Gracias a esto la capa HTTP no
/// necesita saber que detrás hay PostgreSQL: le basta con traducir esta excepción
/// a un 503, con el mismo criterio que usa /api/health.
/// </remarks>
public sealed class DatabaseUnavailableException : Exception
{
    public DatabaseUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
