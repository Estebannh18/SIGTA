namespace WorkForceManagerAPI.Models.DTOs.Response;

public class EmpleadoResponse
{
    public int EmpleadoId { get; set; }
    public string NumeroDocumento { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public int CargoId { get; set; }
    public string Cargo { get; set; } = string.Empty;
    public int AreaId { get; set; }
    public string Area { get; set; } = string.Empty;
    public DateOnly FechaIngreso { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
}
