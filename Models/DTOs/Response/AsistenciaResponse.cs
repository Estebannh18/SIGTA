namespace WorkForceManagerAPI.Models.DTOs.Response;

public class AsistenciaResponse
{
    public int AsistenciaId { get; set; }
    public int EmpleadoId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public DateTime FechaHoraEntrada { get; set; }
    public DateTime? FechaHoraSalida { get; set; }
    public decimal? HorasTrabajadasReal { get; set; }
    public decimal MinutosRetraso { get; set; }
    public string EstadoAsistencia { get; set; } = string.Empty;
    public decimal? HorasProgramadas { get; set; }
    public decimal? HorasExtras { get; set; }
    public decimal? HorasFaltantes { get; set; }
    public decimal? PorcentajeCumplimiento { get; set; }
    public string? Observaciones { get; set; }
}

public class ResumenAsistenciaResponse
{
    public int EmpleadoId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public int DiasConRegistro { get; set; }
    public int Presentes { get; set; }
    public int Tardanzas { get; set; }
    public int AusenciasJustificadas { get; set; }
    public int AusenciasInjustificadas { get; set; }
    public decimal TotalHorasTrabajadas { get; set; }
    public decimal TotalHorasExtras { get; set; }
    public decimal TotalHorasFaltantes { get; set; }
    public decimal CumplimientoPromedio { get; set; }
}

public class EstadoAsistenciaHoyResponse
{
    public int EmpleadoId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public bool TieneHorarioHoy { get; set; }
    public string? TipoTurno { get; set; }
    public TimeOnly? HoraInicioProgramada { get; set; }
    public TimeOnly? HoraFinProgramada { get; set; }
    public bool TieneEntradaActiva { get; set; }
    public bool YaRegistroHoy { get; set; }
    public DateTime? FechaHoraEntrada { get; set; }
    public string? EstadoAsistencia { get; set; }
    public decimal? HorasTrabajadasReal { get; set; }
}