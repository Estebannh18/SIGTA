namespace WorkForceManagerAPI.Models.DTOs.Response;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiracion { get; set; }
    public UsuarioInfoResponse Usuario { get; set; } = null!;
}

public class UsuarioInfoResponse
{
    public int UsuarioId { get; set; }
    public int EmpleadoId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public int RolId { get; set; }
}