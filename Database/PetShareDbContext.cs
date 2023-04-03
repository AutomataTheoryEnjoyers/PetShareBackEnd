using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database;

public sealed class PetShareDbContext : DbContext
{
    public const string DbConnectionStringName = "PetShareDatabase";

    public PetShareDbContext(DbContextOptions options) : base(options) { }

    public DbSet<ShelterEntity> Shelters => Set<ShelterEntity>();
    public DbSet<PetEnitiy> Pets => Set<PetEnitiy>();
    public DbSet<AnnouncementEntity> Announcements => Set<AnnouncementEntity>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnnouncementEntity>().HasOne("Database.Entities.PetEnitiy", "Pet")
                        .WithMany()
                        .HasForeignKey("PetId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

        modelBuilder.Entity<AnnouncementEntity>()
            .HasOne("Database.Entities.ShelterEntity", "Author")
                        .WithMany()
                        .HasForeignKey("ShelterId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired(); 
    }
    public DbSet<PetEntity> Pets => Set<PetEntity>();
}
