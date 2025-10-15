using Microsoft.EntityFrameworkCore;
using WastageUploadService.Models;

namespace WastageUploadService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Wastage> Wastages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Wastage>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.InwardChallanId)
                .IsUnique();

            entity.Property(e => e.InwardChallanId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.PartyName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.VehicleNo)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Date)
                .IsRequired();

            entity.Property(e => e.MouReportJson)
                .IsRequired()
                .HasDefaultValue("[]");

            entity.Property(e => e.ImageUrlsJson)
                .IsRequired()
                .HasDefaultValue("[]");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
