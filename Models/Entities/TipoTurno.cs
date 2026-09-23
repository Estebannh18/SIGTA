namespace WorkForceManagerAPI.Models.Entities;
public class TipoTurno
{
    public int TipoTurnoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public decimal HorasEsperadas { get; set; }
    public bool Activo { get; set; } = true;
    public ICollection<Horario> Horarios { get; set; } = [];
}
