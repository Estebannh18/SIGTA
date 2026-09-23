using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkForceManagerAPI.Helpers;
using WorkForceManagerAPI.Models.DTOs.Request;
using WorkForceManagerAPI.Services.Interfaces;

namespace WorkForceManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HorariosController(IHorarioService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Buscar([FromQuery] BuscarHorarioRequest filtro)
    {
        var result = await service.BuscarAsync(filtro);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var result = await service.ObtenerPorIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

[HttpPost]
[Authorize(Roles = "Administrador,Supervisor")]
public async Task<IActionResult> Crear([FromBody] CrearHorarioRequest request)
{
    if (!ModelState.IsValid) return BadRequest(ModelState);
    var usuarioId = JwtHelper.ObtenerUsuarioId(User);
    
    // Log temporal para debug
    Console.WriteLine($">>> UsuarioId del token: {usuarioId}");
    
    var result = await service.CrearAsync(request, usuarioId);
    return result.Success
        ? CreatedAtAction(nameof(ObtenerPorId), new { id = result.Data?.HorarioId }, result)
        : BadRequest(result);
}

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var result = await service.EliminarAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPost("asignacion-masiva")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<IActionResult> AsignacionMasiva([FromBody] AsignacionMasivaRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var usuarioId = JwtHelper.ObtenerUsuarioId(User);
        var result    = await service.AsignacionMasivaAsync(request, usuarioId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}