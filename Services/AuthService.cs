using WorkForceManagerAPI.Helpers;
using WorkForceManagerAPI.Models.Common;
using WorkForceManagerAPI.Models.DTOs.Request;
using WorkForceManagerAPI.Models.DTOs.Response;
using WorkForceManagerAPI.Models.Entities;
using WorkForceManagerAPI.Repositories.Interfaces;
using WorkForceManagerAPI.Services.Interfaces;

namespace WorkForceManagerAPI.Services;

public class AuthService(
    IAuthRepository authRepo,
    IEmpleadoRepository empleadoRepo,
    JwtHelper jwt) : IAuthService
{
    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var usuario = await authRepo.ObtenerPorEmailAsync(request.Email);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
            return ApiResponse<LoginResponse>.Fail("Credenciales incorrectas.");

        usuario.UltimoAcceso = DateTime.Now;
        await authRepo.ActualizarAsync(usuario);

        var (token, expiracion) = jwt.GenerarToken(usuario);

        return ApiResponse<LoginResponse>.Ok(new LoginResponse
        {
            Token      = token,
            Expiracion = expiracion,
            Usuario    = MapToUsuarioInfo(usuario)
        }, "Login exitoso.");
    }

    public async Task<ApiResponse<UsuarioInfoResponse>> RegisterAsync(RegisterRequest request)
    {
        if (await authRepo.ExisteEmailAsync(request.Email))
            return ApiResponse<UsuarioInfoResponse>.Fail($"El email {request.Email} ya está registrado.");

        if (await authRepo.ExisteEmpleadoConUsuarioAsync(request.EmpleadoId))
            return ApiResponse<UsuarioInfoResponse>.Fail("Este empleado ya tiene un usuario asignado.");

        var empleado = await empleadoRepo.ObtenerPorIdAsync(request.EmpleadoId);
        if (empleado is null)
            return ApiResponse<UsuarioInfoResponse>.Fail($"Empleado con ID {request.EmpleadoId} no encontrado.");

        if (!empleado.Activo)
            return ApiResponse<UsuarioInfoResponse>.Fail("No se puede crear usuario para un empleado inactivo.");

        var usuario = new Usuario
        {
            EmpleadoId   = request.EmpleadoId,
            Email        = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RolId        = request.RolId
        };

        var creado = await authRepo.CrearAsync(usuario);
        return ApiResponse<UsuarioInfoResponse>.Ok(
            MapToUsuarioInfo(creado), "Usuario registrado correctamente.");
    }

    public async Task<ApiResponse<bool>> CambiarPasswordAsync(int usuarioId, CambiarPasswordRequest request)
    {
        var usuario = await authRepo.ObtenerPorIdAsync(usuarioId);
        if (usuario is null)
            return ApiResponse<bool>.Fail("Usuario no encontrado.");

        if (!BCrypt.Net.BCrypt.Verify(request.PasswordActual, usuario.PasswordHash))
            return ApiResponse<bool>.Fail("La contraseña actual es incorrecta.");

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.PasswordNueva);
        await authRepo.ActualizarAsync(usuario);

        return ApiResponse<bool>.Ok(true, "Contraseña actualizada correctamente.");
    }

    public async Task<ApiResponse<UsuarioInfoResponse>> ObtenerPerfilAsync(int usuarioId)
    {
        var usuario = await authRepo.ObtenerPorIdAsync(usuarioId);
        return usuario is null
            ? ApiResponse<UsuarioInfoResponse>.Fail("Usuario no encontrado.")
            : ApiResponse<UsuarioInfoResponse>.Ok(MapToUsuarioInfo(usuario));
    }

    private static UsuarioInfoResponse MapToUsuarioInfo(Usuario u) => new()
    {
        UsuarioId      = u.UsuarioId,
        EmpleadoId     = u.EmpleadoId,
        NombreCompleto = $"{u.Empleado.Nombres} {u.Empleado.Apellidos}",
        Email          = u.Email,
        Rol            = u.Rol.Nombre,
        RolId          = u.RolId
    };
}