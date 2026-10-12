using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Boarding_House_Management_System.Data;
using Boarding_House_Management_System.DTOs;
using Boarding_House_Management_System.Model;

namespace Boarding_House_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaintenanceRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MaintenanceRequestsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all maintenance requests with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MaintenanceRequestResponseDto>>> GetAllRequests(
            [FromQuery] int? roomId,
            [FromQuery] int? userId,
            [FromQuery] string? status)
        {
            var query = _context.MaintenanceRequests
                .Include(mr => mr.Room)
                    .ThenInclude(r => r!.Dormitory)
                .Include(mr => mr.User)
                .AsQueryable();

            if (roomId.HasValue)
                query = query.Where(mr => mr.RoomId == roomId.Value);

            if (userId.HasValue)
                query = query.Where(mr => mr.UserId == userId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(mr => mr.Status.ToLower() == status.ToLower());

            var requests = await query
                .OrderByDescending(mr => mr.DateReported)
                .Select(mr => new MaintenanceRequestResponseDto
                {
                    RequestId = mr.RequestId,
                    RoomId = mr.RoomId,
                    RoomNumber = mr.Room != null ? mr.Room.RoomNumber : "Unknown",
                    DormitoryName = mr.Room != null && mr.Room.Dormitory != null ? mr.Room.Dormitory.Name : "Unassigned",
                    UserId = mr.UserId,
                    RequesterName = mr.User != null ? mr.User.FullName : "Unknown",
                    RequesterContact = mr.User != null ? mr.User.PhoneNumber ?? mr.User.Email : null,
                    IssueDescription = mr.IssueDescription,
                    DateReported = mr.DateReported,
                    Status = mr.Status,
                    ResolvedAt = mr.ResolvedAt
                })
                .ToListAsync();

            return Ok(requests);
        }

        /// <summary>
        /// Get maintenance request details by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<MaintenanceRequestResponseDto>> GetRequestById(int id)
        {
            var mr = await _context.MaintenanceRequests
                .Include(m => m.Room)
                    .ThenInclude(r => r!.Dormitory)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.RequestId == id);

            if (mr == null)
                return NotFound(new { message = $"Maintenance request with ID {id} not found." });

            var response = new MaintenanceRequestResponseDto
            {
                RequestId = mr.RequestId,
                RoomId = mr.RoomId,
                RoomNumber = mr.Room?.RoomNumber ?? "Unknown",
                DormitoryName = mr.Room?.Dormitory?.Name ?? "Unassigned",
                UserId = mr.UserId,
                RequesterName = mr.User?.FullName ?? "Unknown",
                RequesterContact = mr.User?.PhoneNumber ?? mr.User?.Email,
                IssueDescription = mr.IssueDescription,
                DateReported = mr.DateReported,
                Status = mr.Status,
                ResolvedAt = mr.ResolvedAt
            };

            return Ok(response);
        }

        /// <summary>
        /// Submit a new maintenance or repair request
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<MaintenanceRequestResponseDto>> CreateRequest([FromBody] MaintenanceRequestCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var room = await _context.Rooms
                .Include(r => r.Dormitory)
                .FirstOrDefaultAsync(r => r.RoomId == dto.RoomId);

            if (room == null)
                return NotFound(new { message = $"Room with ID {dto.RoomId} not found." });

            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                return NotFound(new { message = $"User with ID {dto.UserId} not found." });

            var request = new MaintenanceRequest
            {
                RoomId = dto.RoomId,
                UserId = dto.UserId,
                IssueDescription = dto.IssueDescription.Trim(),
                DateReported = DateTime.UtcNow,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Pending" : dto.Status.Trim(),
                ResolvedAt = null
            };

            _context.MaintenanceRequests.Add(request);
            await _context.SaveChangesAsync();

            var response = new MaintenanceRequestResponseDto
            {
                RequestId = request.RequestId,
                RoomId = room.RoomId,
                RoomNumber = room.RoomNumber,
                DormitoryName = room.Dormitory?.Name ?? "Unassigned",
                UserId = user.UserId,
                RequesterName = user.FullName,
                RequesterContact = user.PhoneNumber ?? user.Email,
                IssueDescription = request.IssueDescription,
                DateReported = request.DateReported,
                Status = request.Status,
                ResolvedAt = null
            };

            return CreatedAtAction(nameof(GetRequestById), new { id = request.RequestId }, response);
        }

        /// <summary>
        /// Update status of a maintenance request (Pending -> In Progress -> Resolved)
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] MaintenanceStatusUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var mr = await _context.MaintenanceRequests.FindAsync(id);
            if (mr == null)
                return NotFound(new { message = $"Maintenance request with ID {id} not found." });

            mr.Status = dto.Status.Trim();

            if (mr.Status.Equals("Resolved", StringComparison.OrdinalIgnoreCase))
            {
                mr.ResolvedAt = DateTime.UtcNow;
            }
            else
            {
                mr.ResolvedAt = null;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Maintenance request status updated to '{mr.Status}'.",
                requestId = mr.RequestId,
                status = mr.Status,
                resolvedAt = mr.ResolvedAt
            });
        }

        /// <summary>
        /// Delete maintenance request
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var mr = await _context.MaintenanceRequests.FindAsync(id);
            if (mr == null)
                return NotFound(new { message = $"Maintenance request with ID {id} not found." });

            _context.MaintenanceRequests.Remove(mr);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Maintenance request deleted successfully." });
        }
    }
}
