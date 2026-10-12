using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Boarding_House_Management_System.Data;
using Boarding_House_Management_System.DTOs;
using Boarding_House_Management_System.Model;

namespace Boarding_House_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeasesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LeasesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all leases with optional filters
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LeaseResponseDto>>> GetAllLeases(
            [FromQuery] int? userId,
            [FromQuery] int? roomId,
            [FromQuery] string? status)
        {
            var query = _context.Leases
                .Include(l => l.User)
                .Include(l => l.Room)
                    .ThenInclude(r => r!.Dormitory)
                .Include(l => l.Payments)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(l => l.UserId == userId.Value);

            if (roomId.HasValue)
                query = query.Where(l => l.RoomId == roomId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(l => l.Status.ToLower() == status.ToLower());

            var leases = await query
                .OrderByDescending(l => l.StartDate)
                .Select(l => new LeaseResponseDto
                {
                    LeaseId = l.LeaseId,
                    UserId = l.UserId,
                    TenantName = l.User != null ? l.User.FullName : "Unknown",
                    TenantEmail = l.User != null ? l.User.Email : null,
                    TenantPhone = l.User != null ? l.User.PhoneNumber : null,
                    RoomId = l.RoomId,
                    RoomNumber = l.Room != null ? l.Room.RoomNumber : "Unknown",
                    RoomType = l.Room != null ? l.Room.RoomType : null,
                    MonthlyRate = l.Room != null ? l.Room.MonthlyRate : 0,
                    DormitoryName = l.Room != null && l.Room.Dormitory != null ? l.Room.Dormitory.Name : "Unassigned",
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    Status = l.Status,
                    TotalAmountPaid = l.Payments.Where(p => p.Status == "Completed").Sum(p => p.AmountPaid),
                    PaymentCount = l.Payments.Count
                })
                .ToListAsync();

            // Populate balances
            foreach (var lease in leases)
            {
                int months = Math.Max(1, ((lease.EndDate.Year - lease.StartDate.Year) * 12) + lease.EndDate.Month - lease.StartDate.Month);
                lease.EstimatedTotalRent = months * lease.MonthlyRate;
                lease.BalanceRemaining = Math.Max(0, lease.EstimatedTotalRent - lease.TotalAmountPaid);
            }

            return Ok(leases);
        }

        /// <summary>
        /// Get currently active leases
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<LeaseResponseDto>>> GetActiveLeases()
        {
            return await GetAllLeases(null, null, "Active");
        }

        /// <summary>
        /// Get lease details by ID with payments and financial balance
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<LeaseResponseDto>> GetLeaseById(int id)
        {
            var lease = await _context.Leases
                .Include(l => l.User)
                .Include(l => l.Room)
                    .ThenInclude(r => r!.Dormitory)
                .Include(l => l.Payments)
                .FirstOrDefaultAsync(l => l.LeaseId == id);

            if (lease == null)
                return NotFound(new { message = $"Lease with ID {id} not found." });

            var totalPaid = lease.Payments.Where(p => p.Status == "Completed").Sum(p => p.AmountPaid);
            int months = Math.Max(1, ((lease.EndDate.Year - lease.StartDate.Year) * 12) + lease.EndDate.Month - lease.StartDate.Month);
            var monthlyRate = lease.Room?.MonthlyRate ?? 0;
            var expectedTotalRent = months * monthlyRate;

            var response = new LeaseResponseDto
            {
                LeaseId = lease.LeaseId,
                UserId = lease.UserId,
                TenantName = lease.User?.FullName ?? "Unknown",
                TenantEmail = lease.User?.Email,
                TenantPhone = lease.User?.PhoneNumber,
                RoomId = lease.RoomId,
                RoomNumber = lease.Room?.RoomNumber ?? "Unknown",
                RoomType = lease.Room?.RoomType,
                MonthlyRate = monthlyRate,
                DormitoryName = lease.Room?.Dormitory?.Name ?? "Unassigned",
                StartDate = lease.StartDate,
                EndDate = lease.EndDate,
                Status = lease.Status,
                TotalAmountPaid = totalPaid,
                EstimatedTotalRent = expectedTotalRent,
                BalanceRemaining = Math.Max(0, expectedTotalRent - totalPaid),
                PaymentCount = lease.Payments.Count
            };

            return Ok(response);
        }

        /// <summary>
        /// Create a new lease / Check-in tenant (allocates room slot and updates status)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<LeaseResponseDto>> CreateLease([FromBody] LeaseCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.EndDate <= dto.StartDate)
                return BadRequest(new { message = "End date must be after start date." });

            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                return NotFound(new { message = $"User with ID {dto.UserId} not found." });

            var room = await _context.Rooms
                .Include(r => r.Dormitory)
                .FirstOrDefaultAsync(r => r.RoomId == dto.RoomId);

            if (room == null)
                return NotFound(new { message = $"Room with ID {dto.RoomId} not found." });

            // Check if room is available and has vacant capacity
            if (room.CurrentOccupants >= room.Capacity)
            {
                return BadRequest(new { message = $"Room '{room.RoomNumber}' is already at full capacity ({room.Capacity} occupants)." });
            }

            if (room.Status.Equals("Maintenance", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = $"Room '{room.RoomNumber}' is currently under maintenance." });
            }

            var lease = new Lease
            {
                UserId = dto.UserId,
                RoomId = dto.RoomId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status.Trim()
            };

            // Increment room occupants if lease is active
            if (lease.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                room.CurrentOccupants += 1;
                if (room.CurrentOccupants >= room.Capacity)
                {
                    room.Status = "Occupied";
                }
            }

            _context.Leases.Add(lease);
            await _context.SaveChangesAsync();

            var response = new LeaseResponseDto
            {
                LeaseId = lease.LeaseId,
                UserId = lease.UserId,
                TenantName = user.FullName,
                TenantEmail = user.Email,
                TenantPhone = user.PhoneNumber,
                RoomId = lease.RoomId,
                RoomNumber = room.RoomNumber,
                RoomType = room.RoomType,
                MonthlyRate = room.MonthlyRate,
                DormitoryName = room.Dormitory?.Name ?? "Unassigned",
                StartDate = lease.StartDate,
                EndDate = lease.EndDate,
                Status = lease.Status,
                TotalAmountPaid = 0,
                EstimatedTotalRent = Math.Max(1, ((lease.EndDate.Year - lease.StartDate.Year) * 12) + lease.EndDate.Month - lease.StartDate.Month) * room.MonthlyRate,
                BalanceRemaining = Math.Max(1, ((lease.EndDate.Year - lease.StartDate.Year) * 12) + lease.EndDate.Month - lease.StartDate.Month) * room.MonthlyRate,
                PaymentCount = 0
            };

            return CreatedAtAction(nameof(GetLeaseById), new { id = lease.LeaseId }, response);
        }

        /// <summary>
        /// Terminate / Check-out tenant from a lease (frees room slot and updates status)
        /// </summary>
        [HttpPost("{id}/terminate")]
        public async Task<IActionResult> TerminateLease(int id)
        {
            var lease = await _context.Leases
                .Include(l => l.Room)
                .FirstOrDefaultAsync(l => l.LeaseId == id);

            if (lease == null)
                return NotFound(new { message = $"Lease with ID {id} not found." });

            if (lease.Status == "Terminated")
                return BadRequest(new { message = "Lease is already terminated." });

            bool wasActive = lease.Status == "Active";
            lease.Status = "Terminated";

            if (wasActive && lease.Room != null)
            {
                lease.Room.CurrentOccupants = Math.Max(0, lease.Room.CurrentOccupants - 1);
                if (lease.Room.CurrentOccupants < lease.Room.Capacity && lease.Room.Status != "Maintenance")
                {
                    lease.Room.Status = "Available";
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Lease terminated successfully and tenant checked out.",
                leaseId = lease.LeaseId,
                roomOccupancy = lease.Room?.CurrentOccupants,
                roomStatus = lease.Room?.Status
            });
        }

        /// <summary>
        /// Renew a lease with a new end date
        /// </summary>
        [HttpPost("{id}/renew")]
        public async Task<IActionResult> RenewLease(int id, [FromBody] LeaseRenewDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lease = await _context.Leases.FindAsync(id);
            if (lease == null)
                return NotFound(new { message = $"Lease with ID {id} not found." });

            if (dto.NewEndDate <= lease.EndDate)
                return BadRequest(new { message = "New end date must be after current end date." });

            lease.EndDate = dto.NewEndDate;
            lease.Status = "Active";

            await _context.SaveChangesAsync();

            return Ok(new { message = "Lease renewed successfully.", leaseId = lease.LeaseId, newEndDate = lease.EndDate });
        }

        /// <summary>
        /// Get detailed rent balance calculation for a lease
        /// </summary>
        [HttpGet("{id}/balance")]
        public async Task<ActionResult<LeaseBalanceDto>> GetLeaseBalance(int id)
        {
            var lease = await _context.Leases
                .Include(l => l.User)
                .Include(l => l.Room)
                .Include(l => l.Payments)
                .FirstOrDefaultAsync(l => l.LeaseId == id);

            if (lease == null)
                return NotFound(new { message = $"Lease with ID {id} not found." });

            var monthlyRate = lease.Room?.MonthlyRate ?? 0;
            int totalMonths = Math.Max(1, ((lease.EndDate.Year - lease.StartDate.Year) * 12) + lease.EndDate.Month - lease.StartDate.Month);
            decimal expectedTotalRent = totalMonths * monthlyRate;

            var completedPayments = lease.Payments
                .Where(p => p.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                .ToList();

            decimal totalPaid = completedPayments.Sum(p => p.AmountPaid);
            decimal remainingBalance = Math.Max(0, expectedTotalRent - totalPaid);

            string paymentStatus = remainingBalance == 0 ? "Fully Paid" : (totalPaid > 0 ? "Partially Paid" : "Unpaid");

            var result = new LeaseBalanceDto
            {
                LeaseId = lease.LeaseId,
                TenantName = lease.User?.FullName,
                RoomNumber = lease.Room?.RoomNumber,
                MonthlyRate = monthlyRate,
                StartDate = lease.StartDate,
                EndDate = lease.EndDate,
                TotalMonths = totalMonths,
                ExpectedTotalRent = expectedTotalRent,
                TotalPaid = totalPaid,
                RemainingBalance = remainingBalance,
                PaymentStatus = paymentStatus,
                Payments = completedPayments.Select(p => new PaymentResponseDto
                {
                    PaymentId = p.PaymentId,
                    LeaseId = p.LeaseId,
                    AmountPaid = p.AmountPaid,
                    PaymentDate = p.PaymentDate,
                    PaymentMethod = p.PaymentMethod,
                    ReferenceNumber = p.ReferenceNumber,
                    Status = p.Status
                }).ToList()
            };

            return Ok(result);
        }
    }
}
