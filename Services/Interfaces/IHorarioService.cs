using WorkForceManagerAPI.Models.Common;
using WorkForceManagerAPI.Models.DTOs.Request;
using WorkForceManagerAPI.Models.DTOs.Response;

namespace WorkForceManagerAPI.Services.Interfaces;

public interface IHorarioService
{
    Task<PagedResponse<HorarioResponse>> BuscarAsync(BuscarHorarioRequest filtro);
    Task<ApiResponse<HorarioResponse>> ObtenerPorIdAsync(int id);
    Task<ApiResponse<HorarioResponse>> CrearAsync(CrearHorarioRequest request, int usuarioId);
    Task<ApiResponse<bool>> EliminarAsync(int id);
    Task<ApiResponse<AsignacionMasivaResponse>> AsignacionMasivaAsync(AsignacionMasivaRequest request, int usuarioId);
}