using WorkForceManagerAPI.Models.Entities;

namespace WorkForceManagerAPI.Repositories.Interfaces;

public interface IAuthRepository
{
    Task<Usuario?> ObtenerPorEmailAsync(string email);
    Task<Usuario?> ObtenerPorIdAsync(int usuarioId);
    Task<IEnumerable<Usuario>> ObtenerTodosAsync();
    Task<bool> ExisteEmailAsync(string email);
    Task<bool> ExisteEmpleadoConUsuarioAsync(int empleadoId);
    Task<Usuario> CrearAsync(Usuario usuario);
    Task ActualizarAsync(Usuario usuario);
}
