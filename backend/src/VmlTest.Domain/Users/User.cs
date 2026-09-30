using VmlTest.Domain.Exceptions;

namespace VmlTest.Domain.Users;

/// <summary>
/// Usuario del sistema. Es la entidad central del dominio: no hereda de nada ni
/// lleva atributos de Entity Framework, así que puede compilarse sin ninguna
/// dependencia de infraestructura.
/// </summary>
public sealed class User
{
    // Constructor privado sin parámetros: lo necesita EF Core para materializar
    // las filas de la tabla Users. Los valores los asignan los setters privados.
    private User()
    {
        Email = null!;
        PasswordHash = null!;
    }

    // Constructor real del dominio: solo la fábrica CreateNew puede usarlo.
    private User(Guid id, Email email, string passwordHash, DateTimeOffset createdAt)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    /// <summary>Correo del usuario, ya validado y normalizado.</summary>
    public Email Email { get; private set; }

    /// <summary>Hash de la contraseña. El texto plano nunca llega hasta aquí.</summary>
    public string PasswordHash { get; private set; }

    /// <summary>Fecha de creación, siempre en UTC.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Crea un usuario nuevo. El identificador y la fecha los genera el propio
    /// dominio, así el caso de uso no tiene que preocuparse por ellos.
    /// </summary>
    /// <param name="email">Correo ya validado.</param>
    /// <param name="passwordHash">Resultado de aplicar el hasher; nunca la contraseña en claro.</param>
    public static User CreateNew(Email email, string passwordHash)
    {
        ArgumentNullException.ThrowIfNull(email);

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainValidationException("El hash de la contraseña es obligatorio.");
        }

        return new User(Guid.NewGuid(), email, passwordHash, DateTimeOffset.UtcNow);
    }
}
