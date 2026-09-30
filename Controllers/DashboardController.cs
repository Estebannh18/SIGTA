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
    public async Task<ActionResult<DashboardResumenResponse>> Resumen(
        [FromQuery] DateOnly? fecha,
        [FromQuery] DateOnly? fechaInicio,
        [FromQuery] DateOnly? fechaFin)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var inicio = fechaInicio ?? fecha ?? hoy;
        var fin = fechaFin ?? (fechaInicio.HasValue ? hoy : inicio);

        var empleadosActivos = await db.Empleados.CountAsync(x => x.Activo);
        var horarios = await db.Horarios
            .Where(x => x.Fecha >= inicio && x.Fecha <= fin)
            .Select(x => new { x.HorarioId })
            .ToListAsync();
        var asistencias = await db.Asistencias
            .Where(x => DateOnly.FromDateTime(x.FechaHoraEntrada) >= inicio
                     && DateOnly.FromDateTime(x.FechaHoraEntrada) <= fin)
            .Select(x => new { x.HorarioId, x.EstadoAsistencia, x.HorasTrabajadasReal })
            .ToListAsync();

        var horariosIds = horarios.Select(x => x.HorarioId).ToHashSet();
        var asistenciasConHorario = asistencias.Count(x => x.HorarioId.HasValue && horariosIds.Contains(x.HorarioId.Value));

        return Ok(new DashboardResumenResponse
        {
            Fecha = inicio,
            FechaInicio = inicio,
            FechaFin = fin,
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
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var inicio = fechaInicio ?? hoy.AddDays(-29);
        var fin = fechaFin ?? hoy;

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
    public async Task<ActionResult<IEnumerable<DashboardTendenciaResponse>>> Tendencia(
        [FromQuery] DateOnly? fechaInicio,
        [FromQuery] DateOnly? fechaFin,
        [FromQuery] int meses = 6)
    {
        meses = Math.Clamp(meses, 1, 12);
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        DateOnly inicio;
        DateOnly fin;
        if (fechaInicio.HasValue)
        {
            inicio = fechaInicio.Value;
            fin = fechaFin ?? hoy;
        }
        else
        {
            fin = hoy;
            inicio = DateOnly.FromDateTime(new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-(meses - 1)));
        }

        var desde = inicio.ToDateTime(TimeOnly.MinValue);
        var hasta = fin.ToDateTime(TimeOnly.MaxValue);

        var rows = await db.Asistencias
            .Where(x => x.FechaHoraEntrada >= desde && x.FechaHoraEntrada <= hasta)
            .Select(x => new { x.FechaHoraEntrada, x.EstadoAsistencia, x.HorasTrabajadasReal })
            .ToListAsync();

        var dias = fin.DayNumber - inicio.DayNumber + 1;

        if (dias <= 35)
        {
            return Ok(rows.GroupBy(x => DateOnly.FromDateTime(x.FechaHoraEntrada))
                .OrderBy(g => g.Key)
                .Select(group => new DashboardTendenciaResponse
                {
                    Fecha = group.Key,
                    Periodo = group.Key.ToString("yyyy-MM-dd"),
                    HorasTrabajadas = Math.Round(group.Sum(x => x.HorasTrabajadasReal ?? 0), 2),
                    Asistencias = group.Count(),
                    Tardanzas = group.Count(x => x.EstadoAsistencia == "Tardanza")
                }));
        }

        return Ok(rows.GroupBy(x => new { x.FechaHoraEntrada.Year, x.FechaHoraEntrada.Month })
            .OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Month)
            .Select(group =>
            {
                var fecha = new DateOnly(group.Key.Year, group.Key.Month, 1);
                return new DashboardTendenciaResponse
                {
                    Fecha = fecha,
                    Periodo = fecha.ToString("yyyy-MM"),
                    HorasTrabajadas = Math.Round(group.Sum(x => x.HorasTrabajadasReal ?? 0), 2),
                    Asistencias = group.Count(),
                    Tardanzas = group.Count(x => x.EstadoAsistencia == "Tardanza")
                };
            }));
    }
}