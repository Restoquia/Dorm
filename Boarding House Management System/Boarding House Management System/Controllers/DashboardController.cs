using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Boarding_House_Management_System.Data;
using Boarding_House_Management_System.DTOs;

namespace Boarding_House_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get high-level statistics and dashboard summary for the boarding house system
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
        {
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            var totalDormitories = await _context.Dormitories.CountAsync();
            var rooms = await _context.Rooms.Include(r => r.Dormitory).ToListAsync();

            var totalRooms = rooms.Count;
            var availableRooms = rooms.Count(r => r.Status == "Available" && r.CurrentOccupants < r.Capacity);
            var occupiedRooms = rooms.Count(r => r.Status == "Occupied" || r.CurrentOccupants >= r.Capacity);
            var underMaintenanceRooms = rooms.Count(r => r.Status == "Maintenance");
            var totalCapacity = rooms.Sum(r => r.Capacity);
            var totalOccupants = rooms.Sum(r => r.CurrentOccupants);
            var overallOccupancyRate = totalCapacity > 0 ? Math.Round((double)totalOccupants / totalCapacity * 100, 1) : 0;

            var totalTenants = await _context.Users.CountAsync(u => u.Role == "Tenant");
            var activeLeases = await _context.Leases.CountAsync(l => l.Status == "Active");
            var pendingReservations = await _context.Reservations.CountAsync(r => r.Status == "Pending");
            var pendingMaintenance = await _context.MaintenanceRequests.CountAsync(m => m.Status == "Pending" || m.Status == "In Progress");

            var completedPayments = await _context.Payments
                .Include(p => p.Lease)
                    .ThenInclude(l => l!.User)
                .Include(p => p.Lease)
                    .ThenInclude(l => l!.Room)
                        .ThenInclude(r => r!.Dormitory)
                .Where(p => p.Status == "Completed")
                .ToListAsync();

            var totalRevenue = completedPayments.Sum(p => p.AmountPaid);
            var thisMonthRevenue = completedPayments
                .Where(p => p.PaymentDate >= startOfMonth)
                .Sum(p => p.AmountPaid);

            var availableRoomsList = rooms
                .Where(r => r.Status == "Available" && r.CurrentOccupants < r.Capacity)
                .Take(6)
                .Select(r => new RoomResponseDto
                {
                    RoomId = r.RoomId,
                    DormitoryId = r.DormitoryId,
                    DormitoryName = r.Dormitory?.Name ?? "Unassigned",
                    RoomNumber = r.RoomNumber,
                    RoomType = r.RoomType,
                    Capacity = r.Capacity,
                    CurrentOccupants = r.CurrentOccupants,
                    MonthlyRate = r.MonthlyRate,
                    Status = r.Status
                })
                .ToList();

            var recentPayments = completedPayments
                .OrderByDescending(p => p.PaymentDate)
                .Take(5)
                .Select(p => new PaymentResponseDto
                {
                    PaymentId = p.PaymentId,
                    LeaseId = p.LeaseId,
                    TenantName = p.Lease?.User?.FullName ?? "Unknown",
                    RoomNumber = p.Lease?.Room?.RoomNumber ?? "Unknown",
                    DormitoryName = p.Lease?.Room?.Dormitory?.Name ?? "Unassigned",
                    AmountPaid = p.AmountPaid,
                    PaymentDate = p.PaymentDate,
                    PaymentMethod = p.PaymentMethod,
                    ReferenceNumber = p.ReferenceNumber,
                    Status = p.Status
                })
                .ToList();

            var recentMaintenance = await _context.MaintenanceRequests
                .Include(m => m.Room)
                    .ThenInclude(r => r!.Dormitory)
                .Include(m => m.User)
                .OrderByDescending(m => m.DateReported)
                .Take(5)
                .Select(m => new MaintenanceRequestResponseDto
                {
                    RequestId = m.RequestId,
                    RoomId = m.RoomId,
                    RoomNumber = m.Room != null ? m.Room.RoomNumber : "Unknown",
                    DormitoryName = m.Room != null && m.Room.Dormitory != null ? m.Room.Dormitory.Name : "Unassigned",
                    UserId = m.UserId,
                    RequesterName = m.User != null ? m.User.FullName : "Unknown",
                    IssueDescription = m.IssueDescription,
                    DateReported = m.DateReported,
                    Status = m.Status,
                    ResolvedAt = m.ResolvedAt
                })
                .ToListAsync();

            var stats = new DashboardStatsDto
            {
                TotalDormitories = totalDormitories,
                TotalRooms = totalRooms,
                AvailableRooms = availableRooms,
                OccupiedRooms = occupiedRooms,
                UnderMaintenanceRooms = underMaintenanceRooms,
                TotalCapacity = totalCapacity,
                TotalOccupants = totalOccupants,
                OverallOccupancyRate = overallOccupancyRate,
                TotalTenants = totalTenants,
                ActiveLeases = activeLeases,
                PendingReservations = pendingReservations,
                PendingMaintenanceRequests = pendingMaintenance,
                TotalRevenue = totalRevenue,
                ThisMonthRevenue = thisMonthRevenue,
                AvailableRoomsList = availableRoomsList,
                RecentPayments = recentPayments,
                RecentMaintenanceRequests = recentMaintenance
            };

            return Ok(stats);
        }
    }
}
