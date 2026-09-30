namespace VmlTest.Api.Contracts.Auth;

/// <summary>
/// Cuerpo de <c>POST /api/auth/register</c>.
/// </summary>
/// <remarks>
/// A propósito no lleva atributos de validación: las reglas viven en el dominio
/// (<see cref="Domain.Users.Email"/> y <see cref="Domain.Users.PasswordPolicy"/>).
/// Si las duplicara aquí tendría dos fuentes de verdad que se desincronizan con
/// el tiempo.
/// </remarks>
public sealed record RegisterRequest(string? Email, string? Password);
