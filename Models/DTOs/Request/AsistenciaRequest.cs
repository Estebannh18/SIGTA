using System.ComponentModel.DataAnnotations;

namespace WorkForceManagerAPI.Models.DTOs.Request;

public class RegistrarEntradaRequest
{
    [Required] public int EmpleadoId { get; set; }
    public DateTime? FechaHoraEntrada { get; set; }
}

public class RegistrarSalidaRequest
{
    [Required] public int EmpleadoId { get; set; }
    public DateTime? FechaHoraSalida { get; set; }
}

public class BuscarAsistenciaRequest
{
    public int? EmpleadoId { get; set; }
    public int? AreaId { get; set; }
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }
    public string? EstadoAsistencia { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}