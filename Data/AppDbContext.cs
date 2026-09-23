using Microsoft.EntityFrameworkCore;
using WorkForceManagerAPI.Models.Entities;

namespace WorkForceManagerAPI.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Area>         Areas         => Set<Area>();
    public DbSet<Cargo>        Cargos        => Set<Cargo>();
    public DbSet<Rol>          Roles         => Set<Rol>();
    public DbSet<Empleado>     Empleados     => Set<Empleado>();
    public DbSet<Usuario>      Usuarios      => Set<Usuario>();
    public DbSet<TipoTurno>    TiposTurno    => Set<TipoTurno>();
    public DbSet<Horario>      Horarios      => Set<Horario>();
    public DbSet<Asistencia>   Asistencias   => Set<Asistencia>();
    public DbSet<CalculoHoras> CalculoHoras  => Set<CalculoHoras>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Area
        mb.Entity<Area>(e => {
            e.ToTable("Areas");
            e.HasKey(x => x.AreaId);
            e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        });

        // Cargo
        mb.Entity<Cargo>(e => {
            e.ToTable("Cargos");
            e.HasKey(x => x.CargoId);
            e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        });

        // Rol
        mb.Entity<Rol>(e => {
            e.ToTable("Roles");
            e.HasKey(x => x.RolId);
            e.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
        });

        // Empleado
        mb.Entity<Empleado>(e => {
            e.ToTable("Empleados");
            e.HasKey(x => x.EmpleadoId);
            e.Property(x => x.NumeroDocumento).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.NumeroDocumento).IsUnique();
            e.Property(x => x.Nombres).HasMaxLength(100).IsRequired();
            e.Property(x => x.Apellidos).HasMaxLength(100).IsRequired();
            e.HasOne(x => x.Cargo).WithMany(c => c.Empleados).HasForeignKey(x => x.CargoId);
            e.HasOne(x => x.Area).WithMany(a => a.Empleados).HasForeignKey(x => x.AreaId);
        });

        // Usuario
        mb.Entity<Usuario>(e => {
            e.ToTable("Usuarios");
            e.HasKey(x => x.UsuarioId);
            e.Property(x => x.Email).HasMaxLength(150).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.PasswordHash).HasMaxLength(256).IsRequired();
            e.HasOne(x => x.Empleado).WithOne(emp => emp.Usuario).HasForeignKey<Usuario>(x => x.EmpleadoId);
            e.HasOne(x => x.Rol).WithMany(r => r.Usuarios).HasForeignKey(x => x.RolId);
        });

        // TipoTurno
        mb.Entity<TipoTurno>(e => {
            e.ToTable("TiposTurno");
            e.HasKey(x => x.TipoTurnoId);
            e.Property(x => x.HorasEsperadas).HasColumnType("decimal(5,2)");
        });

        // Horario
        mb.Entity<Horario>(e => {
            e.ToTable("Horarios");
            e.HasKey(x => x.HorarioId);
            e.Property(x => x.HorasProgramadas).HasColumnType("decimal(5,2)");
            e.HasIndex(x => new { x.EmpleadoId, x.Fecha }).IsUnique();
            e.HasOne(x => x.Empleado).WithMany(emp => emp.Horarios).HasForeignKey(x => x.EmpleadoId);
            e.HasOne(x => x.TipoTurno).WithMany(t => t.Horarios).HasForeignKey(x => x.TipoTurnoId);
            e.HasOne(x => x.AsignadoPor).WithMany().HasForeignKey(x => x.AsignadoPorUsuarioId);
        });

        // Asistencia
        mb.Entity<Asistencia>(e => {
            e.ToTable("Asistencia");
            e.HasKey(x => x.AsistenciaId);
            e.Property(x => x.HorasTrabajadasReal).HasColumnType("decimal(5,2)");
            e.Property(x => x.MinutosRetraso).HasColumnType("decimal(6,2)");
            e.Property(x => x.EstadoAsistencia).HasMaxLength(20);
            e.HasOne(x => x.Empleado).WithMany(emp => emp.Asistencias).HasForeignKey(x => x.EmpleadoId);
            e.HasOne(x => x.Horario).WithMany(h => h.Asistencias).HasForeignKey(x => x.HorarioId);
        });

        // CalculoHoras
        mb.Entity<CalculoHoras>(e => {
            e.ToTable("CalculoHoras");
            e.HasKey(x => x.CalculoId);
            e.Property(x => x.HorasProgramadas).HasColumnType("decimal(5,2)");
            e.Property(x => x.HorasTrabajadasReal).HasColumnType("decimal(5,2)");
            e.Property(x => x.HorasExtras).HasColumnType("decimal(5,2)");
            e.Property(x => x.HorasFaltantes).HasColumnType("decimal(5,2)");
            e.Property(x => x.MinutosRetraso).HasColumnType("decimal(6,2)");
            e.Property(x => x.PorcentajeCumplimiento).HasColumnType("decimal(5,2)");
            e.HasOne(x => x.Empleado).WithMany().HasForeignKey(x => x.EmpleadoId);
            e.HasOne(x => x.Horario).WithMany().HasForeignKey(x => x.HorarioId);
        });
    }
}
