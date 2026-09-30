using VmlTest.Domain.Exceptions;
using VmlTest.Domain.Users;

namespace VmlTest.Application.Users.Register;

/// <summary>
/// Puerto de entrada del caso de uso de registro: es el contrato que consume la
/// capa HTTP. Un controller depende de esta interfaz, nunca de la clase concreta.
/// </summary>
public interface IRegisterUserUseCase
{
    /// <summary>
    /// Valida los datos, comprueba que el correo esté libre, guarda la contraseña
    /// hasheada y crea la cuenta.
    /// </summary>
    /// <exception cref="DomainValidationException">Correo con formato inválido o contraseña que no cumple la política.</exception>
    /// <exception cref="DuplicateEmailException">Ya existe una cuenta con ese correo.</exception>
    Task<RegisteredUser> ExecuteAsync(
        string? email,
        string? password,
        CancellationToken cancellationToken = default);
}
