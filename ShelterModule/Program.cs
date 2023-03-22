using Database;
using Microsoft.EntityFrameworkCore;
using ShelterModule.Services.Implementations;
using ShelterModule.Services.Interfaces;

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

void ConfigureOptions(IServiceCollection services) { }

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddDbContext<PetShareDbContext>(
        options =>
            options.UseSqlServer(configuration
            .GetConnectionString(PetShareDbContext
            .DbConnectionStringName)));

    services.AddScoped<IShelterQuery,ShelterQuery>();
    services.AddScoped<IShelterCommand,ShelterCommand>();
}

void ApplyMigrations(IServiceProvider services)
{
    using var scope = services.CreateScope();
    using var context = scope.ServiceProvider.GetRequiredService<PetShareDbContext>();
    context.Database.Migrate();
}
