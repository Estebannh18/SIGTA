using Microsoft.EntityFrameworkCore;
using WorkForceManagerAPI.Data;
using WorkForceManagerAPI.Models.DTOs.Request;
using WorkForceManagerAPI.Models.Entities;
using WorkForceManagerAPI.Repositories.Interfaces;

namespace WorkForceManagerAPI.Repositories;

public class HorarioRepository(AppDbContext db) : IHorarioRepository
{
    private IQueryable<Horario> ConRelaciones() =>
        db.Horarios
          .Include(h => h.Empleado).ThenInclude(e => e.Area)
          .Include(h => h.TipoTurno)
          .Include(h => h.AsignadoPor).ThenInclude(u => u.Empleado);

    public async Task<(IEnumerable<Horario> Items, int Total)> BuscarAsync(BuscarHorarioRequest filtro)
    {
        var query = ConRelaciones().AsQueryable();

        if (filtro.EmpleadoId.HasValue)
            query = query.Where(h => h.EmpleadoId == filtro.EmpleadoId);

        if (filtro.AreaId.HasValue)
            query = query.Where(h => h.Empleado.AreaId == filtro.AreaId);

        if (filtro.FechaInicio.HasValue)
            query = query.Where(h => h.Fecha >= filtro.FechaInicio);

        if (filtro.FechaFin.HasValue)
            query = query.Where(h => h.Fecha <= filtro.FechaFin);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(h => h.Fecha)
            .ThenBy(h => h.Empleado.Apellidos)
            .Skip((filtro.Page - 1) * filtro.PageSize)
            .Take(filtro.PageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Horario?> ObtenerPorIdAsync(int id) =>
        await ConRelaciones().FirstOrDefaultAsync(h => h.HorarioId == id);

    public async Task<Horario?> ObtenerPorEmpleadoYFechaAsync(int empleadoId, DateOnly fecha) =>
        await ConRelaciones()
            .FirstOrDefaultAsync(h => h.EmpleadoId == empleadoId && h.Fecha == fecha);

    public async Task<Horario> CrearAsync(Horario horario)
    {
        db.Horarios.Add(horario);
        await db.SaveChangesAsync();
        return await ObtenerPorIdAsync(horario.HorarioId) ?? horario;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var horario = await db.Horarios.FindAsync(id);
        if (horario is null) return false;
        db.Horarios.Remove(horario);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<int> AsignacionMasivaAsync(int areaId, int tipoTurnoId, DateOnly fechaInicio,
        DateOnly fechaFin, int asignadoPorUsuarioId, bool sobreescribir)
    {
        var turno = await db.TiposTurno.FindAsync(tipoTurnoId);
        if (turno is null) return 0;

        var empleados = await db.Empleados
            .Where(e => e.AreaId == areaId && e.Activo)
            .ToListAsync();

        var contador = 0;
        var fecha = fechaInicio;

        while (fecha <= fechaFin)
        {
            // Excluir fines de semana
            if (fecha.DayOfWeek != DayOfWeek.Saturday && fecha.DayOfWeek != DayOfWeek.Sunday)
            {
                foreach (var empleado in empleados)
                {
                    var existente = await db.Horarios
                        .FirstOrDefaultAsync(h => h.EmpleadoId == empleado.EmpleadoId && h.Fecha == fecha);

                    if (existente is not null && sobreescribir)
                    {
                        existente.TipoTurnoId          = tipoTurnoId;
                        existente.HoraInicioProgramada = turno.HoraInicio;
                        existente.HoraFinProgramada    = turno.HoraFin;
                        existente.HorasProgramadas     = turno.HorasEsperadas;
                        existente.AsignadoPorUsuarioId = asignadoPorUsuarioId;
                        existente.FechaAsignacion      = DateTime.Now;
                        contador++;
                    }
                    else if (existente is null)
                    {
                        db.Horarios.Add(new Horario
                        {
                            EmpleadoId           = empleado.EmpleadoId,
                            TipoTurnoId          = tipoTurnoId,
                            Fecha                = fecha,
                            HoraInicioProgramada = turno.HoraInicio,
                            HoraFinProgramada    = turno.HoraFin,
                            HorasProgramadas     = turno.HorasEsperadas,
                            AsignadoPorUsuarioId = asignadoPorUsuarioId
                        });
                        contador++;
                    }
                }
                await db.SaveChangesAsync();
            }
            fecha = fecha.AddDays(1);
        }

        return contador;
    }
}