namespace WorkForceManagerAPI.Models.DTOs.Response;

public class DashboardResumenResponse
{
    public DateOnly Fecha { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public int EmpleadosActivos { get; set; }
    public int HorariosProgramados { get; set; }
    public int AsistenciasRegistradas { get; set; }
    public int Tardanzas { get; set; }
    public decimal HorasTrabajadas { get; set; }
    public decimal Cumplimiento { get; set; }
}

public class DashboardAreaResponse
{
    public string Area { get; set; } = string.Empty;
    public int HorariosProgramados { get; set; }
    public int Asistencias { get; set; }
    public int Tardanzas { get; set; }
    public decimal HorasTrabajadas { get; set; }
    public decimal Cumplimiento { get; set; }
}

public class DashboardTendenciaResponse
{
    public DateOnly Fecha { get; set; }
    public string Periodo { get; set; } = string.Empty;
    public decimal HorasTrabajadas { get; set; }
    public int Asistencias { get; set; }
    public int Tardanzas { get; set; }
}
