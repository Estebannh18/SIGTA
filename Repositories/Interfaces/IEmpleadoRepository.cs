using WorkForceManagerAPI.Models.DTOs.Request;
using WorkForceManagerAPI.Models.Entities;

namespace WorkForceManagerAPI.Repositories.Interfaces;

public interface IEmpleadoRepository
{
    Task<(IEnumerable<Empleado> Items, int Total)> BuscarAsync(BuscarEmpleadoRequest filtro);
    Task<Empleado?> ObtenerPorIdAsync(int id);
    Task<Empleado?> ObtenerPorDocumentoAsync(string documento);
    Task<Empleado> CrearAsync(Empleado empleado);
    Task<Empleado> ActualizarAsync(Empleado empleado);
    Task<bool> CambiarEstadoAsync(int id, bool activo);
    Task<bool> ExisteDocumentoAsync(string documento, int? excluirId = null);
}
