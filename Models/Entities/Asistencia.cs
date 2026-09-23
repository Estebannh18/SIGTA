namespace WorkForceManagerAPI.Models.Entities;
public class Asistencia
{
    public int AsistenciaId { get; set; }
    public int EmpleadoId { get; set; }
    public int? HorarioId { get; set; }
    public DateTime FechaHoraEntrada { get; set; }
    public DateTime? FechaHoraSalida { get; set; }
    public decimal? HorasTrabajadasReal { get; set; }
    public decimal MinutosRetraso { get; set; } = 0;
    public string EstadoAsistencia { get; set; } = "Presente";
    public string? Observaciones { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public Empleado Empleado { get; set; } = null!;
    public Horario? Horario { get; set; }
}
