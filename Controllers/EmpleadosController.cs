using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkForceManagerAPI.Models.DTOs.Request;
using WorkForceManagerAPI.Services.Interfaces;

namespace WorkForceManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Supervisor")]
public class EmpleadosController(IEmpleadoService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Buscar([FromQuery] BuscarEmpleadoRequest filtro)
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
    public async Task<IActionResult> Crear([FromBody] CrearEmpleadoRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await service.CrearAsync(request);
        return result.Success ? CreatedAtAction(nameof(ObtenerPorId),
            new { id = result.Data?.EmpleadoId }, result) : BadRequest(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarEmpleadoRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await service.ActualizarAsync(id, request);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPatch("{id:int}/activar")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Activar(int id)
    {
        var result = await service.CambiarEstadoAsync(id, true);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpPatch("{id:int}/desactivar")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Desactivar(int id)
    {
        var result = await service.CambiarEstadoAsync(id, false);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
