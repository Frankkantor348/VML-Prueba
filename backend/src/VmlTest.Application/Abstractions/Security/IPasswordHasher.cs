namespace VmlTest.Application.Abstractions.Security;

/// <summary>
/// Puerto de hashing de contraseñas. Los casos de uso dependen de esta interfaz,
/// no de BCrypt: cambiar el algoritmo (por ejemplo a Argon2) solo requiere un
/// adaptador nuevo en infraestructura.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Convierte la contraseña en claro a un hash almacenable.</summary>
    string Hash(string plainPassword);

    /// <summary>
    /// Comprueba si la contraseña corresponde al hash guardado.
    /// Devuelve <c>false</c> también cuando el hash almacenado está corrupto.
    /// </summary>
    bool Verify(string plainPassword, string passwordHash);
}
