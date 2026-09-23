namespace WorkForceManagerAPI.Models.DTOs.Response;

public class HorarioResponse
{
    public int HorarioId { get; set; }
    public int EmpleadoId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string TipoTurno { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public TimeOnly HoraInicioProgramada { get; set; }
    public TimeOnly HoraFinProgramada { get; set; }
    public decimal HorasProgramadas { get; set; }
    public string AsignadoPor { get; set; } = string.Empty;
    public DateTime FechaAsignacion { get; set; }
    public string? Observaciones { get; set; }
}

public class AsignacionMasivaResponse
{
    public int HorariosAsignados { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public string Area { get; set; } = string.Empty;
    public string TipoTurno { get; set; } = string.Empty;
}