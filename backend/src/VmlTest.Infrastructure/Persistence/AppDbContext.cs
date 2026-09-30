using Microsoft.EntityFrameworkCore;
using VmlTest.Domain.Users;

namespace VmlTest.Infrastructure.Persistence;

/// <summary>
/// Contexto de EF Core. Es un detalle de infraestructura, por eso vive aquí y no
/// en el dominio: la entidad <see cref="User"/> no sabe que existe.
/// </summary>
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Toda la configuración de mapeo vive en clases aparte; así este archivo no
        // crece cada vez que se agrega una entidad.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
