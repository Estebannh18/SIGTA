using Moq;
using WorkForceManagerAPI.Models.Common;
using WorkForceManagerAPI.Models.DTOs.Request;
using WorkForceManagerAPI.Models.Entities;
using WorkForceManagerAPI.Repositories.Interfaces;
using WorkForceManagerAPI.Services;
using Xunit;

namespace WorkForceManagerAPI.Tests;

public class AsistenciaServiceTests
{
    [Fact]
    public async Task RegistrarEntrada_CalculaTardanzaDespuesDeTolerancia()
    {
        var empleado = new Empleado
        {
            EmpleadoId = 7,
            Nombres = "Ana",
            Apellidos = "Prueba",
            Activo = true,
            Area = new Area { Nombre = "Operaciones" },
            Cargo = new Cargo { Nombre = "Analista" }
        };
        var horario = new Horario
        {
            HorarioId = 11,
            EmpleadoId = empleado.EmpleadoId,
            HoraInicioProgramada = new TimeOnly(8, 0),
            HoraFinProgramada = new TimeOnly(17, 0),
            HorasProgramadas = 8,
            TipoTurno = new TipoTurno { Nombre = "Turno mañana" },
            Empleado = empleado
        };
        var asistenciaRepo = new Mock<IAsistenciaRepository>();
        var empleadoRepo = new Mock<IEmpleadoRepository>();
        var horarioRepo = new Mock<IHorarioRepository>();
        empleadoRepo.Setup(x => x.ObtenerPorIdAsync(7)).ReturnsAsync(empleado);
        asistenciaRepo.Setup(x => x.TieneEntradaHoyAsync(7)).ReturnsAsync(false);
        horarioRepo.Setup(x => x.ObtenerPorEmpleadoYFechaAsync(7, It.IsAny<DateOnly>())).ReturnsAsync(horario);
        asistenciaRepo.Setup(x => x.CrearAsync(It.IsAny<Asistencia>()))
            .ReturnsAsync((Asistencia a) =>
            {
                a.AsistenciaId = 1;
                a.Empleado = empleado;
                a.Horario = horario;
                return a;
            });

        var service = new AsistenciaService(asistenciaRepo.Object, empleadoRepo.Object, horarioRepo.Object);
        var entrada = DateTime.Today.AddHours(8).AddMinutes(20);

        var result = await service.RegistrarEntradaAsync(new RegistrarEntradaRequest
        {
            EmpleadoId = 7,
            FechaHoraEntrada = entrada
        });

        Assert.True(result.Success);
        Assert.Equal("Tardanza", result.Data!.EstadoAsistencia);
        Assert.Equal(20, result.Data.MinutosRetraso);
        asistenciaRepo.Verify(x => x.CrearAsync(It.Is<Asistencia>(a =>
            a.EmpleadoId == 7 && a.HorarioId == 11)), Times.Once);
    }

    [Fact]
    public async Task RegistrarEntrada_RechazaEntradaDuplicada()
    {
        var empleadoRepo = new Mock<IEmpleadoRepository>();
        var asistenciaRepo = new Mock<IAsistenciaRepository>();
        var horarioRepo = new Mock<IHorarioRepository>();
        empleadoRepo.Setup(x => x.ObtenerPorIdAsync(7)).ReturnsAsync(new Empleado { EmpleadoId = 7, Activo = true });
        asistenciaRepo.Setup(x => x.TieneEntradaHoyAsync(7)).ReturnsAsync(true);

        var service = new AsistenciaService(asistenciaRepo.Object, empleadoRepo.Object, horarioRepo.Object);
        var result = await service.RegistrarEntradaAsync(new RegistrarEntradaRequest { EmpleadoId = 7 });

        Assert.False(result.Success);
        Assert.Contains("ya tiene una entrada", result.Message);
        asistenciaRepo.Verify(x => x.CrearAsync(It.IsAny<Asistencia>()), Times.Never);
    }
}
