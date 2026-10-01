using System.ComponentModel.DataAnnotations;

namespace WorkForceManagerAPI.Models.DTOs.Request;

public class CatalogoRequest
{
    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }
}

public class CatalogoTurnoRequest
{
    [Required, MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public TimeOnly HoraInicio { get; set; }

    [Required]
    public TimeOnly HoraFin { get; set; }

    [Range(0.01, 24)]
    public decimal HorasEsperadas { get; set; }
}
