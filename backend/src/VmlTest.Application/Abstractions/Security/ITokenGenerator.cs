using VmlTest.Domain.Users;

namespace VmlTest.Application.Abstractions.Security;

/// <summary>
/// Token de acceso emitido al iniciar sesión, junto con su vencimiento.
/// </summary>
/// <param name="Value">Token firmado que se envía al cliente.</param>
/// <param name="ExpiresAt">Momento (UTC) en el que el token deja de ser válido.</param>
public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);

/// <summary>
/// Puerto de generación de tokens. La implementación real emite un JWT, pero el
/// caso de uso no necesita saberlo: solo pide un token para el usuario.
/// </summary>
public interface ITokenGenerator
{
    AccessToken Generate(User user);
}
