using Microsoft.AspNetCore.Mvc;
using VmlTest.Api.Contracts.Auth;
using VmlTest.Application.Users.Login;
using VmlTest.Application.Users.Register;

namespace VmlTest.Api.Controllers;

/// <summary>
/// Adaptador de entrada HTTP para el registro y el inicio de sesión.
/// El controller solo orquesta: traduce HTTP al caso de uso y el resultado de
/// vuelta a HTTP. No hay ni una regla de negocio ni un try/catch aquí.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IRegisterUserUseCase _registerUserUseCase;
    private readonly ILoginUserUseCase _loginUserUseCase;

    public AuthController(
        IRegisterUserUseCase registerUserUseCase,
        ILoginUserUseCase loginUserUseCase)
    {
        _registerUserUseCase = registerUserUseCase;
        _loginUserUseCase = loginUserUseCase;
    }

    /// <summary>
    /// Crea una cuenta nueva.
    /// </summary>
    /// <response code="201">Usuario creado.</response>
    /// <response code="400">Correo con formato inválido o contraseña que no cumple la política.</response>
    /// <response code="409">Ya existe una cuenta con ese correo.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var usuario = await _registerUserUseCase.ExecuteAsync(
            request.Email,
            request.Password,
            cancellationToken);

        var respuesta = new UserResponse(usuario.Id, usuario.Email, usuario.CreatedAt);

        // Devuelvo 201 sin cabecera Location: el alcance de la prueba son tres
        // endpoints y no existe un GET /api/users/{id} al que apuntar.
        return StatusCode(StatusCodes.Status201Created, respuesta);
    }

    /// <summary>
    /// Valida las credenciales y entrega un token JWT.
    /// </summary>
    /// <response code="200">Credenciales correctas; el cuerpo trae el token.</response>
    /// <response code="400">El correo no tiene formato válido.</response>
    /// <response code="401">Correo inexistente o contraseña incorrecta.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var sesion = await _loginUserUseCase.ExecuteAsync(
            request.Email,
            request.Password,
            cancellationToken);

        return Ok(new LoginResponse(sesion.Token, sesion.ExpiresAt, sesion.Email));
    }
}
