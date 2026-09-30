using Microsoft.EntityFrameworkCore;
using Npgsql;
using VmlTest.Application.Abstractions.Persistence;
using VmlTest.Application.Exceptions;
using VmlTest.Domain.Exceptions;
using VmlTest.Domain.Users;

namespace VmlTest.Infrastructure.Persistence.Repositories;

/// <summary>
/// Adaptador del puerto <see cref="IUserRepository"/> sobre EF Core + Npgsql.
/// </summary>
public sealed class EfCoreUserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public EfCoreUserRepository(AppDbContext context) => _context = context;

    public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken)
        => EjecutarAsync(() => _context.Users.AnyAsync(usuario => usuario.Email == email, cancellationToken));

    public Task<User?> FindByEmailAsync(Email email, CancellationToken cancellationToken)
        // AsNoTracking: el login solo lee, no necesita que EF siga la entidad.
        => EjecutarAsync(() => _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(usuario => usuario.Email == email, cancellationToken));

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        _context.Users.Add(user);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (EsCorreoDuplicado(ex))
        {
            // Carrera de dos registros con el mismo correo: el índice único de
            // PostgreSQL lo impide y aquí traduzco el error de la base al error de
            // dominio, para que la API siga respondiendo 409 y no un 500.
            throw new DuplicateEmailException(user.Email.Value);
        }
        catch (Exception ex) when (EsFalloDeConexion(ex))
        {
            throw new DatabaseUnavailableException(MensajeBaseNoDisponible, ex);
        }
    }

    /// <summary>
    /// Ejecuta una consulta traduciendo los fallos de conexión a la excepción de
    /// aplicación. Así la capa HTTP puede responder 503 sin conocer Npgsql.
    /// </summary>
    private static async Task<T> EjecutarAsync<T>(Func<Task<T>> consulta)
    {
        try
        {
            return await consulta();
        }
        catch (Exception ex) when (EsFalloDeConexion(ex))
        {
            throw new DatabaseUnavailableException(MensajeBaseNoDisponible, ex);
        }
    }

    private const string MensajeBaseNoDisponible =
        "No fue posible comunicarse con la base de datos. Intente de nuevo en unos segundos.";

    // SQLSTATE 23505 = unique_violation en PostgreSQL.
    private static bool EsCorreoDuplicado(DbUpdateException ex)
        => ex.InnerException is PostgresException { SqlState: "23505" };

    // NpgsqlException agrupa los fallos del driver (servidor apagado, credenciales
    // rechazadas, instancia suspendida). El TimeoutException aparece como excepción
    // interna cuando la conexión se queda colgada, algo habitual en los planes
    // gratuitos que suspenden la base por inactividad.
    private static bool EsFalloDeConexion(Exception ex)
        => !EsErrorDelServidor(ex)
            && (ex is NpgsqlException or TimeoutException
                || ex.InnerException is NpgsqlException or TimeoutException);

    // Si PostgreSQL contestó con un SQLSTATE, la base está viva: el fallo es de la
    // consulta y no de la conexión, así que no debe convertirse en un 503.
    private static bool EsErrorDelServidor(Exception ex)
        => ex is PostgresException || ex.InnerException is PostgresException;
}
