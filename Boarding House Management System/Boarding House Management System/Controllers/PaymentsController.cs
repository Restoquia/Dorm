using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Boarding_House_Management_System.Data;
using Boarding_House_Management_System.DTOs;
using Boarding_House_Management_System.Model;

namespace Boarding_House_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Pay rent / Record a rent payment for a lease
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PaymentResponseDto>> PayRent([FromBody] PaymentCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lease = await _context.Leases
                .Include(l => l.User)
                .Include(l => l.Room)
                    .ThenInclude(r => r!.Dormitory)
                .FirstOrDefaultAsync(l => l.LeaseId == dto.LeaseId);

            if (lease == null)
                return NotFound(new { message = $"Lease with ID {dto.LeaseId} not found." });

            if (lease.Status != "Active" && lease.Status != "Pending")
            {
                return BadRequest(new { message = $"Cannot pay rent for a lease with status '{lease.Status}'." });
            }

            // Auto-generate reference number if not provided
            string referenceNumber = !string.IsNullOrWhiteSpace(dto.ReferenceNumber)
                ? dto.ReferenceNumber.Trim()
                : $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}";

            var payment = new Payment
            {
                LeaseId = dto.LeaseId,
                AmountPaid = dto.AmountPaid,
                PaymentDate = dto.PaymentDate ?? DateTime.UtcNow,
                PaymentMethod = string.IsNullOrWhiteSpace(dto.PaymentMethod) ? "Cash" : dto.PaymentMethod.Trim(),
                ReferenceNumber = referenceNumber,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Completed" : dto.Status.Trim()
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            var response = new PaymentResponseDto
            {
                PaymentId = payment.PaymentId,
                LeaseId = payment.LeaseId,
                TenantName = lease.User?.FullName ?? "Unknown",
                UserId = lease.UserId,
                RoomNumber = lease.Room?.RoomNumber ?? "Unknown",
                DormitoryName = lease.Room?.Dormitory?.Name ?? "Unassigned",
                AmountPaid = payment.AmountPaid,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod,
                ReferenceNumber = payment.ReferenceNumber,
                Status = payment.Status,
                LeaseMonthlyRate = lease.Room?.MonthlyRate ?? 0
            };

            return CreatedAtAction(nameof(GetPaymentById), new { id = payment.PaymentId }, response);
        }

        /// <summary>
        /// Get all payments with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentResponseDto>>> GetAllPayments(
            [FromQuery] int? leaseId,
            [FromQuery] int? userId,
            [FromQuery] string? paymentMethod,
            [FromQuery] string? status,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var query = _context.Payments
                .Include(p => p.Lease)
                    .ThenInclude(l => l!.User)
                .Include(p => p.Lease)
                    .ThenInclude(l => l!.Room)
                        .ThenInclude(r => r!.Dormitory)
                .AsQueryable();

            if (leaseId.HasValue)
                query = query.Where(p => p.LeaseId == leaseId.Value);

            if (userId.HasValue)
                query = query.Where(p => p.Lease != null && p.Lease.UserId == userId.Value);

            if (!string.IsNullOrWhiteSpace(paymentMethod))
                query = query.Where(p => p.PaymentMethod.ToLower() == paymentMethod.ToLower());

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(p => p.Status.ToLower() == status.ToLower());

            if (fromDate.HasValue)
                query = query.Where(p => p.PaymentDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(p => p.PaymentDate <= toDate.Value);

            var payments = await query
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new PaymentResponseDto
                {
                    PaymentId = p.PaymentId,
                    LeaseId = p.LeaseId,
                    TenantName = p.Lease != null && p.Lease.User != null ? p.Lease.User.FullName : "Unknown",
                    UserId = p.Lease != null ? p.Lease.UserId : null,
                    RoomNumber = p.Lease != null && p.Lease.Room != null ? p.Lease.Room.RoomNumber : "Unknown",
                    DormitoryName = p.Lease != null && p.Lease.Room != null && p.Lease.Room.Dormitory != null ? p.Lease.Room.Dormitory.Name : "Unassigned",
                    AmountPaid = p.AmountPaid,
                    PaymentDate = p.PaymentDate,
                    PaymentMethod = p.PaymentMethod,
                    ReferenceNumber = p.ReferenceNumber,
                    Status = p.Status,
                    LeaseMonthlyRate = p.Lease != null && p.Lease.Room != null ? p.Lease.Room.MonthlyRate : 0
                })
                .ToListAsync();

            return Ok(payments);
        }

        /// <summary>
        /// Get payment receipt by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentResponseDto>> GetPaymentById(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Lease)
                    .ThenInclude(l => l!.User)
                .Include(p => p.Lease)
                    .ThenInclude(l => l!.Room)
                        .ThenInclude(r => r!.Dormitory)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null)
                return NotFound(new { message = $"Payment with ID {id} not found." });

            var response = new PaymentResponseDto
            {
                PaymentId = payment.PaymentId,
                LeaseId = payment.LeaseId,
                TenantName = payment.Lease?.User?.FullName ?? "Unknown",
                UserId = payment.Lease?.UserId,
                RoomNumber = payment.Lease?.Room?.RoomNumber ?? "Unknown",
                DormitoryName = payment.Lease?.Room?.Dormitory?.Name ?? "Unassigned",
                AmountPaid = payment.AmountPaid,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod,
                ReferenceNumber = payment.ReferenceNumber,
                Status = payment.Status,
                LeaseMonthlyRate = payment.Lease?.Room?.MonthlyRate ?? 0
            };

            return Ok(response);
        }

        /// <summary>
        /// Get all payments and statement of account for a specific lease
        /// </summary>
        [HttpGet("lease/{leaseId}")]
        public async Task<ActionResult<object>> GetPaymentsForLease(int leaseId)
        {
            var lease = await _context.Leases
                .Include(l => l.User)
                .Include(l => l.Room)
                    .ThenInclude(r => r!.Dormitory)
                .Include(l => l.Payments)
                .FirstOrDefaultAsync(l => l.LeaseId == leaseId);

            if (lease == null)
                return NotFound(new { message = $"Lease with ID {leaseId} not found." });

            var monthlyRate = lease.Room?.MonthlyRate ?? 0;
            var totalPaid = lease.Payments
                .Where(p => p.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                .Sum(p => p.AmountPaid);

            // Compute months duration
            int months = Math.Max(1, ((lease.EndDate.Year - lease.StartDate.Year) * 12) + lease.EndDate.Month - lease.StartDate.Month);
            decimal totalExpectedRent = months * monthlyRate;
            decimal remainingBalance = Math.Max(0, totalExpectedRent - totalPaid);

            var paymentsList = lease.Payments
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new PaymentResponseDto
                {
                    PaymentId = p.PaymentId,
                    LeaseId = p.LeaseId,
                    TenantName = lease.User?.FullName,
                    UserId = lease.UserId,
                    RoomNumber = lease.Room?.RoomNumber,
                    DormitoryName = lease.Room?.Dormitory?.Name,
                    AmountPaid = p.AmountPaid,
                    PaymentDate = p.PaymentDate,
                    PaymentMethod = p.PaymentMethod,
                    ReferenceNumber = p.ReferenceNumber,
                    Status = p.Status,
                    LeaseMonthlyRate = monthlyRate
                })
                .ToList();

            return Ok(new
            {
                leaseId = lease.LeaseId,
                tenantName = lease.User?.FullName,
                roomNumber = lease.Room?.RoomNumber,
                dormitoryName = lease.Room?.Dormitory?.Name,
                monthlyRate,
                startDate = lease.StartDate,
                endDate = lease.EndDate,
                totalMonths = months,
                totalExpectedRent,
                totalPaid,
                remainingBalance,
                isFullyPaid = totalPaid >= totalExpectedRent,
                payments = paymentsList
            });
        }

        /// <summary>
        /// Get all payments made by a tenant/user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<PaymentResponseDto>>> GetPaymentsByUser(int userId)
        {
            var payments = await _context.Payments
                .Include(p => p.Lease)
                    .ThenInclude(l => l!.User)
                .Include(p => p.Lease)
                    .ThenInclude(l => l!.Room)
                        .ThenInclude(r => r!.Dormitory)
                .Where(p => p.Lease != null && p.Lease.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new PaymentResponseDto
                {
                    PaymentId = p.PaymentId,
                    LeaseId = p.LeaseId,
                    TenantName = p.Lease!.User != null ? p.Lease.User.FullName : "Unknown",
                    UserId = p.Lease.UserId,
                    RoomNumber = p.Lease.Room != null ? p.Lease.Room.RoomNumber : "Unknown",
                    DormitoryName = p.Lease.Room != null && p.Lease.Room.Dormitory != null ? p.Lease.Room.Dormitory.Name : "Unassigned",
                    AmountPaid = p.AmountPaid,
                    PaymentDate = p.PaymentDate,
                    PaymentMethod = p.PaymentMethod,
                    ReferenceNumber = p.ReferenceNumber,
                    Status = p.Status,
                    LeaseMonthlyRate = p.Lease.Room != null ? p.Lease.Room.MonthlyRate : 0
                })
                .ToListAsync();

            return Ok(payments);
        }

        /// <summary>
        /// Financial summary and payment statistics
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<PaymentSummaryDto>> GetPaymentSummary()
        {
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

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

            var revenueByMethod = completedPayments
                .GroupBy(p => p.PaymentMethod)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.AmountPaid));

            var recentPayments = completedPayments
                .OrderByDescending(p => p.PaymentDate)
                .Take(5)
                .Select(p => new PaymentResponseDto
                {
                    PaymentId = p.PaymentId,
                    LeaseId = p.LeaseId,
                    TenantName = p.Lease?.User?.FullName ?? "Unknown",
                    UserId = p.Lease?.UserId,
                    RoomNumber = p.Lease?.Room?.RoomNumber ?? "Unknown",
                    DormitoryName = p.Lease?.Room?.Dormitory?.Name ?? "Unassigned",
                    AmountPaid = p.AmountPaid,
                    PaymentDate = p.PaymentDate,
                    PaymentMethod = p.PaymentMethod,
                    ReferenceNumber = p.ReferenceNumber,
                    Status = p.Status,
                    LeaseMonthlyRate = p.Lease?.Room?.MonthlyRate ?? 0
                })
                .ToList();

            var summary = new PaymentSummaryDto
            {
                TotalRevenue = totalRevenue,
                ThisMonthRevenue = thisMonthRevenue,
                TotalTransactions = completedPayments.Count,
                RevenueByMethod = revenueByMethod,
                RecentPayments = recentPayments
            };

            return Ok(summary);
        }

        /// <summary>
        /// Update status of a payment (e.g., Completed, Cancelled, Refunded)
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] PaymentStatusUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                return NotFound(new { message = $"Payment with ID {id} not found." });

            payment.Status = dto.Status.Trim();
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Payment status updated to '{payment.Status}'.", paymentId = payment.PaymentId, status = payment.Status });
        }
    }
}
