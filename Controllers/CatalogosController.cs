using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkForceManagerAPI.Data;
using WorkForceManagerAPI.Models.DTOs.Request;
using WorkForceManagerAPI.Models.DTOs.Response;

namespace WorkForceManagerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CatalogosController(AppDbContext db) : ControllerBase
{
    /// <summary>Áreas activas para poblar selectores.</summary>
    [HttpGet("areas")]
    public async Task<ActionResult<IEnumerable<CatalogoResponse>>> Areas() =>
        Ok(await db.Areas
            .Where(a => a.Activo)
            .OrderBy(a => a.Nombre)
            .Select(a => new CatalogoResponse { Id = a.AreaId, Nombre = a.Nombre })
            .ToListAsync());

    /// <summary>Cargos activos para poblar selectores.</summary>
    [HttpGet("cargos")]
    public async Task<ActionResult<IEnumerable<CatalogoResponse>>> Cargos() =>
        Ok(await db.Cargos
            .Where(c => c.Activo)
            .OrderBy(c => c.Nombre)
            .Select(c => new CatalogoResponse { Id = c.CargoId, Nombre = c.Nombre })
            .ToListAsync());

    /// <summary>Tipos de turno activos para asignar horarios.</summary>
    [HttpGet("tipos-turno")]
    public async Task<ActionResult<IEnumerable<CatalogoTurnoResponse>>> TiposTurno() =>
        Ok(await db.TiposTurno
            .Where(t => t.Activo)
            .OrderBy(t => t.HoraInicio)
            .Select(t => new CatalogoTurnoResponse
            {
                Id = t.TipoTurnoId,
                Nombre = t.Nombre,
                HoraInicio = t.HoraInicio,
                HoraFin = t.HoraFin,
                HorasEsperadas = t.HorasEsperadas
            })
            .ToListAsync());

    [HttpGet("admin/areas")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<IEnumerable<CatalogoAdminResponse>>> TodasLasAreas() =>
        Ok(await db.Areas.OrderBy(a => a.Nombre).Select(a => new CatalogoAdminResponse
        {
            Id = a.AreaId, Nombre = a.Nombre, Descripcion = a.Descripcion, Activo = a.Activo
        }).ToListAsync());

    [HttpPost("areas")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CrearArea([FromBody] CatalogoRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (await db.Areas.AnyAsync(a => a.Nombre == request.Nombre.Trim()))
            return Conflict("Ya existe un área con ese nombre.");
        var area = new Models.Entities.Area { Nombre = request.Nombre.Trim(), Descripcion = request.Descripcion?.Trim() };
        db.Areas.Add(area);
        await db.SaveChangesAsync();
        return Created($"api/Catalogos/areas/{area.AreaId}", new CatalogoAdminResponse { Id = area.AreaId, Nombre = area.Nombre, Descripcion = area.Descripcion, Activo = area.Activo });
    }

    [HttpPut("areas/{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ActualizarArea(int id, [FromBody] CatalogoRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var area = await db.Areas.FindAsync(id);
        if (area is null) return NotFound();
        if (await db.Areas.AnyAsync(a => a.AreaId != id && a.Nombre == request.Nombre.Trim()))
            return Conflict("Ya existe un área con ese nombre.");
        area.Nombre = request.Nombre.Trim(); area.Descripcion = request.Descripcion?.Trim();
        await db.SaveChangesAsync();
        return Ok();
    }

    [HttpPatch("areas/{id:int}/{accion:regex(activar|desactivar)}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CambiarEstadoArea(int id, string accion) =>
        await CambiarEstadoAsync(db.Areas, id, accion == "activar");

    [HttpGet("admin/cargos")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<IEnumerable<CatalogoAdminResponse>>> TodosLosCargos() =>
        Ok(await db.Cargos.OrderBy(c => c.Nombre).Select(c => new CatalogoAdminResponse
        {
            Id = c.CargoId, Nombre = c.Nombre, Descripcion = c.Descripcion, Activo = c.Activo
        }).ToListAsync());

    [HttpPost("cargos")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CrearCargo([FromBody] CatalogoRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (await db.Cargos.AnyAsync(c => c.Nombre == request.Nombre.Trim()))
            return Conflict("Ya existe un cargo con ese nombre.");
        var cargo = new Models.Entities.Cargo { Nombre = request.Nombre.Trim(), Descripcion = request.Descripcion?.Trim() };
        db.Cargos.Add(cargo);
        await db.SaveChangesAsync();
        return Created($"api/Catalogos/cargos/{cargo.CargoId}", new CatalogoAdminResponse { Id = cargo.CargoId, Nombre = cargo.Nombre, Descripcion = cargo.Descripcion, Activo = cargo.Activo });
    }

    [HttpPut("cargos/{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ActualizarCargo(int id, [FromBody] CatalogoRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var cargo = await db.Cargos.FindAsync(id);
        if (cargo is null) return NotFound();
        if (await db.Cargos.AnyAsync(c => c.CargoId != id && c.Nombre == request.Nombre.Trim()))
            return Conflict("Ya existe un cargo con ese nombre.");
        cargo.Nombre = request.Nombre.Trim(); cargo.Descripcion = request.Descripcion?.Trim();
        await db.SaveChangesAsync();
        return Ok();
    }

    [HttpPatch("cargos/{id:int}/{accion:regex(activar|desactivar)}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CambiarEstadoCargo(int id, string accion) =>
        await CambiarEstadoAsync(db.Cargos, id, accion == "activar");

    [HttpGet("admin/tipos-turno")]
    [Authorize(Roles = "Administrador")]
    public async Task<ActionResult<IEnumerable<CatalogoAdminTurnoResponse>>> TodosLosTurnos() =>
        Ok(await db.TiposTurno.OrderBy(t => t.HoraInicio).Select(t => new CatalogoAdminTurnoResponse
        {
            Id = t.TipoTurnoId, Nombre = t.Nombre, Activo = t.Activo,
            HoraInicio = t.HoraInicio, HoraFin = t.HoraFin, HorasEsperadas = t.HorasEsperadas
        }).ToListAsync());

    [HttpPost("tipos-turno")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CrearTurno([FromBody] CatalogoTurnoRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (request.HoraFin <= request.HoraInicio)
            return BadRequest("La hora de fin debe ser posterior a la hora de inicio.");
        if (await db.TiposTurno.AnyAsync(t => t.Nombre == request.Nombre.Trim()))
            return Conflict("Ya existe un turno con ese nombre.");
        var turno = new Models.Entities.TipoTurno
        {
            Nombre = request.Nombre.Trim(), HoraInicio = request.HoraInicio,
            HoraFin = request.HoraFin, HorasEsperadas = request.HorasEsperadas
        };
        db.TiposTurno.Add(turno);
        await db.SaveChangesAsync();
        return Created($"api/Catalogos/tipos-turno/{turno.TipoTurnoId}", turno);
    }

    [HttpPut("tipos-turno/{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ActualizarTurno(int id, [FromBody] CatalogoTurnoRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (request.HoraFin <= request.HoraInicio)
            return BadRequest("La hora de fin debe ser posterior a la hora de inicio.");
        var turno = await db.TiposTurno.FindAsync(id);
        if (turno is null) return NotFound();
        if (await db.TiposTurno.AnyAsync(t => t.TipoTurnoId != id && t.Nombre == request.Nombre.Trim()))
            return Conflict("Ya existe un turno con ese nombre.");
        turno.Nombre = request.Nombre.Trim(); turno.HoraInicio = request.HoraInicio;
        turno.HoraFin = request.HoraFin; turno.HorasEsperadas = request.HorasEsperadas;
        await db.SaveChangesAsync();
        return Ok();
    }

    [HttpPatch("tipos-turno/{id:int}/{accion:regex(activar|desactivar)}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CambiarEstadoTurno(int id, string accion) =>
        await CambiarEstadoAsync(db.TiposTurno, id, accion == "activar");

    private async Task<IActionResult> CambiarEstadoAsync<T>(DbSet<T> set, int id, bool activo) where T : class
    {
        var entity = await set.FindAsync(id);
        if (entity is null) return new NotFoundResult();
        var property = typeof(T).GetProperty(nameof(Models.Entities.Area.Activo));
        property?.SetValue(entity, activo);
        await db.SaveChangesAsync();
        return new OkResult();
    }
}
