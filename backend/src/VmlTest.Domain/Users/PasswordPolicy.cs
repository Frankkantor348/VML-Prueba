using VmlTest.Domain.Exceptions;

namespace VmlTest.Domain.Users;

/// <summary>
/// Reglas mínimas de la contraseña. Viven en el dominio porque son una regla del
/// negocio, no una validación del framework ni de la capa HTTP.
/// </summary>
public static class PasswordPolicy
{
    public const int LongitudMinima = 8;

    // BCrypt solo tiene en cuenta los primeros 72 bytes de la contraseña. Si dejo
    // pasar textos más largos, las últimas letras no afectarían al hash y el
    // usuario podría creer que su clave es más larga de lo que realmente se guarda.
    public const int LongitudMaxima = 72;

    /// <summary>
    /// Valida la contraseña en texto plano. Nunca se guarda ni se registra: solo
    /// se comprueba aquí y se convierte a hash en el adaptador correspondiente.
    /// </summary>
    /// <exception cref="DomainValidationException">Si no cumple la política.</exception>
    public static void Validate(string? contrasena)
    {
        if (string.IsNullOrWhiteSpace(contrasena))
        {
            throw new DomainValidationException("La contraseña es obligatoria.");
        }

        if (contrasena.Length < LongitudMinima)
        {
            throw new DomainValidationException(
                $"La contraseña debe tener al menos {LongitudMinima} caracteres.");
        }

        if (contrasena.Length > LongitudMaxima)
        {
            throw new DomainValidationException(
                $"La contraseña no puede superar los {LongitudMaxima} caracteres.");
        }
    }
}
