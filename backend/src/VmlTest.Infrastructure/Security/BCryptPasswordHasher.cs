using VmlTest.Application.Abstractions.Security;

namespace VmlTest.Infrastructure.Security;

/// <summary>
/// Adaptador de <see cref="IPasswordHasher"/> basado en BCrypt.
/// El factor de trabajo 12 es un equilibrio razonable entre resistencia a fuerza
/// bruta y tiempo de respuesta del endpoint.
/// </summary>
public sealed class BCryptPasswordHasher : IPasswordHasher
{
    private const int FactorDeTrabajo = 12;

    public string Hash(string plainPassword)
        => BCrypt.Net.BCrypt.HashPassword(plainPassword, FactorDeTrabajo);

    public bool Verify(string plainPassword, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(plainPassword, passwordHash);
        }
        catch (Exception ex) when (ex is BCrypt.Net.SaltParseException or ArgumentException)
        {
            // El hash guardado está corrupto o tiene un formato que BCrypt no
            // reconoce. Lo trato como credencial inválida (401) en lugar de
            // dejar que se convierta en un error 500.
            return false;
        }
    }
}
