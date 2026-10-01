using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WorkForceManagerAPI.Controllers;
using WorkForceManagerAPI.Models.Common;
using WorkForceManagerAPI.Models.DTOs.Request;
using WorkForceManagerAPI.Models.DTOs.Response;
using WorkForceManagerAPI.Services.Interfaces;
using Xunit;

namespace WorkForceManagerAPI.Tests;

public class AuthorizationTests
{
    [Fact]
    public async Task Empleado_BuscarAsistencia_SoloUsaSuEmpleadoId()
    {
        var service = new Mock<IAsistenciaService>();
        BuscarAsistenciaRequest? recibido = null;
        service.Setup(x => x.BuscarAsync(It.IsAny<BuscarAsistenciaRequest>()))
            .Callback<BuscarAsistenciaRequest>(filtro => recibido = filtro)
            .ReturnsAsync(PagedResponse<AsistenciaResponse>.Create([], 0, 1, 20));

        var controller = new AsistenciaController(service.Object)
        {
            ControllerContext = ContextoUsuario("Empleado", empleadoId: 9)
        };

        await controller.Buscar(new BuscarAsistenciaRequest { EmpleadoId = 1 });

        Assert.NotNull(recibido);
        Assert.Equal(9, recibido!.EmpleadoId);
    }

    [Fact]
    public async Task Empleado_NoPuedeRegistrarEntradaDeOtroEmpleado()
    {
        var service = new Mock<IAsistenciaService>();
        var controller = new AsistenciaController(service.Object)
        {
            ControllerContext = ContextoUsuario("Empleado", empleadoId: 9)
        };

        var result = await controller.RegistrarEntrada(new RegistrarEntradaRequest { EmpleadoId = 1 });

        Assert.IsType<ForbidResult>(result);
        service.Verify(x => x.RegistrarEntradaAsync(It.IsAny<RegistrarEntradaRequest>()), Times.Never);
    }

    [Fact]
    public async Task Empleado_NoPuedeConsultarResumenAjeno()
    {
        var service = new Mock<IAsistenciaService>();
        var controller = new AsistenciaController(service.Object)
        {
            ControllerContext = ContextoUsuario("Empleado", empleadoId: 9)
        };

        var result = await controller.ObtenerResumen(1, DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today));

        Assert.IsType<ForbidResult>(result);
        service.Verify(x => x.ObtenerResumenAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()), Times.Never);
    }

    private static ControllerContext ContextoUsuario(string rol, int empleadoId) => new()
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([
                new Claim(ClaimTypes.Role, rol),
                new Claim("empleadoId", empleadoId.ToString())
            ], "TestAuth"))
        }
    };
}
