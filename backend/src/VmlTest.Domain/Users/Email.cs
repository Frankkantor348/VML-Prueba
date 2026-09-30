using System.Text.RegularExpressions;
using VmlTest.Domain.Exceptions;

namespace VmlTest.Domain.Users;

/// <summary>
/// Dirección de correo electrónico ya validada.
/// Es un objeto de valor: una instancia de <see cref="Email"/> existe únicamente
/// si cumple el formato, por eso el constructor es privado y la única puerta de
/// entrada es <see cref="Create"/>.
/// </summary>
public sealed class Email : IEquatable<Email>
{
    /// <summary>Longitud máxima que permite la RFC 5321 para una dirección.</summary>
    public const int MaxLength = 254;

    // Validación intencionalmente pragmática: no pretendo implementar la RFC 5322
    // completa (es enorme y llena de casos raros), solo descartar lo que claramente
    // no es un correo. Exige texto antes y después de la arroba y al menos un punto
    // en el dominio.
    private static readonly Regex FormatoValido = new(
        @"^[^@\s]+@[^@\s.]+(\.[^@\s.]+)+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private Email(string value) => Value = value;

    /// <summary>Correo normalizado (sin espacios y en minúsculas).</summary>
    public string Value { get; }

    /// <summary>
    /// Crea la dirección a partir del texto que llegó por la API.
    /// </summary>
    /// <exception cref="DomainValidationException">Si es nula, vacía, demasiado larga o no tiene formato de correo.</exception>
    public static Email Create(string? valor)
    {
        var correo = valor?.Trim() ?? string.Empty;

        if (correo.Length == 0)
        {
            throw new DomainValidationException("El correo electrónico es obligatorio.");
        }

        if (correo.Length > MaxLength)
        {
            throw new DomainValidationException(
                $"El correo electrónico no puede superar los {MaxLength} caracteres.");
        }

        if (!FormatoValido.IsMatch(correo))
        {
            throw new DomainValidationException(
                "El correo electrónico no tiene un formato válido.");
        }

        // Normalizo a minúsculas para que "Candidato@VML.com" y "candidato@vml.com"
        // se traten como el mismo correo: si no, se podrían crear cuentas duplicadas.
        return new Email(correo.ToLowerInvariant());
    }

    public override string ToString() => Value;

    public bool Equals(Email? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => obj is Email otro && Equals(otro);

    public override int GetHashCode() => Value.GetHashCode();
}
