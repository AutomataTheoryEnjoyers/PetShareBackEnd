using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using Azure.Identity;
using Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ShelterModule.Configuration;
using ShelterModule.Services;
using ShelterModule.Services.Implementations.Adopters;
using ShelterModule.Services.Implementations.Announcements;
using ShelterModule.Services.Implementations.Applications;
using ShelterModule.Services.Implementations.Pets;
using ShelterModule.Services.Implementations.Shelters;
using ShelterModule.Services.Interfaces.Adopters;
using ShelterModule.Services.Interfaces.Announcements;
using ShelterModule.Services.Interfaces.Applications;
using ShelterModule.Services.Interfaces.Pets;
using ShelterModule.Services.Interfaces.Shelters;

namespace ShelterModule;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureOptions(builder);
        ConfigureJwt(builder.Services, builder.Configuration);
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

        if (!builder.Environment.IsEnvironment("Test"))
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
        {
            options.UseSqlServer(configuration.GetConnectionString(PetShareDbContext.DbConnectionStringName)
                                 ?? throw new
                                     InvalidOperationException("No connection string found. Check if there is a corresponding secret in AzureKeyVault"),
                                 sqlOptions => sqlOptions.EnableRetryOnFailure(5));
        });

        services.AddScoped<TokenValidator>();
        services.AddScoped<IShelterQuery, ShelterQuery>();
        services.AddScoped<IShelterCommand, ShelterCommand>();
        services.AddScoped<IPetQuery, PetQuery>();
        services.AddScoped<IPetCommand, PetCommand>();
        services.AddScoped<IAnnouncementQuery, AnnouncementQuery>();
        services.AddScoped<IAnnouncementCommand, AnnouncementCommand>();
        services.AddScoped<IAdopterCommand, AdopterCommand>();
        services.AddScoped<IAdopterQuery, AdopterQuery>();
        services.AddScoped<IApplicationCommand, ApplicationCommand>();
        services.AddScoped<IApplicationQuery, ApplicationQuery>();
    }

    private static void ConfigureOptions(WebApplicationBuilder builder)
    {
        if (builder.Environment.IsProduction())
        {
            var keyVaultUrl = new Uri(builder.Configuration.GetValue<string>("KeyVaultURL")
                                      ?? throw new InvalidOperationException("No azureKeyVault URL found in config."));
            var azureCredential = new DefaultAzureCredential();
            builder.Configuration.AddAzureKeyVault(keyVaultUrl, azureCredential);
        }

        builder.Services.Configure<JwtConfiguration>(builder.Configuration.GetSection(JwtConfiguration.SectionName));
    }

    private static void ConfigureJwt(IServiceCollection services, IConfiguration config)
    {
        var jwtSettings = config.GetSection(JwtConfiguration.SectionName).Get<JwtConfiguration>()
                          ?? throw new ValidationException("JWT options could not be bound to config object");
        Validator.ValidateObject(jwtSettings, new ValidationContext(jwtSettings));

        services.AddAuthentication(options =>
                 {
                     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                 }).
                 AddJwtBearer(options =>
                 {
                     options.RequireHttpsMetadata = false;

                     options.Events = new JwtBearerEvents
                     {
                         OnMessageReceived = context =>
                         {
                             var settings = context.HttpContext.RequestServices.
                                                    GetRequiredService<IOptions<JwtConfiguration>>().
                                                    Value;

                             context.Options.TokenValidationParameters.ValidateIssuer = true;
                             context.Options.TokenValidationParameters.ValidateAudience = true;
                             context.Options.TokenValidationParameters.ValidateIssuerSigningKey = false;
                             context.Options.TokenValidationParameters.ValidIssuer = settings.ValidIssuer;
                             context.Options.TokenValidationParameters.ValidAudience = settings.ValidAudience;
                             context.Options.TokenValidationParameters.SignatureValidator = (token, _) =>
                                 new JwtSecurityTokenHandler().ReadJwtToken(token);
                             return Task.CompletedTask;
                         }
                     };
                 });
    }
}
