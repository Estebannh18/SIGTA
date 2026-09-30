using WorkForceManagerAPI.Models.DTOs.Response;

namespace WorkForceManagerAPI.Services.Interfaces;

public interface IReporteExportService
{
    byte[] HorasExcel(IEnumerable<ReporteHorasRow> rows, DateOnly fechaInicio, DateOnly fechaFin);
    byte[] HorasPdf(IEnumerable<ReporteHorasRow> rows, DateOnly fechaInicio, DateOnly fechaFin);
    byte[] AsistenciaExcel(IEnumerable<ReporteAsistenciaRow> rows, DateOnly fechaInicio, DateOnly fechaFin);
    byte[] AsistenciaPdf(IEnumerable<ReporteAsistenciaRow> rows, DateOnly fechaInicio, DateOnly fechaFin);
}
