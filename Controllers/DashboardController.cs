using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkForceManagerAPI.Data;
using WorkForceManagerAPI.Models.DTOs.Response;

namespace WorkForceManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController(AppDbContext db) : ControllerBase
{
    [HttpGet("resumen")]
    public async Task<ActionResult<DashboardResumenResponse>> Resumen([FromQuery] DateOnly? fecha)
    {
        var dia = fecha ?? DateOnly.FromDateTime(DateTime.Today);
        var empleadosActivos = await db.Empleados.CountAsync(x => x.Activo);
        var horarios = await db.Horarios
            .Where(x => x.Fecha == dia)
            .Select(x => new { x.HorarioId })
            .ToListAsync();
        var asistencias = await db.Asistencias
            .Where(x => DateOnly.FromDateTime(x.FechaHoraEntrada) == dia)
            .Select(x => new { x.HorarioId, x.EstadoAsistencia, x.HorasTrabajadasReal })
            .ToListAsync();

        var horariosIds = horarios.Select(x => x.HorarioId).ToHashSet();
        var asistenciasConHorario = asistencias.Count(x => x.HorarioId.HasValue && horariosIds.Contains(x.HorarioId.Value));

        return Ok(new DashboardResumenResponse
        {
            Fecha = dia,
            EmpleadosActivos = empleadosActivos,
            HorariosProgramados = horarios.Count,
            AsistenciasRegistradas = asistencias.Count,
            Tardanzas = asistencias.Count(x => x.EstadoAsistencia == "Tardanza"),
            HorasTrabajadas = Math.Round(asistencias.Sum(x => x.HorasTrabajadasReal ?? 0), 2),
            Cumplimiento = horarios.Count == 0 ? 0 : Math.Round((decimal)asistenciasConHorario / horarios.Count * 100, 2)
        });
    }

    [HttpGet("areas")]
    public async Task<ActionResult<IEnumerable<DashboardAreaResponse>>> PorArea(
        [FromQuery] DateOnly? fechaInicio, [FromQuery] DateOnly? fechaFin)
    {
        var inicio = fechaInicio ?? DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        var fin = fechaFin ?? DateOnly.FromDateTime(DateTime.Today);
        var rows = await db.Horarios
            .Where(x => x.Fecha >= inicio && x.Fecha <= fin)
            .Select(x => new
            {
                Area = x.Empleado.Area.Nombre,
                x.HorarioId,
                Asistencia = x.Asistencias.OrderByDescending(a => a.FechaHoraEntrada)
                    .Select(a => new { a.EstadoAsistencia, a.HorasTrabajadasReal }).FirstOrDefault()
            })
            .ToListAsync();

        return Ok(rows.GroupBy(x => x.Area).Select(group =>
        {
            var asistencias = group.Where(x => x.Asistencia is not null).ToList();
            return new DashboardAreaResponse
            {
                Area = group.Key,
                HorariosProgramados = group.Count(),
                Asistencias = asistencias.Count,
                Tardanzas = asistencias.Count(x => x.Asistencia!.EstadoAsistencia == "Tardanza"),
                HorasTrabajadas = Math.Round(asistencias.Sum(x => x.Asistencia!.HorasTrabajadasReal ?? 0), 2),
                Cumplimiento = group.Count() == 0 ? 0 : Math.Round((decimal)asistencias.Count / group.Count() * 100, 2)
            };
        }).OrderByDescending(x => x.Cumplimiento));
    }

    [HttpGet("tendencia")]
    public async Task<ActionResult<IEnumerable<DashboardTendenciaResponse>>> Tendencia([FromQuery] int meses = 6)
    {
        meses = Math.Clamp(meses, 1, 12);
        var desde = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-(meses - 1));
        var rows = await db.Asistencias
            .Where(x => x.FechaHoraEntrada >= desde)
            .Select(x => new { x.FechaHoraEntrada, x.EstadoAsistencia, x.HorasTrabajadasReal })
            .ToListAsync();

        return Ok(rows.GroupBy(x => new { x.FechaHoraEntrada.Year, x.FechaHoraEntrada.Month })
            .OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Month)
            .Select(group => new DashboardTendenciaResponse
            {
                Periodo = $"{group.Key.Year}-{group.Key.Month:00}",
                HorasTrabajadas = Math.Round(group.Sum(x => x.HorasTrabajadasReal ?? 0), 2),
                Asistencias = group.Count(),
                Tardanzas = group.Count(x => x.EstadoAsistencia == "Tardanza")
            }));
    }
}
