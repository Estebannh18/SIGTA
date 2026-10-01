using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkForceManagerAPI.Helpers;
using WorkForceManagerAPI.Models.DTOs.Request;
using WorkForceManagerAPI.Services.Interfaces;

namespace WorkForceManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService service) : ControllerBase
{
    /// <summary>Login — devuelve JWT token</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await service.LoginAsync(request);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    /// <summary>Registrar nuevo usuario (solo Administrador)</summary>
    [HttpPost("register")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await service.RegisterAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("usuarios")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Usuarios() => Ok(await service.ObtenerUsuariosAsync());

    [HttpGet("usuarios/opciones")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> OpcionesRegistro() => Ok(await service.ObtenerOpcionesRegistroAsync());

    [HttpPatch("usuarios/{id:int}/{accion:regex(activar|desactivar)}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CambiarEstadoUsuario(int id, string accion)
    {
        if (id == JwtHelper.ObtenerUsuarioId(User) && accion == "desactivar")
            return BadRequest("No puedes desactivar tu propio usuario.");
        var result = await service.CambiarEstadoUsuarioAsync(id, accion == "activar");
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Cambiar contraseña (usuario autenticado)</summary>
    [HttpPatch("cambiar-password")]
    [Authorize]
    public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var usuarioId = JwtHelper.ObtenerUsuarioId(User);
        var result    = await service.CambiarPasswordAsync(usuarioId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Obtener perfil del usuario autenticado</summary>
    [HttpGet("perfil")]
    [Authorize]
    public async Task<IActionResult> Perfil()
    {
        var usuarioId = JwtHelper.ObtenerUsuarioId(User);
        var result    = await service.ObtenerPerfilAsync(usuarioId);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
