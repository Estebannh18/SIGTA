using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkForceManagerAPI.Data;
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
}
