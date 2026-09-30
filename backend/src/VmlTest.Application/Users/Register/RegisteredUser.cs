namespace VmlTest.Application.Users.Register;

/// <summary>
/// Resultado del registro.
/// Es un DTO propio de la aplicación: la entidad de dominio nunca sale hacia la
/// capa HTTP, así no se filtra el hash de la contraseña.
/// </summary>
public sealed record RegisteredUser(Guid Id, string Email, DateTimeOffset CreatedAt);
