using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkForceManagerAPI.Helpers;
using WorkForceManagerAPI.Models.DTOs.Request;
using WorkForceManagerAPI.Services.Interfaces;

namespace WorkForceManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AsistenciaController(IAsistenciaService service) : ControllerBase
{
    private bool EsEmpleado => JwtHelper.ObtenerRol(User) == "Empleado";
    private int EmpleadoIdPropio => JwtHelper.ObtenerEmpleadoId(User);

    [HttpGet]
    public async Task<IActionResult> Buscar([FromQuery] BuscarAsistenciaRequest filtro)
    {
        if (EsEmpleado)
        {
            if (EmpleadoIdPropio <= 0)
                return Forbid();
            filtro.EmpleadoId = EmpleadoIdPropio;
        }
        var result = await service.BuscarAsync(filtro);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var result = await service.ObtenerPorIdAsync(id);
        if (!result.Success) return NotFound(result);

        if (EsEmpleado && result.Data?.EmpleadoId != EmpleadoIdPropio)
            return Forbid();

        return Ok(result);
    }

    [HttpPost("entrada")]
    public async Task<IActionResult> RegistrarEntrada([FromBody] RegistrarEntradaRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (EsEmpleado)
        {
            if (EmpleadoIdPropio <= 0) return Forbid();
            if (request.EmpleadoId != EmpleadoIdPropio)
                return Forbid();
        }
        var result = await service.RegistrarEntradaAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("salida")]
    public async Task<IActionResult> RegistrarSalida([FromBody] RegistrarSalidaRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (EsEmpleado)
        {
            if (EmpleadoIdPropio <= 0) return Forbid();
            if (request.EmpleadoId != EmpleadoIdPropio)
                return Forbid();
        }
        var result = await service.RegistrarSalidaAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("resumen/{empleadoId:int}")]
    public async Task<IActionResult> ObtenerResumen(
        int empleadoId,
        [FromQuery] DateOnly fechaInicio,
        [FromQuery] DateOnly fechaFin)
    {
        if (EsEmpleado && empleadoId != EmpleadoIdPropio)
            return Forbid();

        var result = await service.ObtenerResumenAsync(empleadoId, fechaInicio, fechaFin);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("estado-hoy/{empleadoId:int}")]
    public async Task<IActionResult> ObtenerEstadoHoy(int empleadoId)
    {
        if (EsEmpleado && empleadoId != EmpleadoIdPropio)
            return Forbid();

        var result = await service.ObtenerEstadoHoyAsync(empleadoId);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
