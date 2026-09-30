using VmlTest.Application.Abstractions.Persistence;
using VmlTest.Application.Abstractions.Security;
using VmlTest.Domain.Exceptions;
using VmlTest.Domain.Users;

namespace VmlTest.Application.Users.Login;

/// <summary>
/// Caso de uso: iniciar sesión.
/// Comprueba las credenciales y, si son correctas, delega la emisión del token en
/// el puerto correspondiente.
/// </summary>
public sealed class LoginUserUseCase : ILoginUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;

    public LoginUserUseCase(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenGenerator tokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthenticatedUser> ExecuteAsync(
        string? email,
        string? password,
        CancellationToken cancellationToken = default)
    {
        // Reutilizo la validación del dominio para el formato del correo. Un correo
        // mal formado es un 400, no un intento fallido de sesión.
        var correo = Email.Create(email);

        var usuario = await _userRepository.FindByEmailAsync(correo, cancellationToken);

        // Deliberadamente el mismo error si el correo no existe o si la contraseña
        // no coincide: así nadie puede averiguar qué correos están registrados.
        if (usuario is null || !_passwordHasher.Verify(password ?? string.Empty, usuario.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        var token = _tokenGenerator.Generate(usuario);

        return new AuthenticatedUser(usuario.Email.Value, token.Value, token.ExpiresAt);
    }
}
