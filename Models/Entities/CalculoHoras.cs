namespace WorkForceManagerAPI.Models.Entities;
public class CalculoHoras
{
    public int CalculoId { get; set; }
    public int EmpleadoId { get; set; }
    public int HorarioId { get; set; }
    public int? AsistenciaId { get; set; }
    public DateOnly Fecha { get; set; }
    public decimal HorasProgramadas { get; set; }
    public decimal HorasTrabajadasReal { get; set; }
    public decimal HorasExtras { get; set; }
    public decimal HorasFaltantes { get; set; }
    public decimal MinutosRetraso { get; set; }
    public decimal PorcentajeCumplimiento { get; set; }
    public DateTime FechaCalculo { get; set; } = DateTime.Now;
    public Empleado Empleado { get; set; } = null!;
    public Horario Horario { get; set; } = null!;
}
