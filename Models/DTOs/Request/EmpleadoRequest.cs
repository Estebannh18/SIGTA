using System.ComponentModel.DataAnnotations;

namespace WorkForceManagerAPI.Models.DTOs.Request;

public class CrearEmpleadoRequest
{
    [Required(ErrorMessage = "El número de documento es requerido.")]
    [MaxLength(20)]
    public string NumeroDocumento { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los nombres son requeridos.")]
    [MaxLength(100)]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son requeridos.")]
    [MaxLength(100)]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "El cargo es requerido.")]
    public int CargoId { get; set; }

    [Required(ErrorMessage = "El área es requerida.")]
    public int AreaId { get; set; }

    [Required(ErrorMessage = "La fecha de ingreso es requerida.")]
    public DateOnly FechaIngreso { get; set; }
}

public class ActualizarEmpleadoRequest
{
    [Required][MaxLength(100)] public string Nombres { get; set; } = string.Empty;
    [Required][MaxLength(100)] public string Apellidos { get; set; } = string.Empty;
    [Required] public int CargoId { get; set; }
    [Required] public int AreaId { get; set; }
    [Required] public DateOnly FechaIngreso { get; set; }
}

public class BuscarEmpleadoRequest
{
    public string? Termino { get; set; }
    public int? AreaId { get; set; }
    public int? CargoId { get; set; }
    public bool? Activo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
