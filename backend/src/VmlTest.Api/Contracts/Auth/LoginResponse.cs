namespace VmlTest.Api.Contracts.Auth;

/// <summary>
/// Respuesta de un login correcto: el JWT y su vencimiento.
/// </summary>
public sealed record LoginResponse(string Token, DateTimeOffset ExpiresAt, string Email);
