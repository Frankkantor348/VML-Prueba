namespace VmlTest.Domain.Exceptions;

/// <summary>
/// Se lanza cuando el correo no está registrado o la contraseña no coincide.
/// Es el mismo error para los dos casos a propósito: así el cliente no puede
/// averiguar qué correos existen en el sistema.
/// La capa HTTP la traduce a un 401 Unauthorized.
/// </summary>
public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException()
        : base("El correo o la contraseña no son correctos.")
    {
    }
}
