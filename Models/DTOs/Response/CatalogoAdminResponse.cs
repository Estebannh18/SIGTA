namespace WorkForceManagerAPI.Models.DTOs.Response;

public class CatalogoAdminResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
}

public class CatalogoAdminTurnoResponse : CatalogoAdminResponse
{
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public decimal HorasEsperadas { get; set; }
}
