using WorkForceManagerAPI.Models.Common;
using WorkForceManagerAPI.Models.DTOs.Request;
using WorkForceManagerAPI.Models.DTOs.Response;

namespace WorkForceManagerAPI.Services.Interfaces;

public interface IAsistenciaService
{
    Task<PagedResponse<AsistenciaResponse>> BuscarAsync(BuscarAsistenciaRequest filtro);
    Task<ApiResponse<AsistenciaResponse>> ObtenerPorIdAsync(int id);
    Task<ApiResponse<AsistenciaResponse>> RegistrarEntradaAsync(RegistrarEntradaRequest request);
    Task<ApiResponse<AsistenciaResponse>> RegistrarSalidaAsync(RegistrarSalidaRequest request);
    Task<ApiResponse<ResumenAsistenciaResponse>> ObtenerResumenAsync(int empleadoId, DateOnly fechaInicio, DateOnly fechaFin);
    Task<ApiResponse<EstadoAsistenciaHoyResponse>> ObtenerEstadoHoyAsync(int empleadoId);
}