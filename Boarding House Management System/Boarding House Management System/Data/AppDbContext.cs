using Microsoft.EntityFrameworkCore;
using Boarding_House_Management_System.Model;

namespace Boarding_House_Management_System.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Dormitory> Dormitories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Lease> Leases { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Dormitory -> Rooms (1-to-many)
            modelBuilder.Entity<Room>()
                .HasOne(r => r.Dormitory)
                .WithMany(d => d.Rooms)
                .HasForeignKey(r => r.DormitoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // User -> Reservations (1-to-many)
            modelBuilder.Entity<Reservation>()
                .HasOne(res => res.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(res => res.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Room -> Reservations (1-to-many)
            modelBuilder.Entity<Reservation>()
                .HasOne(res => res.Room)
                .WithMany(r => r.Reservations)
                .HasForeignKey(res => res.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Leases (1-to-many)
            modelBuilder.Entity<Lease>()
                .HasOne(l => l.User)
                .WithMany(u => u.Leases)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Room -> Leases (1-to-many)
            modelBuilder.Entity<Lease>()
                .HasOne(l => l.Room)
                .WithMany(r => r.Leases)
                .HasForeignKey(l => l.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // Lease -> Payments (1-to-many)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Lease)
                .WithMany(l => l.Payments)
                .HasForeignKey(p => p.LeaseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Room -> MaintenanceRequests (1-to-many)
            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(mr => mr.Room)
                .WithMany(r => r.MaintenanceRequests)
                .HasForeignKey(mr => mr.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> MaintenanceRequests (1-to-many)
            modelBuilder.Entity<MaintenanceRequest>()
                .HasOne(mr => mr.User)
                .WithMany(u => u.MaintenanceRequests)
                .HasForeignKey(mr => mr.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
