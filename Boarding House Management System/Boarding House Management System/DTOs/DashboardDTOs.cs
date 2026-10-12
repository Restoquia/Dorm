namespace Boarding_House_Management_System.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalDormitories { get; set; }
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int UnderMaintenanceRooms { get; set; }
        public int TotalCapacity { get; set; }
        public int TotalOccupants { get; set; }
        public double OverallOccupancyRate { get; set; }
        public int TotalTenants { get; set; }
        public int ActiveLeases { get; set; }
        public int PendingReservations { get; set; }
        public int PendingMaintenanceRequests { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public List<RoomResponseDto> AvailableRoomsList { get; set; } = new();
        public List<PaymentResponseDto> RecentPayments { get; set; } = new();
        public List<MaintenanceRequestResponseDto> RecentMaintenanceRequests { get; set; } = new();
    }
}
