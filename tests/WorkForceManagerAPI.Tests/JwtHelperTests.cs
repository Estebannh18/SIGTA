using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using WorkForceManagerAPI.Helpers;
using Xunit;

namespace WorkForceManagerAPI.Tests;

public class JwtHelperTests
{
    [Fact]
    public void ObtenerUsuarioId_LeeNameIdentifierMapeadoPorJwt()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "42")
        ]));

        Assert.Equal(42, JwtHelper.ObtenerUsuarioId(user));
    }

    [Fact]
    public void ObtenerUsuarioId_UsaSubComoFallback()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim("sub", "18")
        ]));

        Assert.Equal(18, JwtHelper.ObtenerUsuarioId(user));
    }

    [Fact]
    public void ObtenerEmpleadoIdYRol_LeeClaimsDelToken()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim("empleadoId", "7"),
            new Claim(ClaimTypes.Role, "Supervisor")
        ]));

        Assert.Equal(7, JwtHelper.ObtenerEmpleadoId(user));
        Assert.Equal("Supervisor", JwtHelper.ObtenerRol(user));
    }
}
