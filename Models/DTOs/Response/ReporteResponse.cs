namespace WorkForceManagerAPI.Models.DTOs.Response;

public class ReporteHorasRow
{
    public int EmpleadoId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public int DiasProgramados { get; set; }
    public int DiasAsistidos { get; set; }
    public decimal HorasProgramadas { get; set; }
    public decimal HorasTrabajadas { get; set; }
    public decimal HorasExtras { get; set; }
    public decimal HorasFaltantes { get; set; }
    public int Tardanzas { get; set; }
    public decimal Cumplimiento { get; set; }
}

public class ReporteAsistenciaRow
{
    public DateOnly Fecha { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public DateTime HoraEntrada { get; set; }
    public DateTime? HoraSalida { get; set; }
    public decimal HorasTrabajadas { get; set; }
    public decimal MinutosRetraso { get; set; }
    public string Estado { get; set; } = string.Empty;
}
