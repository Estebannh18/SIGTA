using WorkForceManagerAPI.Models.Common;
using WorkForceManagerAPI.Models.DTOs.Request;
using WorkForceManagerAPI.Models.DTOs.Response;

namespace WorkForceManagerAPI.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<UsuarioInfoResponse>> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<bool>> CambiarPasswordAsync(int usuarioId, CambiarPasswordRequest request);
    Task<ApiResponse<UsuarioInfoResponse>> ObtenerPerfilAsync(int usuarioId);
}