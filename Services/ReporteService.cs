using Microsoft.EntityFrameworkCore;
using WorkForceManagerAPI.Data;
using WorkForceManagerAPI.Models.DTOs.Response;
using WorkForceManagerAPI.Services.Interfaces;

namespace WorkForceManagerAPI.Services;

public class ReporteService(AppDbContext db) : IReporteService
{
    public async Task<IEnumerable<ReporteHorasRow>> HorasAsync(DateOnly fechaInicio, DateOnly fechaFin)
    {
        var rows = await db.Horarios
            .Where(h => h.Fecha >= fechaInicio && h.Fecha <= fechaFin)
            .Select(h => new
            {
                h.EmpleadoId,
                NombreCompleto = h.Empleado.Nombres + " " + h.Empleado.Apellidos,
                Area = h.Empleado.Area.Nombre,
                Cargo = h.Empleado.Cargo.Nombre,
                h.HorasProgramadas,
                Asistencia = h.Asistencias.OrderByDescending(a => a.FechaHoraEntrada)
                    .Select(a => new { a.HorasTrabajadasReal, a.EstadoAsistencia }).FirstOrDefault()
            })
            .ToListAsync();

        return rows
            .GroupBy(x => new { x.EmpleadoId, x.NombreCompleto, x.Area, x.Cargo })
            .Select(g =>
            {
                var asistidos = g.Where(x => x.Asistencia is not null).ToList();
                var horasProg = g.Sum(x => x.HorasProgramadas);
                var horasTrab = asistidos.Sum(x => x.Asistencia!.HorasTrabajadasReal ?? 0);

                return new ReporteHorasRow
                {
                    EmpleadoId = g.Key.EmpleadoId,
                    NombreCompleto = g.Key.NombreCompleto,
                    Area = g.Key.Area,
                    Cargo = g.Key.Cargo,
                    DiasProgramados = g.Count(),
                    DiasAsistidos = asistidos.Count,
                    HorasProgramadas = Math.Round(horasProg, 2),
                    HorasTrabajadas = Math.Round(horasTrab, 2),
                    HorasExtras = Math.Round(Math.Max(horasTrab - horasProg, 0), 2),
                    HorasFaltantes = Math.Round(Math.Max(horasProg - horasTrab, 0), 2),
                    Tardanzas = asistidos.Count(x => x.Asistencia!.EstadoAsistencia == "Tardanza"),
                    Cumplimiento = horasProg == 0 ? 0 : Math.Round(horasTrab / horasProg * 100, 2)
                };
            })
            .OrderByDescending(x => x.Cumplimiento)
            .ThenBy(x => x.NombreCompleto)
            .ToList();
    }

    public async Task<IEnumerable<ReporteAsistenciaRow>> AsistenciaAsync(DateOnly fechaInicio, DateOnly fechaFin) =>
        await db.Asistencias
            .Where(a => DateOnly.FromDateTime(a.FechaHoraEntrada) >= fechaInicio
                     && DateOnly.FromDateTime(a.FechaHoraEntrada) <= fechaFin)
            .OrderBy(a => a.FechaHoraEntrada)
            .Select(a => new ReporteAsistenciaRow
            {
                Fecha = DateOnly.FromDateTime(a.FechaHoraEntrada),
                NombreCompleto = a.Empleado.Nombres + " " + a.Empleado.Apellidos,
                Area = a.Empleado.Area.Nombre,
                HoraEntrada = a.FechaHoraEntrada,
                HoraSalida = a.FechaHoraSalida,
                HorasTrabajadas = a.HorasTrabajadasReal ?? 0,
                MinutosRetraso = a.MinutosRetraso,
                Estado = a.EstadoAsistencia
            })
            .ToListAsync();
}
