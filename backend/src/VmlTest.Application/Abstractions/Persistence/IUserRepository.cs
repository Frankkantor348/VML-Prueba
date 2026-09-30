using VmlTest.Domain.Users;

namespace VmlTest.Application.Abstractions.Persistence;

/// <summary>
/// Puerto de salida hacia la persistencia de usuarios.
/// La capa de aplicación solo conoce esta interfaz: si detrás hay PostgreSQL,
/// SQLite o una lista en memoria, es un detalle del adaptador.
/// </summary>
public interface IUserRepository
{
    /// <summary>Indica si ya existe una cuenta con ese correo.</summary>
    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken);

    /// <summary>Busca un usuario por correo. Devuelve <c>null</c> si no existe.</summary>
    Task<User?> FindByEmailAsync(Email email, CancellationToken cancellationToken);

    /// <summary>
    /// Agrega el usuario y confirma los cambios.
    /// </summary>
    /// <exception cref="VmlTest.Domain.Exceptions.DuplicateEmailException">
    /// Si otro proceso insertó el mismo correo entre la verificación y la escritura.
    /// </exception>
    Task AddAsync(User user, CancellationToken cancellationToken);
}
