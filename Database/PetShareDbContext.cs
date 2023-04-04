using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database;

public sealed class PetShareDbContext : DbContext
{
    public const string DbConnectionStringName = "PetShareDatabase";

    public PetShareDbContext(DbContextOptions options) : base(options) { }

    public DbSet<ShelterEntity> Shelters => Set<ShelterEntity>();
    public DbSet<PetEntity> Pets => Set<PetEntity>();
    public DbSet<AnnouncementEntity> Announcements => Set<AnnouncementEntity>();
}
