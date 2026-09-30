using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkForceManagerAPI.Models.DTOs.Response;
using WorkForceManagerAPI.Services.Interfaces;

namespace WorkForceManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportesController(
    IReporteService service,
    IReporteExportService export) : ControllerBase
{
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);
    private const string ExcelMime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private const string PdfMime = "application/pdf";

    private (DateOnly Inicio, DateOnly Fin) Rango(DateOnly? inicio, DateOnly? fin) =>
        (inicio ?? new DateOnly(Hoy.Year, Hoy.Month, 1), fin ?? Hoy);

    [HttpGet("horas")]
    public async Task<ActionResult<IEnumerable<ReporteHorasRow>>> Horas(
        [FromQuery] DateOnly? fechaInicio, [FromQuery] DateOnly? fechaFin)
    {
        var (inicio, fin) = Rango(fechaInicio, fechaFin);
        return Ok(await service.HorasAsync(inicio, fin));
    }

    [HttpGet("horas/excel")]
    public async Task<IActionResult> HorasExcel([FromQuery] DateOnly? fechaInicio, [FromQuery] DateOnly? fechaFin)
    {
        var (inicio, fin) = Rango(fechaInicio, fechaFin);
        var rows = await service.HorasAsync(inicio, fin);
        var bytes = export.HorasExcel(rows, inicio, fin);
        return File(bytes, ExcelMime, $"reporte-horas-{inicio:yyyyMMdd}-{fin:yyyyMMdd}.xlsx");
    }

    [HttpGet("horas/pdf")]
    public async Task<IActionResult> HorasPdf([FromQuery] DateOnly? fechaInicio, [FromQuery] DateOnly? fechaFin)
    {
        var (inicio, fin) = Rango(fechaInicio, fechaFin);
        var rows = await service.HorasAsync(inicio, fin);
        var bytes = export.HorasPdf(rows, inicio, fin);
        return File(bytes, PdfMime, $"reporte-horas-{inicio:yyyyMMdd}-{fin:yyyyMMdd}.pdf");
    }

    [HttpGet("asistencia")]
    public async Task<ActionResult<IEnumerable<ReporteAsistenciaRow>>> Asistencia(
        [FromQuery] DateOnly? fechaInicio, [FromQuery] DateOnly? fechaFin)
    {
        var (inicio, fin) = Rango(fechaInicio, fechaFin);
        return Ok(await service.AsistenciaAsync(inicio, fin));
    }

    [HttpGet("asistencia/excel")]
    public async Task<IActionResult> AsistenciaExcel([FromQuery] DateOnly? fechaInicio, [FromQuery] DateOnly? fechaFin)
    {
        var (inicio, fin) = Rango(fechaInicio, fechaFin);
        var rows = await service.AsistenciaAsync(inicio, fin);
        var bytes = export.AsistenciaExcel(rows, inicio, fin);
        return File(bytes, ExcelMime, $"reporte-asistencia-{inicio:yyyyMMdd}-{fin:yyyyMMdd}.xlsx");
    }

    [HttpGet("asistencia/pdf")]
    public async Task<IActionResult> AsistenciaPdf([FromQuery] DateOnly? fechaInicio, [FromQuery] DateOnly? fechaFin)
    {
        var (inicio, fin) = Rango(fechaInicio, fechaFin);
        var rows = await service.AsistenciaAsync(inicio, fin);
        var bytes = export.AsistenciaPdf(rows, inicio, fin);
        return File(bytes, PdfMime, $"reporte-asistencia-{inicio:yyyyMMdd}-{fin:yyyyMMdd}.pdf");
    }
}
