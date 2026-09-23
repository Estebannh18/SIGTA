namespace WorkForceManagerAPI.Models.Entities;
public class Empleado
{
    public int EmpleadoId { get; set; }
    public string NumeroDocumento { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public int CargoId { get; set; }
    public int AreaId { get; set; }
    public DateOnly FechaIngreso { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaModificacion { get; set; }
    public Cargo Cargo { get; set; } = null!;
    public Area Area { get; set; } = null!;
    public Usuario? Usuario { get; set; }
    public ICollection<Horario> Horarios { get; set; } = [];
    public ICollection<Asistencia> Asistencias { get; set; } = [];
}
