namespace WorkForceManagerAPI.Models.Entities;
public class Horario
{
    public int HorarioId { get; set; }
    public int EmpleadoId { get; set; }
    public int TipoTurnoId { get; set; }
    public DateOnly Fecha { get; set; }
    public TimeOnly HoraInicioProgramada { get; set; }
    public TimeOnly HoraFinProgramada { get; set; }
    public decimal HorasProgramadas { get; set; }
    public int AsignadoPorUsuarioId { get; set; }
    public DateTime FechaAsignacion { get; set; } = DateTime.Now;
    public string? Observaciones { get; set; }
    public Empleado Empleado { get; set; } = null!;
    public TipoTurno TipoTurno { get; set; } = null!;
    public Usuario AsignadoPor { get; set; } = null!;
    public ICollection<Asistencia> Asistencias { get; set; } = [];
}
