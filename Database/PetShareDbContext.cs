using Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Database;

public sealed class PetShareDbContext : DbContext
{
    // NOTE: this string has to mach value for AzureKeyVaultSecret ConnectionStrings--{value}
    public const string DbConnectionStringName = "PetShareDatabase";

    public PetShareDbContext(DbContextOptions options) : base(options) { }

    public DbSet<ShelterEntity> Shelters => Set<ShelterEntity>();
    public DbSet<PetEntity> Pets => Set<PetEntity>();
    public DbSet<AnnouncementEntity> Announcements => Set<AnnouncementEntity>();
    public DbSet<AdopterEntity> Adopters => Set<AdopterEntity>();
    public DbSet<AdopterVerificationEntity> Verifications => Set<AdopterVerificationEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnnouncementEntity>().
                     HasOne("Database.Entities.ShelterEntity", "Author").
                     WithMany().
                     HasForeignKey("AuthorId").
                     OnDelete(DeleteBehavior.Restrict).
                     IsRequired();
    }
}
