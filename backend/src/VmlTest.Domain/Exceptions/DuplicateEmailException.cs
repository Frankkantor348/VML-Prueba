namespace VmlTest.Domain.Exceptions;

/// <summary>
/// Se lanza cuando se intenta registrar un correo que ya tiene cuenta.
/// La capa HTTP la traduce a un 409 Conflict.
/// </summary>
public sealed class DuplicateEmailException : Exception
{
    public DuplicateEmailException(string email)
        : base($"Ya existe una cuenta registrada con el correo '{email}'.")
    {
        Email = email;
    }

    /// <summary>Correo que provocó el conflicto.</summary>
    public string Email { get; }
}
