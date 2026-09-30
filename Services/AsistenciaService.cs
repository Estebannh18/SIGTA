using WorkForceManagerAPI.Models.Common;
using WorkForceManagerAPI.Models.DTOs.Request;
using WorkForceManagerAPI.Models.DTOs.Response;
using WorkForceManagerAPI.Models.Entities;
using WorkForceManagerAPI.Repositories.Interfaces;
using WorkForceManagerAPI.Services.Interfaces;

namespace WorkForceManagerAPI.Services;

public class AsistenciaService(
    IAsistenciaRepository asistenciaRepo,
    IEmpleadoRepository empleadoRepo,
    IHorarioRepository horarioRepo) : IAsistenciaService
{
    public async Task<PagedResponse<AsistenciaResponse>> BuscarAsync(BuscarAsistenciaRequest filtro)
    {
        var (items, total) = await asistenciaRepo.BuscarAsync(filtro);
        return PagedResponse<AsistenciaResponse>.Create(
            items.Select(MapToResponse),
            total, filtro.Page, filtro.PageSize);
    }

    public async Task<ApiResponse<AsistenciaResponse>> ObtenerPorIdAsync(int id)
    {
        var asistencia = await asistenciaRepo.ObtenerPorIdAsync(id);
        return asistencia is null
            ? ApiResponse<AsistenciaResponse>.Fail($"Asistencia con ID {id} no encontrada.")
            : ApiResponse<AsistenciaResponse>.Ok(MapToResponse(asistencia));
    }

    public async Task<ApiResponse<AsistenciaResponse>> RegistrarEntradaAsync(RegistrarEntradaRequest request)
    {
        // Validar empleado
        var empleado = await empleadoRepo.ObtenerPorIdAsync(request.EmpleadoId);
        if (empleado is null)
            return ApiResponse<AsistenciaResponse>.Fail($"Empleado con ID {request.EmpleadoId} no encontrado.");

        if (!empleado.Activo)
            return ApiResponse<AsistenciaResponse>.Fail("El empleado está inactivo.");

        // Validar que no tenga entrada activa hoy
        if (await asistenciaRepo.TieneEntradaHoyAsync(request.EmpleadoId))
            return ApiResponse<AsistenciaResponse>.Fail("El empleado ya tiene una entrada registrada hoy.");

        var fechaHoraEntrada = request.FechaHoraEntrada ?? DateTime.Now;
        var fechaHoy         = DateOnly.FromDateTime(fechaHoraEntrada);

        // Buscar horario del día para calcular retraso
        var horario = await horarioRepo.ObtenerPorEmpleadoYFechaAsync(request.EmpleadoId, fechaHoy);

        decimal minutosRetraso   = 0;
        var estadoAsistencia     = "Presente";
        int? horarioId           = horario?.HorarioId;

        if (horario is not null)
        {
            var entradaProgramada = fechaHoy.ToDateTime(horario.HoraInicioProgramada);
            var tolerancia        = entradaProgramada.AddMinutes(15);

            if (fechaHoraEntrada > tolerancia)
            {
                minutosRetraso   = (decimal)(fechaHoraEntrada - entradaProgramada).TotalMinutes;
                estadoAsistencia = "Tardanza";
            }
        }

        var asistencia = new Asistencia
        {
            EmpleadoId       = request.EmpleadoId,
            HorarioId        = horarioId,
            FechaHoraEntrada = fechaHoraEntrada,
            MinutosRetraso   = minutosRetraso,
            EstadoAsistencia = estadoAsistencia
        };

        var creada = await asistenciaRepo.CrearAsync(asistencia);
        return ApiResponse<AsistenciaResponse>.Ok(MapToResponse(creada), 
            $"Entrada registrada. Estado: {estadoAsistencia}.");
    }

    public async Task<ApiResponse<AsistenciaResponse>> RegistrarSalidaAsync(RegistrarSalidaRequest request)
    {
        // Buscar entrada activa de hoy
        var asistencia = await asistenciaRepo.ObtenerEntradaActivaAsync(request.EmpleadoId);
        if (asistencia is null)
            return ApiResponse<AsistenciaResponse>.Fail("No se encontró entrada activa para hoy.");

        var fechaHoraSalida = request.FechaHoraSalida ?? DateTime.Now;

        if (fechaHoraSalida <= asistencia.FechaHoraEntrada)
            return ApiResponse<AsistenciaResponse>.Fail("La hora de salida no puede ser menor a la hora de entrada.");

        // Calcular horas trabajadas
        var horasTrabajadas = (decimal)(fechaHoraSalida - asistencia.FechaHoraEntrada).TotalHours;
        asistencia.FechaHoraSalida    = fechaHoraSalida;
        asistencia.HorasTrabajadasReal = Math.Round(horasTrabajadas, 2);

        // Actualizar estado si salió temprano
        if (asistencia.Horario is not null)
        {
            var salidaProgramada = DateOnly.FromDateTime(asistencia.FechaHoraEntrada)
                .ToDateTime(asistencia.Horario.HoraFinProgramada);

            if (fechaHoraSalida < salidaProgramada.AddMinutes(-15) &&
                asistencia.EstadoAsistencia == "Presente")
                asistencia.EstadoAsistencia = "SalidaTemprana";
        }

        var actualizada = await asistenciaRepo.ActualizarAsync(asistencia);
        return ApiResponse<AsistenciaResponse>.Ok(MapToResponse(actualizada),
            $"Salida registrada. Horas trabajadas: {horasTrabajadas:F2}.");
    }

    public async Task<ApiResponse<ResumenAsistenciaResponse>> ObtenerResumenAsync(
        int empleadoId, DateOnly fechaInicio, DateOnly fechaFin)
    {
        var empleado = await empleadoRepo.ObtenerPorIdAsync(empleadoId);
        if (empleado is null)
            return ApiResponse<ResumenAsistenciaResponse>.Fail($"Empleado con ID {empleadoId} no encontrado.");

        var filtro = new BuscarAsistenciaRequest
        {
            EmpleadoId   = empleadoId,
            FechaInicio  = fechaInicio,
            FechaFin     = fechaFin,
            PageSize     = 1000
        };

        var (items, _) = await asistenciaRepo.BuscarAsync(filtro);
        var lista       = items.ToList();

        var resumen = new ResumenAsistenciaResponse
        {
            EmpleadoId              = empleadoId,
            NombreCompleto          = $"{empleado.Nombres} {empleado.Apellidos}",
            DiasConRegistro         = lista.Count,
            Presentes               = lista.Count(a => a.EstadoAsistencia == "Presente"),
            Tardanzas               = lista.Count(a => a.EstadoAsistencia == "Tardanza"),
            AusenciasJustificadas   = lista.Count(a => a.EstadoAsistencia == "AusenciaJustificada"),
            AusenciasInjustificadas = lista.Count(a => a.EstadoAsistencia == "AusenciaInjustificada"),
            TotalHorasTrabajadas    = lista.Sum(a => a.HorasTrabajadasReal ?? 0),
            TotalHorasExtras        = lista.Sum(a => a.Horario != null && a.HorasTrabajadasReal > a.Horario.HorasProgramadas
                ? a.HorasTrabajadasReal.Value - a.Horario.HorasProgramadas : 0),
            TotalHorasFaltantes     = lista.Sum(a => a.Horario != null && a.HorasTrabajadasReal < a.Horario.HorasProgramadas
                ? a.Horario.HorasProgramadas - (a.HorasTrabajadasReal ?? 0) : 0),
            CumplimientoPromedio    = lista.Any(a => a.Horario != null && a.Horario.HorasProgramadas > 0)
                ? Math.Round(lista.Where(a => a.Horario != null && a.Horario.HorasProgramadas > 0)
                    .Average(a => (a.HorasTrabajadasReal ?? 0) / a.Horario!.HorasProgramadas * 100), 2)
                : 0
        };

        return ApiResponse<ResumenAsistenciaResponse>.Ok(resumen);
    }

    public async Task<ApiResponse<EstadoAsistenciaHoyResponse>> ObtenerEstadoHoyAsync(int empleadoId)
    {
        var empleado = await empleadoRepo.ObtenerPorIdAsync(empleadoId);
        if (empleado is null)
            return ApiResponse<EstadoAsistenciaHoyResponse>.Fail($"Empleado con ID {empleadoId} no encontrado.");

        var hoy = DateOnly.FromDateTime(DateTime.Now);
        var horario = await horarioRepo.ObtenerPorEmpleadoYFechaAsync(empleadoId, hoy);
        var activa = await asistenciaRepo.ObtenerEntradaActivaAsync(empleadoId);
        var tieneHoy = activa is not null || await asistenciaRepo.TieneEntradaHoyAsync(empleadoId);

        Asistencia? ultima = activa;
        if (ultima is null && tieneHoy)
        {
            var (items, _) = await asistenciaRepo.BuscarAsync(new BuscarAsistenciaRequest
            {
                EmpleadoId = empleadoId,
                FechaInicio = hoy,
                FechaFin = hoy,
                PageSize = 20
            });
            ultima = items.FirstOrDefault();
        }

        return ApiResponse<EstadoAsistenciaHoyResponse>.Ok(new EstadoAsistenciaHoyResponse
        {
            EmpleadoId = empleado.EmpleadoId,
            NombreCompleto = $"{empleado.Nombres} {empleado.Apellidos}",
            Area = empleado.Area?.Nombre ?? string.Empty,
            Cargo = empleado.Cargo?.Nombre ?? string.Empty,
            TieneHorarioHoy = horario is not null,
            TipoTurno = horario?.TipoTurno?.Nombre,
            HoraInicioProgramada = horario?.HoraInicioProgramada,
            HoraFinProgramada = horario?.HoraFinProgramada,
            TieneEntradaActiva = activa is not null,
            YaRegistroHoy = tieneHoy,
            FechaHoraEntrada = ultima?.FechaHoraEntrada,
            EstadoAsistencia = ultima?.EstadoAsistencia,
            HorasTrabajadasReal = ultima?.HorasTrabajadasReal
        });
    }

    private static AsistenciaResponse MapToResponse(Asistencia a) => new()
    {
        AsistenciaId         = a.AsistenciaId,
        EmpleadoId           = a.EmpleadoId,
        NombreCompleto       = $"{a.Empleado.Nombres} {a.Empleado.Apellidos}",
        Area                 = a.Empleado.Area?.Nombre ?? string.Empty,
        Cargo                = a.Empleado.Cargo?.Nombre ?? string.Empty,
        Fecha                = DateOnly.FromDateTime(a.FechaHoraEntrada),
        FechaHoraEntrada     = a.FechaHoraEntrada,
        FechaHoraSalida      = a.FechaHoraSalida,
        HorasTrabajadasReal  = a.HorasTrabajadasReal,
        MinutosRetraso       = a.MinutosRetraso,
        EstadoAsistencia     = a.EstadoAsistencia,
        HorasProgramadas     = a.Horario?.HorasProgramadas,
        HorasExtras          = a.Horario != null && a.HorasTrabajadasReal > a.Horario.HorasProgramadas
            ? a.HorasTrabajadasReal - a.Horario.HorasProgramadas : 0,
        HorasFaltantes       = a.Horario != null && a.HorasTrabajadasReal < a.Horario.HorasProgramadas
            ? a.Horario.HorasProgramadas - (a.HorasTrabajadasReal ?? 0) : 0,
        PorcentajeCumplimiento = a.Horario != null && a.Horario.HorasProgramadas > 0
            ? Math.Round((a.HorasTrabajadasReal ?? 0) / a.Horario.HorasProgramadas * 100, 2) : 0,
        Observaciones        = a.Observaciones
    };
}