using Microsoft.EntityFrameworkCore;
using WorkForceManagerAPI.Data;
using WorkForceManagerAPI.Models.Entities;
using WorkForceManagerAPI.Repositories.Interfaces;

namespace WorkForceManagerAPI.Repositories;

public class AuthRepository(AppDbContext db) : IAuthRepository
{
    private IQueryable<Usuario> ConRelaciones() =>
        db.Usuarios
          .Include(u => u.Empleado)
          .Include(u => u.Rol);

    public async Task<Usuario?> ObtenerPorEmailAsync(string email) =>
        await ConRelaciones()
            .FirstOrDefaultAsync(u => u.Email == email && u.Activo);

    public async Task<Usuario?> ObtenerPorIdAsync(int usuarioId) =>
        await ConRelaciones()
            .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

    public async Task<IEnumerable<Usuario>> ObtenerTodosAsync() =>
        await ConRelaciones().OrderBy(u => u.Empleado.Apellidos).ThenBy(u => u.Empleado.Nombres).ToListAsync();

    public async Task<bool> ExisteEmailAsync(string email) =>
        await db.Usuarios.AnyAsync(u => u.Email == email);

    public async Task<bool> ExisteEmpleadoConUsuarioAsync(int empleadoId) =>
        await db.Usuarios.AnyAsync(u => u.EmpleadoId == empleadoId);

    public async Task<Usuario> CrearAsync(Usuario usuario)
    {
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        return await ObtenerPorIdAsync(usuario.UsuarioId) ?? usuario;
    }

    public async Task ActualizarAsync(Usuario usuario)
    {
        db.Usuarios.Update(usuario);
        await db.SaveChangesAsync();
    }
}
