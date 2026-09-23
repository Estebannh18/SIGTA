using Microsoft.EntityFrameworkCore;
using WorkForceManagerAPI.Data;
using WorkForceManagerAPI.Models.DTOs.Request;
using WorkForceManagerAPI.Models.Entities;
using WorkForceManagerAPI.Repositories.Interfaces;

namespace WorkForceManagerAPI.Repositories;

public class AsistenciaRepository(AppDbContext db) : IAsistenciaRepository
{
    private IQueryable<Asistencia> ConRelaciones() =>
        db.Asistencias
          .Include(a => a.Empleado).ThenInclude(e => e.Area)
          .Include(a => a.Empleado).ThenInclude(e => e.Cargo)
          .Include(a => a.Horario);

    public async Task<(IEnumerable<Asistencia> Items, int Total)> BuscarAsync(BuscarAsistenciaRequest filtro)
    {
        var query = ConRelaciones().AsQueryable();

        if (filtro.EmpleadoId.HasValue)
            query = query.Where(a => a.EmpleadoId == filtro.EmpleadoId);

        if (filtro.AreaId.HasValue)
            query = query.Where(a => a.Empleado.AreaId == filtro.AreaId);

        if (filtro.FechaInicio.HasValue)
            query = query.Where(a =>
                DateOnly.FromDateTime(a.FechaHoraEntrada) >= filtro.FechaInicio);

        if (filtro.FechaFin.HasValue)
            query = query.Where(a =>
                DateOnly.FromDateTime(a.FechaHoraEntrada) <= filtro.FechaFin);

        if (!string.IsNullOrWhiteSpace(filtro.EstadoAsistencia))
            query = query.Where(a => a.EstadoAsistencia == filtro.EstadoAsistencia);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.FechaHoraEntrada)
            .Skip((filtro.Page - 1) * filtro.PageSize)
            .Take(filtro.PageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Asistencia?> ObtenerPorIdAsync(int id) =>
        await ConRelaciones().FirstOrDefaultAsync(a => a.AsistenciaId == id);

    public async Task<Asistencia?> ObtenerEntradaActivaAsync(int empleadoId) =>
        await ConRelaciones().FirstOrDefaultAsync(a =>
            a.EmpleadoId == empleadoId &&
            a.FechaHoraSalida == null &&
            DateOnly.FromDateTime(a.FechaHoraEntrada) == DateOnly.FromDateTime(DateTime.Now));

    public async Task<bool> TieneEntradaHoyAsync(int empleadoId) =>
        await db.Asistencias.AnyAsync(a =>
            a.EmpleadoId == empleadoId &&
            DateOnly.FromDateTime(a.FechaHoraEntrada) == DateOnly.FromDateTime(DateTime.Now));

    public async Task<Asistencia> CrearAsync(Asistencia asistencia)
    {
        db.Asistencias.Add(asistencia);
        await db.SaveChangesAsync();
        return await ObtenerPorIdAsync(asistencia.AsistenciaId) ?? asistencia;
    }

    public async Task<Asistencia> ActualizarAsync(Asistencia asistencia)
    {
        db.Asistencias.Update(asistencia);
        await db.SaveChangesAsync();
        return await ObtenerPorIdAsync(asistencia.AsistenciaId) ?? asistencia;
    }
}