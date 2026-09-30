namespace VmlTest.Application.Users.Login;

/// <summary>
/// Resultado de un inicio de sesión correcto.
/// Es un DTO de la aplicación: expone el token y el correo, nunca el hash.
/// </summary>
public sealed record AuthenticatedUser(string Email, string Token, DateTimeOffset ExpiresAt);
