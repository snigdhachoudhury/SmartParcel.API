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
        // public DbSet<ParcelHistory> ParcelHistory { get; set; } // <--- Add this back if you plan to use ParcelHistory

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

                // If you re-introduce ParcelHistory, its configuration would go here.
                // entity.HasMany(p => p.History)
                //       .WithOne(ph => ph.Parcel)
                //       .HasForeignKey(ph => ph.ParcelTrackingId)
                //
                //.HasPrincipalKey(p => p.TrackingId);
            });
        }
            // Optional: You can also explicitly configure User.Id if you want, though 'int' usually works by default
            // modelBuilder.Entity<User>(entity =>
            // {
            //     entity.Property(u => u.Id)
            //           .ValueGeneratedOnAdd(); // For int primary keys, this is usually the default
            // });

            // If Handover and TamperAlert also have Guid primary keys, you'd add similar configurations for them:
            // modelBuilder.Entity<Handover>(entity =>
            // {
            //     entity.Property(e => e.Id).ValueGeneratedNever();
            // });
            // modelBuilder.Entity<TamperAlert>(entity =>
            // {
            //     entity.Property(e => e.Id).ValueGeneratedNever();
            // });
        }
    }
