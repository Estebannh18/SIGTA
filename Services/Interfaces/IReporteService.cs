using WorkForceManagerAPI.Models.DTOs.Response;

namespace WorkForceManagerAPI.Services.Interfaces;

public interface IReporteService
{
    Task<IEnumerable<ReporteHorasRow>> HorasAsync(DateOnly fechaInicio, DateOnly fechaFin);
    Task<IEnumerable<ReporteAsistenciaRow>> AsistenciaAsync(DateOnly fechaInicio, DateOnly fechaFin);
}
