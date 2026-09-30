using Moq;
using VmlTest.Application.Abstractions.Persistence;
using VmlTest.Application.Abstractions.Security;
using VmlTest.Application.Users.Login;
using VmlTest.Domain.Exceptions;
using VmlTest.Domain.Users;

namespace VmlTest.Application.Tests.Users.Login;

/// <summary>
/// Pruebas del caso de uso de inicio de sesión.
/// Igual que las de registro, sustituyen los puertos de salida por dobles.
/// </summary>
public class LoginUserUseCaseTests
{
    private const string CorreoValido = "candidato@vml.com";
    private const string ContrasenaValida = "ClaveSegura123";
    private const string HashGuardado = "hash-de-prueba";

    private readonly Mock<IUserRepository> _repositorio = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ITokenGenerator> _generadorDeTokens = new();
    private readonly LoginUserUseCase _casoDeUso;

    public LoginUserUseCaseTests()
    {
        _casoDeUso = new LoginUserUseCase(
            _repositorio.Object,
            _hasher.Object,
            _generadorDeTokens.Object);
    }

    // Usuario ya registrado que usan las pruebas que sí llegan a validar la clave.
    private static User UsuarioRegistrado() => User.CreateNew(Email.Create(CorreoValido), HashGuardado);

    private void PrepararUsuarioExistente(User usuario)
        => _repositorio
            .Setup(r => r.FindByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

    [Fact]
    public async Task Login_ConCredencialesValidas_DevuelveElTokenGenerado()
    {
        // Arrange
        var usuario = UsuarioRegistrado();
        PrepararUsuarioExistente(usuario);
        _hasher
            .Setup(h => h.Verify(ContrasenaValida, HashGuardado))
            .Returns(true);

        var vencimiento = new DateTimeOffset(2030, 1, 1, 12, 0, 0, TimeSpan.Zero);
        _generadorDeTokens
            .Setup(t => t.Generate(usuario))
            .Returns(new AccessToken("jwt-de-prueba", vencimiento));

        // Act
        var resultado = await _casoDeUso.ExecuteAsync(CorreoValido, ContrasenaValida);

        // Assert
        Assert.Equal("jwt-de-prueba", resultado.Token);
        Assert.Equal(vencimiento, resultado.ExpiresAt);
        Assert.Equal(CorreoValido, resultado.Email);
        _generadorDeTokens.Verify(t => t.Generate(usuario), Times.Once);
    }

    [Fact]
    public async Task Login_ConContrasenaIncorrecta_LanzaCredencialesInvalidasYNoEmiteToken()
    {
        // Arrange: el usuario existe pero la clave no coincide.
        PrepararUsuarioExistente(UsuarioRegistrado());
        _hasher
            .Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        // Act + Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => _casoDeUso.ExecuteAsync(CorreoValido, "ClaveEquivocada123"));

        _generadorDeTokens.Verify(t => t.Generate(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Login_ConCorreoNoRegistrado_LanzaElMismoErrorQueUnaContrasenaIncorrecta()
    {
        // Arrange: no existe ninguna cuenta con ese correo.
        _repositorio
            .Setup(r => r.FindByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act + Assert: devuelvo el mismo error que con una clave mala para no
        // revelar si el correo está registrado.
        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => _casoDeUso.ExecuteAsync(CorreoValido, ContrasenaValida));

        _hasher.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _generadorDeTokens.Verify(t => t.Generate(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Login_ConContrasenaVacia_LanzaCredencialesInvalidas()
    {
        PrepararUsuarioExistente(UsuarioRegistrado());
        _hasher
            .Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => _casoDeUso.ExecuteAsync(CorreoValido, null));
    }

    [Fact]
    public async Task Login_ConCorreoMalFormado_LanzaErrorDeValidacion()
    {
        // Un correo mal formado es un 400, no un intento fallido de sesión.
        await Assert.ThrowsAsync<DomainValidationException>(
            () => _casoDeUso.ExecuteAsync("sin-arroba", ContrasenaValida));

        _generadorDeTokens.Verify(t => t.Generate(It.IsAny<User>()), Times.Never);
    }
}
