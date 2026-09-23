namespace WorkForceManagerAPI.Models.Entities;
public class Usuario
{
    public int UsuarioId { get; set; }
    public int EmpleadoId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RolId { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime? UltimoAcceso { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public Empleado Empleado { get; set; } = null!;
    public Rol Rol { get; set; } = null!;
}
