using WorkForceManagerAPI.Models.DTOs.Request;
using WorkForceManagerAPI.Models.Entities;

namespace WorkForceManagerAPI.Repositories.Interfaces;

public interface IHorarioRepository
{
    Task<(IEnumerable<Horario> Items, int Total)> BuscarAsync(BuscarHorarioRequest filtro);
    Task<Horario?> ObtenerPorIdAsync(int id);
    Task<Horario?> ObtenerPorEmpleadoYFechaAsync(int empleadoId, DateOnly fecha);
    Task<Horario> CrearAsync(Horario horario);
    Task<bool> EliminarAsync(int id);
    Task<int> AsignacionMasivaAsync(int areaId, int tipoTurnoId, DateOnly fechaInicio,
        DateOnly fechaFin, int asignadoPorUsuarioId, bool sobreescribir);
}