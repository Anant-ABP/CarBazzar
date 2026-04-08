using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarBazzar.Models.Entity;

public partial class CarBazaarContext : IdentityDbContext<ApplicationUser>
{
    public CarBazaarContext()
    {
    }

    public CarBazaarContext(DbContextOptions<CarBazaarContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Car> Cars { get; set; }
    public virtual DbSet<CarImage> CarImages { get; set; }
    public virtual DbSet<Message> Messages { get; set; }
    public virtual DbSet<Wishlist> Wishlists { get; set; }
    public virtual DbSet<RecentlyViewed> RecentlyViewedCars { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Car>(entity =>
        {
            entity.HasIndex(e => e.SellerId, "IX_Cars_SellerId");

            entity.Property(e => e.Brand).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.FuelType).HasMaxLength(20);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.Model).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.Condition).HasMaxLength(20).HasDefaultValue("Old");
            entity.Property(e => e.IsHidden).HasDefaultValue(false);
            entity.Property(e => e.Transmission).HasMaxLength(20);

            entity.HasOne(d => d.Seller).WithMany(p => p.Cars)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CarImage>(entity =>
        {
            entity.HasIndex(e => e.CarId, "IX_CarImages_CarId");

            entity.Property(e => e.ImageData).HasColumnType("varbinary(max)");
            entity.Property(e => e.ContentType).HasMaxLength(100);

            entity.HasOne(d => d.Car).WithMany(p => p.CarImages).HasForeignKey(d => d.CarId);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasIndex(e => e.CarId, "IX_Messages_CarId");
            entity.HasIndex(e => e.ReceiverId, "IX_Messages_ReceiverId");
            entity.HasIndex(e => e.SenderId, "IX_Messages_SenderId");

            entity.Property(e => e.MessageText).HasMaxLength(1000);

            entity.HasOne(d => d.Car).WithMany(p => p.Messages).HasForeignKey(d => d.CarId);
            entity.HasOne(d => d.Receiver).WithMany(p => p.MessageReceivers).HasForeignKey(d => d.ReceiverId);
            entity.HasOne(d => d.Sender).WithMany(p => p.MessageSenders).HasForeignKey(d => d.SenderId);
        });

        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.HasIndex(e => e.CarId, "IX_Wishlists_CarId");
            entity.HasIndex(e => e.UserId, "IX_Wishlists_UserId");

            entity.HasOne(d => d.Car).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.CarId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
