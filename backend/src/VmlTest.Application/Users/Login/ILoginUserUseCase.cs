using VmlTest.Domain.Exceptions;

namespace VmlTest.Application.Users.Login;

/// <summary>
/// Puerto de entrada del caso de uso de inicio de sesión.
/// </summary>
public interface ILoginUserUseCase
{
    /// <summary>
    /// Valida las credenciales y devuelve un token de acceso.
    /// </summary>
    /// <exception cref="DomainValidationException">Si el correo viene vacío o mal formado.</exception>
    /// <exception cref="InvalidCredentialsException">Si el correo no existe o la contraseña no coincide.</exception>
    Task<AuthenticatedUser> ExecuteAsync(
        string? email,
        string? password,
        CancellationToken cancellationToken = default);
}
