using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Azure.Identity;
using Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PetShare.Configuration;
using PetShare.Services;
using PetShare.Services.Implementations;
using PetShare.Services.Implementations.Adopters;
using PetShare.Services.Implementations.Announcements;
using PetShare.Services.Implementations.Applications;
using PetShare.Services.Implementations.Pets;
using PetShare.Services.Implementations.Shelters;
using PetShare.Services.Interfaces;
using PetShare.Services.Interfaces.Adopters;
using PetShare.Services.Interfaces.Announcements;
using PetShare.Services.Interfaces.Applications;
using PetShare.Services.Interfaces.Pets;
using PetShare.Services.Interfaces.Shelters;

namespace PetShare;

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
        services.AddSingleton<IImageStorage, ImgurImageStorage>();
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
        builder.Services.Configure<ImgurConfiguration>(builder.Configuration.
                                                               GetSection(ImgurConfiguration.SectionName));
    }

    private static void ConfigureJwt(IServiceCollection services, IConfiguration config)
    {
        var jwtSettings = config.GetSection(JwtConfiguration.SectionName).Get<JwtConfiguration>()
                          ?? throw new ValidationException("JWT options could not be bound to config object");
        Validator.ValidateObject(jwtSettings, new ValidationContext(jwtSettings));

        services.AddTransient<RsaSecurityKey>(provider =>
        {
            var settings = provider.GetRequiredService<IOptions<JwtConfiguration>>().Value;
            var rsa = RSA.Create();
            if (settings.KeyIsPem)
                rsa.ImportFromPem(settings.SigningKey);
            else
                rsa.ImportRSAPublicKey(Convert.FromBase64String(settings.SigningKey), out _);
            return new RsaSecurityKey(rsa);
        });
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
                             var rsaKey = context.HttpContext.RequestServices.GetRequiredService<RsaSecurityKey>();

                             context.Options.TokenValidationParameters.ValidateIssuer = true;
                             context.Options.TokenValidationParameters.ValidateAudience = true;
                             context.Options.TokenValidationParameters.ValidateIssuerSigningKey = true;
                             context.Options.TokenValidationParameters.ValidateLifetime = true;
                             context.Options.TokenValidationParameters.ValidIssuer = settings.ValidIssuer;
                             context.Options.TokenValidationParameters.ValidAudience = settings.ValidAudience;
                             context.Options.TokenValidationParameters.IssuerSigningKey = rsaKey;
                             return Task.CompletedTask;
                         }
                     };
                 });
    }
}
