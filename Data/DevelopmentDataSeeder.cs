using Microsoft.EntityFrameworkCore;
using WorkForceManagerAPI.Models.Entities;

namespace WorkForceManagerAPI.Data;

public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        if (!await db.Roles.AnyAsync())
        {
            db.Roles.AddRange(
                new Rol { Nombre = "Administrador", Descripcion = "Acceso completo al sistema." },
                new Rol { Nombre = "Supervisor", Descripcion = "Gestiona equipos y horarios." },
                new Rol { Nombre = "Empleado", Descripcion = "Consulta su información y asistencia." });
            await db.SaveChangesAsync();
        }

        if (!await db.Areas.AnyAsync())
        {
            db.Areas.AddRange(
                new Area { Nombre = "Operaciones", Descripcion = "Operación diaria y logística." },
                new Area { Nombre = "Tecnología", Descripcion = "Producto y soporte tecnológico." },
                new Area { Nombre = "Administración", Descripcion = "Finanzas y gestión interna." });
            await db.SaveChangesAsync();
        }

        if (!await db.Cargos.AnyAsync())
        {
            db.Cargos.AddRange(
                new Cargo { Nombre = "Coordinador" },
                new Cargo { Nombre = "Analista" },
                new Cargo { Nombre = "Especialista" });
            await db.SaveChangesAsync();
        }

        if (!await db.TiposTurno.AnyAsync())
        {
            db.TiposTurno.AddRange(
                new TipoTurno { Nombre = "Turno mañana", HoraInicio = new TimeOnly(7, 0), HoraFin = new TimeOnly(16, 0), HorasEsperadas = 8 },
                new TipoTurno { Nombre = "Turno tarde", HoraInicio = new TimeOnly(14, 0), HoraFin = new TimeOnly(22, 0), HorasEsperadas = 8 },
                new TipoTurno { Nombre = "Turno completo", HoraInicio = new TimeOnly(8, 0), HoraFin = new TimeOnly(17, 0), HorasEsperadas = 8 });
            await db.SaveChangesAsync();
        }

        if (!await db.Empleados.AnyAsync())
        {
            var area = await db.Areas.OrderBy(x => x.AreaId).FirstAsync();
            var cargo = await db.Cargos.OrderBy(x => x.CargoId).FirstAsync();

            db.Empleados.Add(new Empleado
            {
                NumeroDocumento = "DEV-0001",
                Nombres = "Administrador",
                Apellidos = "Demo",
                AreaId = area.AreaId,
                CargoId = cargo.CargoId,
                FechaIngreso = DateOnly.FromDateTime(DateTime.Today)
            });
            await db.SaveChangesAsync();
        }

        if (!await db.Usuarios.AnyAsync())
        {
            var empleado = await db.Empleados.OrderBy(x => x.EmpleadoId).FirstAsync();
            var rol = await db.Roles.FirstAsync(x => x.Nombre == "Administrador");

            db.Usuarios.Add(new Usuario
            {
                EmpleadoId = empleado.EmpleadoId,
                Email = "admin@workforce.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                RolId = rol.RolId
            });
            await db.SaveChangesAsync();
        }

        if (!await db.Horarios.AnyAsync())
        {
            var usuario = await db.Usuarios.OrderBy(x => x.UsuarioId).FirstAsync();
            var empleado = await db.Empleados.OrderBy(x => x.EmpleadoId).FirstAsync();
            var turno = await db.TiposTurno.OrderBy(x => x.TipoTurnoId).FirstAsync();

            for (var monthsAgo = 5; monthsAgo >= 0; monthsAgo--)
            {
                var date = DateOnly.FromDateTime(DateTime.Today.AddMonths(-monthsAgo));
                var horario = new Horario
                {
                    EmpleadoId = empleado.EmpleadoId,
                    TipoTurnoId = turno.TipoTurnoId,
                    Fecha = date,
                    HoraInicioProgramada = turno.HoraInicio,
                    HoraFinProgramada = turno.HoraFin,
                    HorasProgramadas = turno.HorasEsperadas,
                    AsignadoPorUsuarioId = usuario.UsuarioId,
                    Observaciones = "Registro demo para validar el dashboard."
                };

                db.Horarios.Add(horario);
                await db.SaveChangesAsync();
                db.Asistencias.Add(new Asistencia
                {
                    EmpleadoId = empleado.EmpleadoId,
                    HorarioId = horario.HorarioId,
                    FechaHoraEntrada = date.ToDateTime(turno.HoraInicio).AddMinutes(monthsAgo == 0 ? 18 : 4),
                    FechaHoraSalida = date.ToDateTime(turno.HoraFin),
                    HorasTrabajadasReal = monthsAgo == 0 ? 7.7m : 8m,
                    MinutosRetraso = monthsAgo == 0 ? 18 : 0,
                    EstadoAsistencia = monthsAgo == 0 ? "Tardanza" : "Presente"
                });
                await db.SaveChangesAsync();
            }
        }
    }
}
