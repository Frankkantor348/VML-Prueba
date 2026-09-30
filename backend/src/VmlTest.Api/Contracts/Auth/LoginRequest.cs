namespace VmlTest.Api.Contracts.Auth;

/// <summary>
/// Cuerpo de <c>POST /api/auth/login</c>.
/// Igual que el registro, delega la validación al dominio.
/// </summary>
public sealed record LoginRequest(string? Email, string? Password);
