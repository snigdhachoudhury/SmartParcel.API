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
    }
}

