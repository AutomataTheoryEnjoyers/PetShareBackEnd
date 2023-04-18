using Azure.Identity;
using Database;
using Microsoft.EntityFrameworkCore;
using ShelterModule.Services.Implementations.Announcements;
using ShelterModule.Services.Implementations.Pets;
using ShelterModule.Services.Implementations.Shelters;
using ShelterModule.Services.Interfaces.Announcements;
using ShelterModule.Services.Interfaces.Pets;
using ShelterModule.Services.Interfaces.Shelters;

namespace ShelterModule;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureOptions(builder);
        ConfigureServices(builder.Services, builder.Configuration);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseCors(options => options.AllowAnyMethod().
                                       AllowAnyHeader().
                                       SetIsOriginAllowed(_ => true).
                                       AllowCredentials());

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        ApplyMigrations(app.Services);

        app.Run();
    }

    private static void ApplyMigrations(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
        context.Database.Migrate();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(PetShareDbContext.DbConnectionStringName)
                               ?? throw new
                                   InvalidOperationException("No connection string found. Check if there is a corresponding secret in AzureKeyVault");
        services.AddDbContext<PetShareDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure(5));
        });

        services.AddScoped<IShelterQuery, ShelterQuery>();
        services.AddScoped<IShelterCommand, ShelterCommand>();
        services.AddScoped<IPetQuery, PetQuery>();
        services.AddScoped<IPetCommand, PetCommand>();
        services.AddScoped<IAnnouncementQuery, AnnouncementQuery>();
        services.AddScoped<IAnnouncementCommand, AnnouncementCommand>();
    }

    private static void ConfigureOptions(WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
            return;

        var keyVaultUrl = new Uri(builder.Configuration.GetValue<string>("KeyVaultURL")
                                  ?? throw new InvalidOperationException("No azureKeyVault URL found in config."));
        var azureCredential = new DefaultAzureCredential();
        builder.Configuration.AddAzureKeyVault(keyVaultUrl, azureCredential);
    }
}
