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

        ConfigureOptions(builder.Services);
        ConfigureServices(builder.Services, builder.Configuration);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

       
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

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
        services.AddDbContext<PetShareDbContext>(options =>
                                                     options.UseSqlServer(configuration.
                                                                              GetConnectionString(PetShareDbContext.
                                                                                                      DbConnectionStringName)));

        services.AddScoped<IShelterQuery, ShelterQuery>();
        services.AddScoped<IShelterCommand, ShelterCommand>();
        services.AddScoped<IPetQuery, PetQuery>();
        services.AddScoped<IPetCommand, PetCommand>();
        services.AddScoped<IAnnouncementQuery, AnnoucementQuery>();
        services.AddScoped<IAnnouncementCommand, AnnoucementCommand>();
    }

    private static void ConfigureOptions(IServiceCollection services) { }
}
