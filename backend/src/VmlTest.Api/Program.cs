using VmlTest.Api.Extensions;
using VmlTest.Api.Middleware;
using VmlTest.Application.Users.Login;
using VmlTest.Application.Users.Register;
using VmlTest.Infrastructure;
using VmlTest.Infrastructure.Persistence;

// Los proveedores de nube (Render, Koyeb, Cloud Run) asignan el puerto en la
// variable PORT y enrutan el tráfico hacia él. Se traduce a ASPNETCORE_URLS antes
// de crear el host porque esa variable tiene prioridad sobre WebHost.UseUrls; en
// local no se toca nada y se sigue usando el puerto de launchSettings.
var puertoDelProveedor = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(puertoDelProveedor))
{
    Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://+:{puertoDelProveedor}");
}

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Composition root
// Es el único lugar donde se decide qué implementación concreta usa cada puerto.
// El dominio y la aplicación no saben que este archivo existe.
// ---------------------------------------------------------------------------

// Puertos de entrada: los controllers piden interfaces, no clases concretas.
builder.Services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
builder.Services.AddScoped<ILoginUserUseCase, LoginUserUseCase>();

// Adaptadores de salida: PostgreSQL, BCrypt, JWT y verificador de la base.
builder.Services.AddInfrastructure(builder.Configuration);

// Lectura del JWT que emite esta misma API.
builder.Services.AddJwtAuthentication(builder.Configuration);

// CORS: solo los dominios configurados pueden llamar a la API desde un navegador.
builder.Services.AddAppCors(builder.Configuration);

builder.Services.AddControllers();

// ProblemDetails (RFC 7807) como formato único de error.
builder.Services.AddProblemDetails();

// Swagger, para que el evaluador pruebe los tres endpoints sin herramientas extra.
builder.Services.AddAppSwagger();

var app = builder.Build();

// Aplico las migraciones pendientes al arrancar (con reintentos). Así la base
// desplegada queda lista sin que nadie ejecute comandos a mano.
await app.Services.MigrateDatabaseAsync();

// Va primero para que atrape también los fallos de todo lo que venga después.
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.DocumentTitle = "API Prueba Técnica Full-Stack";
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Full-Stack v1");
});

// No uso UseHttpsRedirection: los proveedores gratuitos terminan el TLS en su
// proxy y la redirección provocaría un bucle.
app.UseCors(CorsExtensions.PolicyName);
app.UseAuthentication();
app.UseAuthorization();

// La raíz no expone nada del negocio, así que redirige a Swagger: quien abra la
// URL base de la API encuentra la documentación en lugar de un 404.
app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapControllers();

app.Run();
