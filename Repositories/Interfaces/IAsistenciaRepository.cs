using WorkForceManagerAPI.Models.DTOs.Request;
using WorkForceManagerAPI.Models.Entities;

namespace WorkForceManagerAPI.Repositories.Interfaces;

public interface IAsistenciaRepository
{
    Task<(IEnumerable<Asistencia> Items, int Total)> BuscarAsync(BuscarAsistenciaRequest filtro);
    Task<Asistencia?> ObtenerPorIdAsync(int id);
    Task<Asistencia?> ObtenerEntradaActivaAsync(int empleadoId);
    Task<Asistencia> CrearAsync(Asistencia asistencia);
    Task<Asistencia> ActualizarAsync(Asistencia asistencia);
    Task<bool> TieneEntradaHoyAsync(int empleadoId);
}