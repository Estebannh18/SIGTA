using Microsoft.EntityFrameworkCore;
using WorkForceManagerAPI.Data;
using WorkForceManagerAPI.Models.DTOs.Request;
using WorkForceManagerAPI.Models.Entities;
using WorkForceManagerAPI.Repositories.Interfaces;

namespace WorkForceManagerAPI.Repositories;

public class EmpleadoRepository(AppDbContext db) : IEmpleadoRepository
{
    public async Task<(IEnumerable<Empleado> Items, int Total)> BuscarAsync(BuscarEmpleadoRequest filtro)
    {
        var query = db.Empleados
            .Include(e => e.Cargo)
            .Include(e => e.Area)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro.Termino))
        {
            var t = filtro.Termino.ToLower();
            query = query.Where(e =>
                e.NumeroDocumento.ToLower().Contains(t) ||
                e.Nombres.ToLower().Contains(t) ||
                e.Apellidos.ToLower().Contains(t));
        }

        if (filtro.AreaId.HasValue)   query = query.Where(e => e.AreaId == filtro.AreaId);
        if (filtro.CargoId.HasValue)  query = query.Where(e => e.CargoId == filtro.CargoId);
        if (filtro.Activo.HasValue)   query = query.Where(e => e.Activo == filtro.Activo);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(e => e.Apellidos).ThenBy(e => e.Nombres)
            .Skip((filtro.Page - 1) * filtro.PageSize)
            .Take(filtro.PageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Empleado?> ObtenerPorIdAsync(int id) =>
        await db.Empleados.Include(e => e.Cargo).Include(e => e.Area)
            .FirstOrDefaultAsync(e => e.EmpleadoId == id);

    public async Task<Empleado?> ObtenerPorDocumentoAsync(string documento) =>
        await db.Empleados.Include(e => e.Cargo).Include(e => e.Area)
            .FirstOrDefaultAsync(e => e.NumeroDocumento == documento);

    public async Task<Empleado> CrearAsync(Empleado empleado)
    {
        db.Empleados.Add(empleado);
        await db.SaveChangesAsync();
        return await ObtenerPorIdAsync(empleado.EmpleadoId) ?? empleado;
    }

    public async Task<Empleado> ActualizarAsync(Empleado empleado)
    {
        empleado.FechaModificacion = DateTime.Now;
        db.Empleados.Update(empleado);
        await db.SaveChangesAsync();
        return await ObtenerPorIdAsync(empleado.EmpleadoId) ?? empleado;
    }

    public async Task<bool> CambiarEstadoAsync(int id, bool activo)
    {
        var empleado = await db.Empleados.FindAsync(id);
        if (empleado is null) return false;
        empleado.Activo = activo;
        empleado.FechaModificacion = DateTime.Now;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExisteDocumentoAsync(string documento, int? excluirId = null) =>
        await db.Empleados.AnyAsync(e =>
            e.NumeroDocumento == documento &&
            (excluirId == null || e.EmpleadoId != excluirId));
}
