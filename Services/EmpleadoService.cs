using WorkForceManagerAPI.Models.Common;
using WorkForceManagerAPI.Models.DTOs.Request;
using WorkForceManagerAPI.Models.DTOs.Response;
using WorkForceManagerAPI.Models.Entities;
using WorkForceManagerAPI.Repositories.Interfaces;
using WorkForceManagerAPI.Services.Interfaces;

namespace WorkForceManagerAPI.Services;

public class EmpleadoService(IEmpleadoRepository repo) : IEmpleadoService
{
    public async Task<PagedResponse<EmpleadoResponse>> BuscarAsync(BuscarEmpleadoRequest filtro)
    {
        var (items, total) = await repo.BuscarAsync(filtro);
        return PagedResponse<EmpleadoResponse>.Create(
            items.Select(MapToResponse),
            total, filtro.Page, filtro.PageSize);
    }

    public async Task<ApiResponse<EmpleadoResponse>> ObtenerPorIdAsync(int id)
    {
        var empleado = await repo.ObtenerPorIdAsync(id);
        return empleado is null
            ? ApiResponse<EmpleadoResponse>.Fail($"Empleado con ID {id} no encontrado.")
            : ApiResponse<EmpleadoResponse>.Ok(MapToResponse(empleado));
    }

    public async Task<ApiResponse<EmpleadoResponse>> CrearAsync(CrearEmpleadoRequest request)
    {
        if (await repo.ExisteDocumentoAsync(request.NumeroDocumento))
            return ApiResponse<EmpleadoResponse>.Fail(
                $"Ya existe un empleado con el documento {request.NumeroDocumento}.");

        var empleado = new Empleado
        {
            NumeroDocumento = request.NumeroDocumento,
            Nombres         = request.Nombres,
            Apellidos       = request.Apellidos,
            CargoId         = request.CargoId,
            AreaId          = request.AreaId,
            FechaIngreso    = request.FechaIngreso
        };

        var creado = await repo.CrearAsync(empleado);
        return ApiResponse<EmpleadoResponse>.Ok(MapToResponse(creado), "Empleado creado correctamente.");
    }

    public async Task<ApiResponse<EmpleadoResponse>> ActualizarAsync(int id, ActualizarEmpleadoRequest request)
    {
        var empleado = await repo.ObtenerPorIdAsync(id);
        if (empleado is null)
            return ApiResponse<EmpleadoResponse>.Fail($"Empleado con ID {id} no encontrado.");

        empleado.Nombres      = request.Nombres;
        empleado.Apellidos    = request.Apellidos;
        empleado.CargoId      = request.CargoId;
        empleado.AreaId       = request.AreaId;
        empleado.FechaIngreso = request.FechaIngreso;

        var actualizado = await repo.ActualizarAsync(empleado);
        return ApiResponse<EmpleadoResponse>.Ok(MapToResponse(actualizado), "Empleado actualizado correctamente.");
    }

    public async Task<ApiResponse<bool>> CambiarEstadoAsync(int id, bool activo)
    {
        var ok = await repo.CambiarEstadoAsync(id, activo);
        return ok
            ? ApiResponse<bool>.Ok(true, activo ? "Empleado activado." : "Empleado desactivado.")
            : ApiResponse<bool>.Fail($"Empleado con ID {id} no encontrado.");
    }

    private static EmpleadoResponse MapToResponse(Empleado e) => new()
    {
        EmpleadoId      = e.EmpleadoId,
        NumeroDocumento = e.NumeroDocumento,
        NombreCompleto  = $"{e.Nombres} {e.Apellidos}",
        Nombres         = e.Nombres,
        Apellidos       = e.Apellidos,
        CargoId         = e.CargoId,
        Cargo           = e.Cargo?.Nombre ?? string.Empty,
        AreaId          = e.AreaId,
        Area            = e.Area?.Nombre ?? string.Empty,
        FechaIngreso    = e.FechaIngreso,
        Activo          = e.Activo,
        FechaCreacion   = e.FechaCreacion
    };
}
