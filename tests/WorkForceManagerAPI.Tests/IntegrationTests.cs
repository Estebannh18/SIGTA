using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace WorkForceManagerAPI.Tests;

public class IntegrationTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient client;

    public IntegrationTests(ApiFactory factory) => client = factory.CreateClient();

    [Theory]
    [InlineData("/api/Dashboard/resumen")]
    [InlineData("/api/Reportes/horas")]
    [InlineData("/api/Empleados")]
    public async Task EndpointsProtegidos_SinToken_Devuelven401(string endpoint)
    {
        var response = await client.GetAsync(endpoint);

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Server=localhost\\SQLEXPRESS;Database=WorkForceManagerDB;Trusted_Connection=True;TrustServerCertificate=True;");
        builder.UseSetting("JwtSettings:SecretKey", "integration-test-secret-key-long-enough-2026");
        builder.UseSetting("JwtSettings:Issuer", "SIGTA.Tests");
        builder.UseSetting("JwtSettings:Audience", "SIGTA.Tests.Client");
        builder.UseSetting("JwtSettings:ExpirationHours", "1");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost\\SQLEXPRESS;Database=WorkForceManagerDB;Trusted_Connection=True;TrustServerCertificate=True;",
                ["JwtSettings:SecretKey"] = "integration-test-secret-key-long-enough-2026",
                ["JwtSettings:Issuer"] = "SIGTA.Tests",
                ["JwtSettings:Audience"] = "SIGTA.Tests.Client",
                ["JwtSettings:ExpirationHours"] = "1"
            });
        });
    }
}
