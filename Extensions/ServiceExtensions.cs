using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WorkForceManagerAPI.Data;
using WorkForceManagerAPI.Helpers;
using WorkForceManagerAPI.Repositories;
using WorkForceManagerAPI.Repositories.Interfaces;
using WorkForceManagerAPI.Services;
using WorkForceManagerAPI.Services.Interfaces;

namespace WorkForceManagerAPI.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));
        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
    {
        var key = Encoding.UTF8.GetBytes(config["JwtSettings:SecretKey"]!);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = config["JwtSettings:Issuer"],
                    ValidAudience            = config["JwtSettings:Audience"],
                    IssuerSigningKey         = new SymmetricSecurityKey(key)
                };
            });

        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IHorarioRepository, HorarioRepository>();
        services.AddScoped<IAsistenciaRepository, AsistenciaRepository>();
        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IEmpleadoService, EmpleadoService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IHorarioService, HorarioService>();
        services.AddScoped<IAsistenciaService, AsistenciaService>();
        services.AddScoped<IReporteService, ReporteService>();
        services.AddScoped<IReporteExportService, ReporteExportService>();
        services.AddScoped<JwtHelper>();
        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title       = "SIGTA API",
                Version     = "v1",
                Description = "API para gestión de turnos y control de horas trabajadas"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name         = "Authorization",
                Type         = SecuritySchemeType.ApiKey,
                Scheme       = "Bearer",
                BearerFormat = "JWT",
                In           = ParameterLocation.Header,
                Description  = "Ingresa: Bearer {token}"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                            { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
