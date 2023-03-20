using Database;
using Flurl.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ShelterModule;

namespace ShelterModuleTests;

public class IntegrationTestSetup : WebApplicationFactory<Program>
{
    public FlurlClient CreateFlurlClient()
    {
        return new FlurlClient(CreateClient());
    }

    private static string CreateConnectionString()
    {
        return $"Server=localhost;DataBase=PetShare-test-{Guid.NewGuid()};TrustServerCertificate=true";
    }

    public static TestDbConnectionString CreateTestDatabase()
    {
        return new TestDbConnectionString(CreateConnectionString());
    }

    public static PetShareDbContext CreateDbContext(TestDbConnectionString connectionString)
    {
        var options = new DbContextOptionsBuilder<PetShareDbContext>().UseSqlServer(connectionString.ConnectionString).
                                                                       Options;
        return new PetShareDbContext(options);
    }
    
    protected virtual void ConfigureServices(IServiceCollection services)
    {
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configuration =>
                {
                    configuration.AddJsonFile("appsettings.json").
                                  AddJsonFile("appsettings.Development.json").
                                  AddInMemoryCollection(new Dictionary<string, string?>
                                  {
                                      [$"ConnectionStrings:{PetShareDbContext.DbConnectionStringName}"] =
                                          CreateConnectionString()
                                  });
                }).
                ConfigureTestServices(services =>
                {
                    services.RemoveAll<IHostedService>();
                    ConfigureServices(services);
                });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Database.EnsureCreated();
        return host;
    }

    public override async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        using var scope = Services.CreateScope();
        await using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        await context.Database.EnsureDeletedAsync();
        await base.DisposeAsync();
    }
}

public sealed class TestDbConnectionString : IDisposable
{
    public string ConnectionString { get; }

    public TestDbConnectionString(string connectionString)
    {
        ConnectionString = connectionString;
        var options = new DbContextOptionsBuilder<PetShareDbContext>().UseSqlServer(connectionString).Options;
        using var context = new PetShareDbContext(options);
        context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        var options = new DbContextOptionsBuilder<PetShareDbContext>().UseSqlServer(ConnectionString).Options;
        using var context = new PetShareDbContext(options);
        context.Database.EnsureDeleted();
    }
}
