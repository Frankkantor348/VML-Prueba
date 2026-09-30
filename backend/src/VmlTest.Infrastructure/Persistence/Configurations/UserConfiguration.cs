using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VmlTest.Domain.Users;

namespace VmlTest.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeo de la entidad <see cref="User"/> a la tabla Users de PostgreSQL.
/// </summary>
internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        // El Id lo genera el dominio (Guid.NewGuid), no la base de datos.
        builder.HasKey(usuario => usuario.Id);
        builder.Property(usuario => usuario.Id)
            .HasColumnName("Id")
            .ValueGeneratedNever();

        // El dominio maneja el correo como objeto de valor, pero en la tabla es una
        // columna de texto: aquí está la conversión entre los dos mundos.
        builder.Property(usuario => usuario.Email)
            .HasColumnName("Email")
            .HasMaxLength(Email.MaxLength)
            .HasConversion(
                email => email.Value,
                valor => Email.Create(valor))
            .IsRequired();

        // 60 caracteres de BCrypt; dejo margen por si más adelante se cambia el hasher.
        builder.Property(usuario => usuario.PasswordHash)
            .HasColumnName("PasswordHash")
            .HasMaxLength(200)
            .IsRequired();

        // Npgsql guarda DateTimeOffset como timestamptz y siempre en UTC.
        builder.Property(usuario => usuario.CreatedAt)
            .HasColumnName("CreatedAt")
            .IsRequired();

        // Un correo, una cuenta. Además de la verificación que hace el caso de uso,
        // el índice único protege contra dos registros simultáneos.
        builder.HasIndex(usuario => usuario.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");
    }
}
