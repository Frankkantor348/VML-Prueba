using Moq;
using VmlTest.Application.Abstractions.Persistence;
using VmlTest.Application.Abstractions.Security;
using VmlTest.Application.Users.Register;
using VmlTest.Domain.Exceptions;
using VmlTest.Domain.Users;

namespace VmlTest.Application.Tests.Users.Register;

/// <summary>
/// Pruebas del caso de uso de registro.
/// Uso dobles de prueba (Moq) para los puertos de salida: estas pruebas no
/// necesitan PostgreSQL ni BCrypt reales, corren en milisegundos.
/// </summary>
public class RegisterUserUseCaseTests
{
    private const string CorreoValido = "candidato@vml.com";
    private const string ContrasenaValida = "ClaveSegura123";

    private readonly Mock<IUserRepository> _repositorio = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly RegisterUserUseCase _casoDeUso;

    public RegisterUserUseCaseTests()
    {
        _casoDeUso = new RegisterUserUseCase(_repositorio.Object, _hasher.Object);
    }

    [Fact]
    public async Task Registro_ConDatosValidos_GuardaLaContrasenaHasheada()
    {
        // Arrange: el correo está libre y el hasher devuelve un hash conocido.
        _repositorio
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _hasher
            .Setup(h => h.Hash(ContrasenaValida))
            .Returns("hash-de-prueba");

        // Act
        var resultado = await _casoDeUso.ExecuteAsync(CorreoValido, ContrasenaValida);

        // Assert: se devuelve el DTO, se hasheó una sola vez y se guardó el hash,
        // nunca la contraseña en texto plano.
        Assert.Equal(CorreoValido, resultado.Email);
        Assert.NotEqual(Guid.Empty, resultado.Id);
        _hasher.Verify(h => h.Hash(ContrasenaValida), Times.Once);
        _repositorio.Verify(
            r => r.AddAsync(
                It.Is<User>(u => u.PasswordHash == "hash-de-prueba" && u.Email.Value == CorreoValido),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Registro_ConCorreoYaRegistrado_LanzaDuplicateEmailYNoGuardaNada()
    {
        // Arrange: el repositorio dice que el correo ya existe.
        _repositorio
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act + Assert
        await Assert.ThrowsAsync<DuplicateEmailException>(
            () => _casoDeUso.ExecuteAsync(CorreoValido, ContrasenaValida));

        // Ni se hashea la contraseña ni se escribe en la base.
        _hasher.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
        _repositorio.Verify(
            r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("sin-arroba")]
    [InlineData("usuario@")]
    [InlineData("usuario@dominio")]
    [InlineData("@dominio.com")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task Registro_ConCorreoInvalido_LanzaErrorDeValidacion(string? correoInvalido)
    {
        await Assert.ThrowsAsync<DomainValidationException>(
            () => _casoDeUso.ExecuteAsync(correoInvalido, ContrasenaValida));

        _repositorio.Verify(
            r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("corta")]
    [InlineData("1234567")]
    [InlineData("")]
    [InlineData(null)]
    public async Task Registro_ConContrasenaQueNoCumpleLaPolitica_LanzaErrorDeValidacion(string? contrasenaInvalida)
    {
        await Assert.ThrowsAsync<DomainValidationException>(
            () => _casoDeUso.ExecuteAsync(CorreoValido, contrasenaInvalida));

        _hasher.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Registro_NormalizaElCorreoAMinusculas()
    {
        // "Candidato@VML.com" y "candidato@vml.com" deben ser la misma cuenta.
        _repositorio
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _hasher
            .Setup(h => h.Hash(It.IsAny<string>()))
            .Returns("hash-de-prueba");

        var resultado = await _casoDeUso.ExecuteAsync("Candidato@VML.com", ContrasenaValida);

        Assert.Equal(CorreoValido, resultado.Email);
    }
}
