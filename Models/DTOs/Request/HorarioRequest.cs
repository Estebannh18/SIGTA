using System.ComponentModel.DataAnnotations;

namespace WorkForceManagerAPI.Models.DTOs.Request;

public class CrearHorarioRequest
{
    [Required] public int EmpleadoId { get; set; }
    [Required] public int TipoTurnoId { get; set; }
    [Required] public DateOnly Fecha { get; set; }
    public string? Observaciones { get; set; }
}

public class AsignacionMasivaRequest
{
    [Required] public int AreaId { get; set; }
    [Required] public int TipoTurnoId { get; set; }
    [Required] public DateOnly FechaInicio { get; set; }
    [Required] public DateOnly FechaFin { get; set; }
    public bool SobreescribirExistentes { get; set; } = false;
}

public class BuscarHorarioRequest
{
    public int? EmpleadoId { get; set; }
    public int? AreaId { get; set; }
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}