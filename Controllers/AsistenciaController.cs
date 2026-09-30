using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkForceManagerAPI.Models.DTOs.Request;
using WorkForceManagerAPI.Services.Interfaces;

namespace WorkForceManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AsistenciaController(IAsistenciaService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Buscar([FromQuery] BuscarAsistenciaRequest filtro)
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

    [HttpPost("entrada")]
    public async Task<IActionResult> RegistrarEntrada([FromBody] RegistrarEntradaRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await service.RegistrarEntradaAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("salida")]
    public async Task<IActionResult> RegistrarSalida([FromBody] RegistrarSalidaRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await service.RegistrarSalidaAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("resumen/{empleadoId:int}")]
    public async Task<IActionResult> ObtenerResumen(
        int empleadoId,
        [FromQuery] DateOnly fechaInicio,
        [FromQuery] DateOnly fechaFin)
    {
        var result = await service.ObtenerResumenAsync(empleadoId, fechaInicio, fechaFin);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("estado-hoy/{empleadoId:int}")]
    public async Task<IActionResult> ObtenerEstadoHoy(int empleadoId)
    {
        var result = await service.ObtenerEstadoHoyAsync(empleadoId);
        return result.Success ? Ok(result) : NotFound(result);
    }
}