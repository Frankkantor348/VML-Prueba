using VmlTest.Application.Abstractions.Persistence;
using VmlTest.Application.Abstractions.Security;
using VmlTest.Domain.Exceptions;
using VmlTest.Domain.Users;

namespace VmlTest.Application.Users.Register;

/// <summary>
/// Caso de uso: registrar un usuario nuevo.
/// Orquesta las reglas del dominio y los puertos de salida; no sabe si detrás hay
/// PostgreSQL, BCrypt o un doble de prueba. Hace una sola cosa.
/// </summary>
public sealed class RegisterUserUseCase : IRegisterUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserUseCase(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisteredUser> ExecuteAsync(
        string? email,
        string? password,
        CancellationToken cancellationToken = default)
    {
        // 1) Valido con las reglas del dominio. Si algo está mal, la excepción
        //    llega al middleware y la API responde 400.
        var correo = Email.Create(email);
        PasswordPolicy.Validate(password);

        // A partir de aquí el dominio ya garantiza que la contraseña no es nula.
        var contrasena = password!;

        // 2) El correo es único: si ya está tomado lo rechazo con 409 sin llegar a
        //    escribir en la base.
        if (await _userRepository.ExistsByEmailAsync(correo, cancellationToken))
        {
            throw new DuplicateEmailException(correo.Value);
        }

        // 3) Nunca guardo la contraseña en texto plano. El hashing es un puerto:
        //    aquí solo pido "conviérteme esto en un hash".
        var hash = _passwordHasher.Hash(contrasena);
        var usuario = User.CreateNew(correo, hash);

        // El repositorio además protege contra la carrera de dos registros
        // simultáneos con el mismo correo (índice único en la base).
        await _userRepository.AddAsync(usuario, cancellationToken);

        // Devuelvo un DTO, no la entidad: el PasswordHash no debe salir de aquí.
        return new RegisteredUser(usuario.Id, usuario.Email.Value, usuario.CreatedAt);
    }
}
