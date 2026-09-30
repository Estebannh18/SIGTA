namespace WorkForceManagerAPI.Models.DTOs.Response;

public class CatalogoTurnoResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public decimal HorasEsperadas { get; set; }
}
