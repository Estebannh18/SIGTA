using WorkForceManagerAPI.Models.Common;
using WorkForceManagerAPI.Models.DTOs.Request;
using WorkForceManagerAPI.Models.DTOs.Response;

namespace WorkForceManagerAPI.Services.Interfaces;

public interface IEmpleadoService
{
    Task<PagedResponse<EmpleadoResponse>> BuscarAsync(BuscarEmpleadoRequest filtro);
    Task<ApiResponse<EmpleadoResponse>> ObtenerPorIdAsync(int id);
    Task<ApiResponse<EmpleadoResponse>> CrearAsync(CrearEmpleadoRequest request);
    Task<ApiResponse<EmpleadoResponse>> ActualizarAsync(int id, ActualizarEmpleadoRequest request);
    Task<ApiResponse<bool>> CambiarEstadoAsync(int id, bool activo);
}
