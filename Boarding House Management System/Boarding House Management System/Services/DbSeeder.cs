using Microsoft.EntityFrameworkCore;
using Boarding_House_Management_System.Data;
using Boarding_House_Management_System.Model;

namespace Boarding_House_Management_System.Services
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Check if data already exists
            if (await context.Dormitories.AnyAsync())
            {
                return; // Already seeded
            }

            // 1. Seed Dormitories
            var dorm1 = new Dormitory
            {
                Name = "Pinecrest Residence Hall",
                Address = "124 University Avenue, Metro District",
                ContactNumber = "0917-123-4567",
                Description = "A modern 3-story boarding house with Wi-Fi, 24/7 security, study lounges, and CCTV.",
                CreatedAt = DateTime.UtcNow.AddMonths(-6)
            };

            var dorm2 = new Dormitory
            {
                Name = "Green Meadows Boarding House",
                Address = "88 Acacia Street, College Park",
                ContactNumber = "0928-987-6543",
                Description = "Affordable and peaceful accommodation for students and young professionals.",
                CreatedAt = DateTime.UtcNow.AddMonths(-4)
            };

            context.Dormitories.AddRange(dorm1, dorm2);
            await context.SaveChangesAsync();

            // 2. Seed Users
            var admin = new User
            {
                Username = "admin",
                PasswordHash = PasswordHasher.HashPassword("admin123"),
                FullName = "Maria Santos (Landlord/Admin)",
                Email = "admin@boardinghouse.com",
                PhoneNumber = "0917-111-2222",
                Role = "Admin",
                CreatedAt = DateTime.UtcNow.AddMonths(-6)
            };

            var tenant1 = new User
            {
                Username = "john_doe",
                PasswordHash = PasswordHasher.HashPassword("tenant123"),
                FullName = "John Doe",
                Email = "john.doe@email.com",
                PhoneNumber = "0918-333-4444",
                Role = "Tenant",
                CreatedAt = DateTime.UtcNow.AddMonths(-5)
            };

            var tenant2 = new User
            {
                Username = "anna_reyes",
                PasswordHash = PasswordHasher.HashPassword("tenant123"),
                FullName = "Anna Reyes",
                Email = "anna.reyes@email.com",
                PhoneNumber = "0922-555-6666",
                Role = "Tenant",
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            };

            var tenant3 = new User
            {
                Username = "carlos_tan",
                PasswordHash = PasswordHasher.HashPassword("tenant123"),
                FullName = "Carlos Tan",
                Email = "carlos.tan@email.com",
                PhoneNumber = "0939-777-8888",
                Role = "Tenant",
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            };

            context.Users.AddRange(admin, tenant1, tenant2, tenant3);
            await context.SaveChangesAsync();

            // 3. Seed Rooms
            // In Pinecrest Residence Hall
            var room101 = new Room
            {
                DormitoryId = dorm1.DormitoryId,
                RoomNumber = "Room 101",
                RoomType = "Single Private",
                Capacity = 1,
                CurrentOccupants = 1,
                MonthlyRate = 4500.00m,
                Status = "Occupied"
            };

            var room102 = new Room
            {
                DormitoryId = dorm1.DormitoryId,
                RoomNumber = "Room 102",
                RoomType = "Double Sharing",
                Capacity = 2,
                CurrentOccupants = 1,
                MonthlyRate = 3200.00m,
                Status = "Available" // 1 vacant slot
            };

            var room103 = new Room
            {
                DormitoryId = dorm1.DormitoryId,
                RoomNumber = "Room 103",
                RoomType = "Deluxe Studio",
                Capacity = 1,
                CurrentOccupants = 0,
                MonthlyRate = 5500.00m,
                Status = "Available" // fully vacant
            };

            var room104 = new Room
            {
                DormitoryId = dorm1.DormitoryId,
                RoomNumber = "Room 104",
                RoomType = "Quad Bedspace",
                Capacity = 4,
                CurrentOccupants = 0,
                MonthlyRate = 2200.00m,
                Status = "Maintenance"
            };

            // In Green Meadows Boarding House
            var room201 = new Room
            {
                DormitoryId = dorm2.DormitoryId,
                RoomNumber = "Room 201",
                RoomType = "Single Standard",
                Capacity = 1,
                CurrentOccupants = 1,
                MonthlyRate = 3800.00m,
                Status = "Occupied"
            };

            var room202 = new Room
            {
                DormitoryId = dorm2.DormitoryId,
                RoomNumber = "Room 202",
                RoomType = "Double Sharing",
                Capacity = 2,
                CurrentOccupants = 0,
                MonthlyRate = 2800.00m,
                Status = "Available"
            };

            var room203 = new Room
            {
                DormitoryId = dorm2.DormitoryId,
                RoomNumber = "Room 203",
                RoomType = "Bedspace Dormitory",
                Capacity = 4,
                CurrentOccupants = 2,
                MonthlyRate = 1900.00m,
                Status = "Available" // 2 vacant slots
            };

            context.Rooms.AddRange(room101, room102, room103, room104, room201, room202, room203);
            await context.SaveChangesAsync();

            // 4. Seed Leases
            var lease1 = new Lease
            {
                UserId = tenant1.UserId,
                RoomId = room101.RoomId,
                StartDate = DateTime.UtcNow.AddMonths(-4),
                EndDate = DateTime.UtcNow.AddMonths(2),
                Status = "Active"
            };

            var lease2 = new Lease
            {
                UserId = tenant2.UserId,
                RoomId = room102.RoomId,
                StartDate = DateTime.UtcNow.AddMonths(-2),
                EndDate = DateTime.UtcNow.AddMonths(4),
                Status = "Active"
            };

            var lease3 = new Lease
            {
                UserId = tenant3.UserId,
                RoomId = room201.RoomId,
                StartDate = DateTime.UtcNow.AddMonths(-1),
                EndDate = DateTime.UtcNow.AddMonths(5),
                Status = "Active"
            };

            context.Leases.AddRange(lease1, lease2, lease3);
            await context.SaveChangesAsync();

            // 5. Seed Payments ("Paying Rent")
            var pay1 = new Payment
            {
                LeaseId = lease1.LeaseId,
                AmountPaid = 4500.00m,
                PaymentDate = DateTime.UtcNow.AddMonths(-3),
                PaymentMethod = "Cash",
                ReferenceNumber = "PAY-20260715-001",
                Status = "Completed"
            };

            var pay2 = new Payment
            {
                LeaseId = lease1.LeaseId,
                AmountPaid = 4500.00m,
                PaymentDate = DateTime.UtcNow.AddMonths(-2),
                PaymentMethod = "GCash",
                ReferenceNumber = "GCASH-9876543210",
                Status = "Completed"
            };

            var pay3 = new Payment
            {
                LeaseId = lease1.LeaseId,
                AmountPaid = 4500.00m,
                PaymentDate = DateTime.UtcNow.AddMonths(-1),
                PaymentMethod = "Bank Transfer",
                ReferenceNumber = "BDO-REF-445566",
                Status = "Completed"
            };

            var pay4 = new Payment
            {
                LeaseId = lease2.LeaseId,
                AmountPaid = 3200.00m,
                PaymentDate = DateTime.UtcNow.AddMonths(-2),
                PaymentMethod = "GCash",
                ReferenceNumber = "GCASH-1122334455",
                Status = "Completed"
            };

            var pay5 = new Payment
            {
                LeaseId = lease2.LeaseId,
                AmountPaid = 3200.00m,
                PaymentDate = DateTime.UtcNow.AddMonths(-1),
                PaymentMethod = "Cash",
                ReferenceNumber = "PAY-20260901-002",
                Status = "Completed"
            };

            var pay6 = new Payment
            {
                LeaseId = lease3.LeaseId,
                AmountPaid = 3800.00m,
                PaymentDate = DateTime.UtcNow.AddDays(-10),
                PaymentMethod = "Maya",
                ReferenceNumber = "MAYA-7788990011",
                Status = "Completed"
            };

            context.Payments.AddRange(pay1, pay2, pay3, pay4, pay5, pay6);
            await context.SaveChangesAsync();

            // 6. Seed Reservations
            var res1 = new Reservation
            {
                UserId = tenant3.UserId,
                RoomId = room103.RoomId,
                ReservationDate = DateTime.UtcNow.AddDays(-2),
                Status = "Pending"
            };

            context.Reservations.Add(res1);
            await context.SaveChangesAsync();

            // 7. Seed Maintenance Requests
            var mr1 = new MaintenanceRequest
            {
                RoomId = room101.RoomId,
                UserId = tenant1.UserId,
                IssueDescription = "Bathroom sink faucet is leaking slightly.",
                DateReported = DateTime.UtcNow.AddDays(-5),
                Status = "In Progress",
                ResolvedAt = null
            };

            var mr2 = new MaintenanceRequest
            {
                RoomId = room104.RoomId,
                UserId = admin.UserId,
                IssueDescription = "Repainting walls and replacing ceiling fan fixture.",
                DateReported = DateTime.UtcNow.AddDays(-12),
                Status = "Pending",
                ResolvedAt = null
            };

            context.MaintenanceRequests.AddRange(mr1, mr2);
            await context.SaveChangesAsync();
        }
    }
}
