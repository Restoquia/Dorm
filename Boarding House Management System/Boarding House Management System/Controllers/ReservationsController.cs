using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Boarding_House_Management_System.Data;
using Boarding_House_Management_System.DTOs;
using Boarding_House_Management_System.Model;

namespace Boarding_House_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReservationsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all reservations with optional filters
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservationResponseDto>>> GetAllReservations(
            [FromQuery] int? userId,
            [FromQuery] int? roomId,
            [FromQuery] string? status)
        {
            var query = _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Room)
                    .ThenInclude(rm => rm!.Dormitory)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(r => r.UserId == userId.Value);

            if (roomId.HasValue)
                query = query.Where(r => r.RoomId == roomId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(r => r.Status.ToLower() == status.ToLower());

            var reservations = await query
                .OrderByDescending(r => r.ReservationDate)
                .Select(r => new ReservationResponseDto
                {
                    ReservationId = r.ReservationId,
                    UserId = r.UserId,
                    UserName = r.User != null ? r.User.FullName : "Unknown",
                    UserEmail = r.User != null ? r.User.Email : null,
                    UserPhone = r.User != null ? r.User.PhoneNumber : null,
                    RoomId = r.RoomId,
                    RoomNumber = r.Room != null ? r.Room.RoomNumber : "Unknown",
                    RoomType = r.Room != null ? r.Room.RoomType : null,
                    MonthlyRate = r.Room != null ? r.Room.MonthlyRate : 0,
                    DormitoryName = r.Room != null && r.Room.Dormitory != null ? r.Room.Dormitory.Name : "Unassigned",
                    ReservationDate = r.ReservationDate,
                    Status = r.Status
                })
                .ToListAsync();

            return Ok(reservations);
        }

        /// <summary>
        /// Get reservation details by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ReservationResponseDto>> GetReservationById(int id)
        {
            var res = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Room)
                    .ThenInclude(rm => rm!.Dormitory)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (res == null)
                return NotFound(new { message = $"Reservation with ID {id} not found." });

            var response = new ReservationResponseDto
            {
                ReservationId = res.ReservationId,
                UserId = res.UserId,
                UserName = res.User?.FullName ?? "Unknown",
                UserEmail = res.User?.Email,
                UserPhone = res.User?.PhoneNumber,
                RoomId = res.RoomId,
                RoomNumber = res.Room?.RoomNumber ?? "Unknown",
                RoomType = res.Room?.RoomType,
                MonthlyRate = res.Room?.MonthlyRate ?? 0,
                DormitoryName = res.Room?.Dormitory?.Name ?? "Unassigned",
                ReservationDate = res.ReservationDate,
                Status = res.Status
            };

            return Ok(response);
        }

        /// <summary>
        /// Create a reservation / Book an available room
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ReservationResponseDto>> CreateReservation([FromBody] ReservationCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                return NotFound(new { message = $"User with ID {dto.UserId} not found." });

            var room = await _context.Rooms
                .Include(r => r.Dormitory)
                .FirstOrDefaultAsync(r => r.RoomId == dto.RoomId);

            if (room == null)
                return NotFound(new { message = $"Room with ID {dto.RoomId} not found." });

            if (room.CurrentOccupants >= room.Capacity)
                return BadRequest(new { message = $"Room '{room.RoomNumber}' is already at full capacity." });

            if (room.Status.Equals("Maintenance", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = $"Room '{room.RoomNumber}' is currently under maintenance." });

            var reservation = new Reservation
            {
                UserId = dto.UserId,
                RoomId = dto.RoomId,
                ReservationDate = dto.ReservationDate ?? DateTime.UtcNow,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Pending" : dto.Status.Trim()
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            var response = new ReservationResponseDto
            {
                ReservationId = reservation.ReservationId,
                UserId = reservation.UserId,
                UserName = user.FullName,
                UserEmail = user.Email,
                UserPhone = user.PhoneNumber,
                RoomId = room.RoomId,
                RoomNumber = room.RoomNumber,
                RoomType = room.RoomType,
                MonthlyRate = room.MonthlyRate,
                DormitoryName = room.Dormitory?.Name ?? "Unassigned",
                ReservationDate = reservation.ReservationDate,
                Status = reservation.Status
            };

            return CreatedAtAction(nameof(GetReservationById), new { id = reservation.ReservationId }, response);
        }

        /// <summary>
        /// Approve a pending reservation
        /// </summary>
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveReservation(int id)
        {
            var res = await _context.Reservations.FindAsync(id);
            if (res == null)
                return NotFound(new { message = $"Reservation with ID {id} not found." });

            res.Status = "Approved";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Reservation approved successfully.", reservationId = res.ReservationId, status = res.Status });
        }

        /// <summary>
        /// Cancel a reservation
        /// </summary>
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var res = await _context.Reservations.FindAsync(id);
            if (res == null)
                return NotFound(new { message = $"Reservation with ID {id} not found." });

            res.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Reservation cancelled.", reservationId = res.ReservationId, status = res.Status });
        }

        /// <summary>
        /// Convert an approved reservation into an active lease (moves tenant into room)
        /// </summary>
        [HttpPost("{id}/convert-to-lease")]
        public async Task<ActionResult<LeaseResponseDto>> ConvertToLease(int id, [FromBody] ReservationConvertToLeaseDto dto)
        {
            var res = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Room)
                    .ThenInclude(rm => rm!.Dormitory)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (res == null)
                return NotFound(new { message = $"Reservation with ID {id} not found." });

            var room = res.Room;
            if (room == null)
                return NotFound(new { message = "Room associated with this reservation was not found." });

            if (room.CurrentOccupants >= room.Capacity)
                return BadRequest(new { message = $"Room '{room.RoomNumber}' is already at full capacity." });

            // Create lease
            var lease = new Lease
            {
                UserId = res.UserId,
                RoomId = res.RoomId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = "Active"
            };

            // Increment occupants
            room.CurrentOccupants += 1;
            if (room.CurrentOccupants >= room.Capacity)
                room.Status = "Occupied";

            // Mark reservation as converted
            res.Status = "Converted";

            _context.Leases.Add(lease);
            await _context.SaveChangesAsync();

            var response = new LeaseResponseDto
            {
                LeaseId = lease.LeaseId,
                UserId = lease.UserId,
                TenantName = res.User?.FullName,
                TenantEmail = res.User?.Email,
                TenantPhone = res.User?.PhoneNumber,
                RoomId = room.RoomId,
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

            return Ok(response);
        }

        /// <summary>
        /// Delete reservation record
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var res = await _context.Reservations.FindAsync(id);
            if (res == null)
                return NotFound(new { message = $"Reservation with ID {id} not found." });

            _context.Reservations.Remove(res);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Reservation removed successfully." });
        }
    }
}
