namespace VmlTest.Api.Contracts.Auth;

/// <summary>
/// Usuario recién creado. Es lo que devuelve el registro: nunca expongo la
/// entidad de dominio, así el hash de la contraseña no puede salir por la API.
/// </summary>
public sealed record UserResponse(Guid Id, string Email, DateTimeOffset CreatedAt);
