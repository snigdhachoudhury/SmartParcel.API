// src/SmartParcel.API/Data/AppDbContext.cs

using Microsoft.EntityFrameworkCore;
using SmartParcel.API.Models;

namespace SmartParcel.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Parcel> Parcels { get; set; }
        public DbSet<Handover> Handovers { get; set; }
        public DbSet<TamperAlert> TamperAlerts { get; set; }
        public DbSet<ParcelHistory> ParcelHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Parcel>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Explicitly tell EF Core that Parcel.Id (Guid) is NOT database-generated
                //entity.Property(e => e.Id)
                //.ValueGeneratedNever(); // <--- ADD THIS LINE FOR Parcel.Id

                // Required properties
                entity.Property(e => e.TrackingId).IsRequired();
                entity.Property(e => e.SenderEmail).IsRequired();
                entity.Property(e => e.RecipientEmail).IsRequired();
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Weight).IsRequired();
                entity.Property(e => e.PickupLocation).IsRequired();
                entity.Property(e => e.DeliveryLocation).IsRequired();

                // Expected dates (required)
                entity.Property(e => e.ExpectedPickupDate).IsRequired();
                entity.Property(e => e.ExpectedDeliveryDate).IsRequired();

                // Actual dates (nullable)
                entity.Property(e => e.ActualPickupDate).IsRequired(false);
                entity.Property(e => e.ActualDeliveryDate).IsRequired(false);

                // Status and timestamps
                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasDefaultValue("Created");

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpdatedAt)
                    .IsRequired(false);
                // Configure OTP fields
                entity.Property(e => e.DeliveryOTP)
                    .IsRequired(false);

                entity.Property(e => e.OTPGeneratedAt)
                    .IsRequired(false);

                entity.Property(e => e.IsOTPVerified)
                    .HasDefaultValue(false);

                // Optional: Ensure TrackingId is unique and indexed
                entity.HasIndex(e => e.TrackingId).IsUnique();
            });

            modelBuilder.Entity<ParcelHistory>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Required properties with proper configuration
                entity.Property(e => e.TrackingId).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.Timestamp)
                      .IsRequired()
                      .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'"); // Important for PostgreSQL

                // Optional properties 
                entity.Property(e => e.Location).IsRequired(false);
                entity.Property(e => e.Notes).IsRequired(false);
                entity.Property(e => e.HandledBy).IsRequired(false);

                // Relationship
                entity.HasOne(ph => ph.Parcel)
                      .WithMany()
                      .HasForeignKey(ph => ph.TrackingId)
                      .HasPrincipalKey(p => p.TrackingId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Indexes for better performance
                entity.HasIndex(ph => new { ph.TrackingId, ph.Timestamp });
            });
        }
    }
}
