using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WorkForceManagerAPI.Models.Entities;

namespace WorkForceManagerAPI.Helpers;

public class JwtHelper(IConfiguration config)
{
    public (string Token, DateTime Expiracion) GenerarToken(Usuario usuario)
    {
        var key        = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:SecretKey"]!));
        var creds      = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var horas      = int.Parse(config["JwtSettings:ExpirationHours"]!);
        var expiracion = DateTime.UtcNow.AddHours(horas);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   usuario.UsuarioId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new Claim("empleadoId",                  usuario.EmpleadoId.ToString()),
            new Claim("nombreCompleto",
                $"{usuario.Empleado.Nombres} {usuario.Empleado.Apellidos}"),
            new Claim(ClaimTypes.Role,               usuario.Rol.Nombre)
        };

        var token = new JwtSecurityToken(
            issuer:             config["JwtSettings:Issuer"],
            audience:           config["JwtSettings:Audience"],
            claims:             claims,
            expires:            expiracion,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiracion);
    }

    public static int ObtenerUsuarioId(ClaimsPrincipal user)
    {
        var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return int.TryParse(value, out var usuarioId) ? usuarioId : 0;
    }

    public static int ObtenerEmpleadoId(ClaimsPrincipal user) =>
        int.Parse(user.FindFirst("empleadoId")?.Value ?? "0");

    public static string ObtenerRol(ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
}
