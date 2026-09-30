using System.Net;
using Microsoft.AspNetCore.Mvc;
using VmlTest.Application.Exceptions;
using VmlTest.Domain.Exceptions;

namespace VmlTest.Api.Middleware;

/// <summary>
/// Traduce las excepciones a respuestas HTTP en un solo lugar.
/// Gracias a esto los controllers no llevan ni un try/catch y los códigos de
/// estado salen siempre de la misma tabla de decisión.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _siguiente;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate siguiente,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _siguiente = siguiente;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _siguiente(context);
        }
        catch (DomainValidationException ex)
        {
            // Datos que no cumplen las reglas del dominio.
            await EscribirProblemaAsync(context, HttpStatusCode.BadRequest, "Datos inválidos", ex.Message);
        }
        catch (DuplicateEmailException ex)
        {
            await EscribirProblemaAsync(context, HttpStatusCode.Conflict, "Correo ya registrado", ex.Message);
        }
        catch (InvalidCredentialsException ex)
        {
            await EscribirProblemaAsync(context, HttpStatusCode.Unauthorized, "Credenciales inválidas", ex.Message);
        }
        catch (DatabaseUnavailableException ex)
        {
            // El problema no está en la petición sino en un recurso del que depende
            // la API, así que respondo 503 con el mismo criterio que /api/health:
            // así el cliente sabe que puede reintentar más tarde.
            _logger.LogError(
                ex,
                "La base de datos no está disponible atendiendo {Metodo} {Ruta}.",
                context.Request.Method,
                context.Request.Path);

            await EscribirProblemaAsync(
                context,
                HttpStatusCode.ServiceUnavailable,
                "Servicio no disponible",
                ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error no controlado atendiendo {Metodo} {Ruta}.",
                context.Request.Method,
                context.Request.Path);

            // Si la respuesta ya empezó a enviarse no puedo reescribirla; dejo que
            // el error suba y lo maneje el servidor.
            if (context.Response.HasStarted)
            {
                throw;
            }

            await EscribirProblemaAsync(
                context,
                HttpStatusCode.InternalServerError,
                "Error interno del servidor",
                "Ocurrió un error inesperado al procesar la solicitud.");
        }
    }

    /// <summary>
    /// Escribe la respuesta usando ProblemDetails (RFC 7807), el formato estándar
    /// de error que el frontend y la app móvil pueden interpretar igual.
    /// </summary>
    private static async Task EscribirProblemaAsync(
        HttpContext context,
        HttpStatusCode estado,
        string titulo,
        string detalle)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        var problema = new ProblemDetails
        {
            Status = (int)estado,
            Title = titulo,
            Detail = detalle,
            Instance = context.Request.Path
        };

        // El traceId permite cruzar lo que vio el usuario con el log del servidor.
        problema.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.Clear();
        context.Response.StatusCode = (int)estado;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problema);
    }
}
