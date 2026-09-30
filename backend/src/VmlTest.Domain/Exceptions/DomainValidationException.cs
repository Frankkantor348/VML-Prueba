namespace VmlTest.Domain.Exceptions;

/// <summary>
/// Se lanza cuando un dato no cumple una regla del dominio (correo mal formado,
/// contraseña demasiado corta, etc.). La capa HTTP la traduce a un 400.
/// </summary>
public sealed class DomainValidationException : Exception
{
    public DomainValidationException(string message) : base(message)
    {
    }
}
