using Microsoft.AspNetCore.Authorization;
using WorkForceManagerAPI.Controllers;
using Xunit;

namespace WorkForceManagerAPI.Tests;

public class ControllerPolicyTests
{
    [Theory]
    [InlineData(typeof(DashboardController))]
    [InlineData(typeof(ReportesController))]
    [InlineData(typeof(EmpleadosController))]
    public void EndpointsGlobales_RequierenAdministradorOSupervisor(Type controllerType)
    {
        var authorize = controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .Single();

        Assert.Equal("Administrador,Supervisor", authorize.Roles);
    }
}
