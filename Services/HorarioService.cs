using Microsoft.EntityFrameworkCore;
using WorkForceManagerAPI.Data;
using WorkForceManagerAPI.Models.Common;
using WorkForceManagerAPI.Models.DTOs.Request;
using WorkForceManagerAPI.Models.DTOs.Response;
using WorkForceManagerAPI.Models.Entities;
using WorkForceManagerAPI.Repositories.Interfaces;
using WorkForceManagerAPI.Services.Interfaces;

namespace WorkForceManagerAPI.Services;

public class HorarioService(
    IHorarioRepository horarioRepo,
    IEmpleadoRepository empleadoRepo,
    AppDbContext db) : IHorarioService
{
    public async Task<PagedResponse<HorarioResponse>> BuscarAsync(BuscarHorarioRequest filtro)
    {
        var (items, total) = await horarioRepo.BuscarAsync(filtro);
        return PagedResponse<HorarioResponse>.Create(
            items.Select(MapToResponse),
            total, filtro.Page, filtro.PageSize);
    }

    public async Task<ApiResponse<HorarioResponse>> ObtenerPorIdAsync(int id)
    {
        var horario = await horarioRepo.ObtenerPorIdAsync(id);
        return horario is null
            ? ApiResponse<HorarioResponse>.Fail($"Horario con ID {id} no encontrado.")
            : ApiResponse<HorarioResponse>.Ok(MapToResponse(horario));
    }

    public async Task<ApiResponse<HorarioResponse>> CrearAsync(CrearHorarioRequest request, int usuarioId)
    {
        if (!await db.Usuarios.AnyAsync(u => u.UsuarioId == usuarioId && u.Activo))
            return ApiResponse<HorarioResponse>.Fail(
                $"El usuario asignador con ID {usuarioId} no existe o está inactivo.");

        // Validar que el empleado existe
        var empleado = await empleadoRepo.ObtenerPorIdAsync(request.EmpleadoId);
        if (empleado is null)
            return ApiResponse<HorarioResponse>.Fail($"Empleado con ID {request.EmpleadoId} no encontrado.");

        if (!empleado.Activo)
            return ApiResponse<HorarioResponse>.Fail("No se puede asignar horario a un empleado inactivo.");

        // Validar que no tenga horario ese día
        var existente = await horarioRepo.ObtenerPorEmpleadoYFechaAsync(request.EmpleadoId, request.Fecha);
        if (existente is not null)
            return ApiResponse<HorarioResponse>.Fail(
                $"El empleado ya tiene un horario asignado para el {request.Fecha:dd/MM/yyyy}.");

        // Cargar TipoTurno para obtener las horas reales
        var turno = await db.TiposTurno.FindAsync(request.TipoTurnoId);
        if (turno is null)
            return ApiResponse<HorarioResponse>.Fail($"Tipo de turno con ID {request.TipoTurnoId} no encontrado.");

        var horario = new Horario
        {
            EmpleadoId           = request.EmpleadoId,
            TipoTurnoId          = request.TipoTurnoId,
            Fecha                = request.Fecha,
            HoraInicioProgramada = turno.HoraInicio,
            HoraFinProgramada    = turno.HoraFin,
            HorasProgramadas     = turno.HorasEsperadas,
            AsignadoPorUsuarioId = usuarioId,
            Observaciones        = request.Observaciones
        };

        var creado = await horarioRepo.CrearAsync(horario);
        return ApiResponse<HorarioResponse>.Ok(MapToResponse(creado), "Horario creado correctamente.");
    }

    public async Task<ApiResponse<bool>> EliminarAsync(int id)
    {
        var ok = await horarioRepo.EliminarAsync(id);
        return ok
            ? ApiResponse<bool>.Ok(true, "Horario eliminado correctamente.")
            : ApiResponse<bool>.Fail($"Horario con ID {id} no encontrado.");
    }

    public async Task<ApiResponse<AsignacionMasivaResponse>> AsignacionMasivaAsync(
        AsignacionMasivaRequest request, int usuarioId)
    {
        if (!await db.Usuarios.AnyAsync(u => u.UsuarioId == usuarioId && u.Activo))
            return ApiResponse<AsignacionMasivaResponse>.Fail(
                $"El usuario asignador con ID {usuarioId} no existe o está inactivo.");

        if (request.FechaFin < request.FechaInicio)
            return ApiResponse<AsignacionMasivaResponse>.Fail("La fecha fin no puede ser menor a la fecha inicio.");

        var turno = await db.TiposTurno.FindAsync(request.TipoTurnoId);
        if (turno is null)
            return ApiResponse<AsignacionMasivaResponse>.Fail($"Tipo de turno con ID {request.TipoTurnoId} no encontrado.");

        var area = await db.Areas.FindAsync(request.AreaId);
        if (area is null)
            return ApiResponse<AsignacionMasivaResponse>.Fail($"Área con ID {request.AreaId} no encontrada.");

        var total = await horarioRepo.AsignacionMasivaAsync(
            request.AreaId, request.TipoTurnoId,
            request.FechaInicio, request.FechaFin,
            usuarioId, request.SobreescribirExistentes);

        return ApiResponse<AsignacionMasivaResponse>.Ok(new AsignacionMasivaResponse
        {
            HorariosAsignados = total,
            FechaInicio       = request.FechaInicio,
            FechaFin          = request.FechaFin,
            Area              = area.Nombre,
            TipoTurno         = turno.Nombre
        }, $"Se asignaron {total} horarios correctamente.");
    }

    private static HorarioResponse MapToResponse(Horario h) => new()
    {
        HorarioId            = h.HorarioId,
        EmpleadoId           = h.EmpleadoId,
        NombreCompleto       = $"{h.Empleado.Nombres} {h.Empleado.Apellidos}",
        Area                 = h.Empleado.Area?.Nombre ?? string.Empty,
        TipoTurno            = h.TipoTurno?.Nombre ?? string.Empty,
        Fecha                = h.Fecha,
        HoraInicioProgramada = h.HoraInicioProgramada,
        HoraFinProgramada    = h.HoraFinProgramada,
        HorasProgramadas     = h.HorasProgramadas,
        AsignadoPor          = h.AsignadoPor is not null
            ? $"{h.AsignadoPor.Empleado.Nombres} {h.AsignadoPor.Empleado.Apellidos}"
            : string.Empty,
        FechaAsignacion      = h.FechaAsignacion,
        Observaciones        = h.Observaciones
    };
}
