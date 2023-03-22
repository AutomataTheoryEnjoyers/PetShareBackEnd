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

/// <summary>
///     This class provides utilities needed in integration (and sometimes unit) tests. If some services need to be
///     replaced with mock implementation or removed before test, this class should be inherited, and necessary changes
///     should be made in <see cref="ConfigureServices" /> override.
/// </summary>
public class IntegrationTestSetup : WebApplicationFactory<Program>
{
    public FlurlClient CreateFlurlClient()
    {
        return new FlurlClient(CreateClient());
    }

    private static string CreateConnectionString()
    {
        var template = Environment.GetEnvironmentVariable("ConnectionStringTemplate")
                       ?? "Server=localhost;DataBase={0};TrustServerCertificate=true";
        return string.Format(template, $"PetShare-tests-{Guid.NewGuid()}");
    }

    /// <summary>
    ///     Construct a unique connection string to test database, creates it, and returns a wrapper that provides
    ///     access to it. This wrapper should be disposed after the test is executed, which deletes the database.
    /// </summary>
    /// <remarks>
    ///     To create a <see cref="PetShareDbContext" /> from <see cref="TestDbConnectionString" />, call
    ///     <see cref="CreateDbContext" />
    /// </remarks>
    /// <returns> <see cref="TestDbConnectionString" /> used to access a unique test database </returns>
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

    protected virtual void ConfigureServices(IServiceCollection services) { }

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
    public TestDbConnectionString(string connectionString)
    {
        ConnectionString = connectionString;
        var options = new DbContextOptionsBuilder<PetShareDbContext>().UseSqlServer(connectionString).Options;
        using var context = new PetShareDbContext(options);
        context.Database.EnsureCreated();
    }

    public string ConnectionString { get; }

    public void Dispose()
    {
        var options = new DbContextOptionsBuilder<PetShareDbContext>().UseSqlServer(ConnectionString).Options;
        using var context = new PetShareDbContext(options);
        context.Database.EnsureDeleted();
    }
}
