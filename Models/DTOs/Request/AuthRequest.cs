using System.ComponentModel.DataAnnotations;

namespace WorkForceManagerAPI.Models.DTOs.Request;

public class LoginRequest
{
    [Required(ErrorMessage = "El email es requerido.")]
    [EmailAddress(ErrorMessage = "Formato de email inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida.")]
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [Required(ErrorMessage = "El EmpleadoId es requerido.")]
    public int EmpleadoId { get; set; }

    [Required(ErrorMessage = "El email es requerido.")]
    [EmailAddress(ErrorMessage = "Formato de email inválido.")]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener mínimo 8 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El RolId es requerido.")]
    public int RolId { get; set; }
}

public class CambiarPasswordRequest
{
    [Required] public string PasswordActual { get; set; } = string.Empty;
    [Required][MinLength(8)] public string PasswordNueva { get; set; } = string.Empty;
}