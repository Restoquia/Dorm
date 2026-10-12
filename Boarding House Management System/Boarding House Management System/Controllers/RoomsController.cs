using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Boarding_House_Management_System.Data;
using Boarding_House_Management_System.DTOs;
using Boarding_House_Management_System.Model;

namespace Boarding_House_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoomsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all rooms with optional filters (dormitory, status, type, rate range, search query)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomResponseDto>>> GetAllRooms(
            [FromQuery] int? dormitoryId,
            [FromQuery] string? status,
            [FromQuery] string? roomType,
            [FromQuery] decimal? minRate,
            [FromQuery] decimal? maxRate,
            [FromQuery] string? search)
        {
            var query = _context.Rooms
                .Include(r => r.Dormitory)
                .AsQueryable();

            if (dormitoryId.HasValue)
                query = query.Where(r => r.DormitoryId == dormitoryId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(r => r.Status.ToLower() == status.ToLower());

            if (!string.IsNullOrWhiteSpace(roomType))
                query = query.Where(r => r.RoomType.ToLower() == roomType.ToLower());

            if (minRate.HasValue)
                query = query.Where(r => r.MonthlyRate >= minRate.Value);

            if (maxRate.HasValue)
                query = query.Where(r => r.MonthlyRate <= maxRate.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(r => r.RoomNumber.ToLower().Contains(s) ||
                                         r.RoomType.ToLower().Contains(s) ||
                                         (r.Dormitory != null && r.Dormitory.Name.ToLower().Contains(s)));
            }

            var rooms = await query
                .OrderBy(r => r.RoomNumber)
                .Select(r => new RoomResponseDto
                {
                    RoomId = r.RoomId,
                    DormitoryId = r.DormitoryId,
                    DormitoryName = r.Dormitory != null ? r.Dormitory.Name : "Unassigned",
                    RoomNumber = r.RoomNumber,
                    RoomType = r.RoomType,
                    Capacity = r.Capacity,
                    CurrentOccupants = r.CurrentOccupants,
                    MonthlyRate = r.MonthlyRate,
                    Status = r.Status
                })
                .ToListAsync();

            return Ok(rooms);
        }

        /// <summary>
        /// View only available rooms (status is Available and capacity has vacant slots)
        /// </summary>
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<RoomResponseDto>>> GetAvailableRooms(
            [FromQuery] int? dormitoryId,
            [FromQuery] string? roomType,
            [FromQuery] decimal? maxRate)
        {
            var query = _context.Rooms
                .Include(r => r.Dormitory)
                .Where(r => r.Status == "Available" && r.CurrentOccupants < r.Capacity);

            if (dormitoryId.HasValue)
                query = query.Where(r => r.DormitoryId == dormitoryId.Value);

            if (!string.IsNullOrWhiteSpace(roomType))
                query = query.Where(r => r.RoomType.ToLower() == roomType.ToLower());

            if (maxRate.HasValue)
                query = query.Where(r => r.MonthlyRate <= maxRate.Value);

            var availableRooms = await query
                .OrderBy(r => r.MonthlyRate)
                .Select(r => new RoomResponseDto
                {
                    RoomId = r.RoomId,
                    DormitoryId = r.DormitoryId,
                    DormitoryName = r.Dormitory != null ? r.Dormitory.Name : "Unassigned",
                    RoomNumber = r.RoomNumber,
                    RoomType = r.RoomType,
                    Capacity = r.Capacity,
                    CurrentOccupants = r.CurrentOccupants,
                    MonthlyRate = r.MonthlyRate,
                    Status = r.Status
                })
                .ToListAsync();

            return Ok(availableRooms);
        }

        /// <summary>
        /// Get details of a single room by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<RoomResponseDto>> GetRoomById(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.Dormitory)
                .FirstOrDefaultAsync(r => r.RoomId == id);

            if (room == null)
                return NotFound(new { message = $"Room with ID {id} not found." });

            var result = new RoomResponseDto
            {
                RoomId = room.RoomId,
                DormitoryId = room.DormitoryId,
                DormitoryName = room.Dormitory != null ? room.Dormitory.Name : "Unassigned",
                RoomNumber = room.RoomNumber,
                RoomType = room.RoomType,
                Capacity = room.Capacity,
                CurrentOccupants = room.CurrentOccupants,
                MonthlyRate = room.MonthlyRate,
                Status = room.Status
            };

            return Ok(result);
        }

        /// <summary>
        /// Add a new room
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<RoomResponseDto>> CreateRoom([FromBody] RoomCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.DormitoryId.HasValue)
            {
                var dormExists = await _context.Dormitories.AnyAsync(d => d.DormitoryId == dto.DormitoryId.Value);
                if (!dormExists)
                    return BadRequest(new { message = $"Dormitory with ID {dto.DormitoryId} does not exist." });
            }

            var duplicate = await _context.Rooms.AnyAsync(r =>
                r.DormitoryId == dto.DormitoryId &&
                r.RoomNumber.ToLower() == dto.RoomNumber.ToLower());

            if (duplicate)
                return Conflict(new { message = $"Room '{dto.RoomNumber}' already exists in this dormitory." });

            var room = new Room
            {
                DormitoryId = dto.DormitoryId,
                RoomNumber = dto.RoomNumber.Trim(),
                RoomType = dto.RoomType.Trim(),
                Capacity = dto.Capacity,
                CurrentOccupants = dto.CurrentOccupants,
                MonthlyRate = dto.MonthlyRate,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Available" : dto.Status.Trim()
            };

            // Auto-adjust status based on occupancy
            if (room.CurrentOccupants >= room.Capacity)
                room.Status = "Occupied";

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            // Load dormitory name
            string? dormName = null;
            if (room.DormitoryId.HasValue)
            {
                var dorm = await _context.Dormitories.FindAsync(room.DormitoryId.Value);
                dormName = dorm?.Name;
            }

            var response = new RoomResponseDto
            {
                RoomId = room.RoomId,
                DormitoryId = room.DormitoryId,
                DormitoryName = dormName ?? "Unassigned",
                RoomNumber = room.RoomNumber,
                RoomType = room.RoomType,
                Capacity = room.Capacity,
                CurrentOccupants = room.CurrentOccupants,
                MonthlyRate = room.MonthlyRate,
                Status = room.Status
            };

            return CreatedAtAction(nameof(GetRoomById), new { id = room.RoomId }, response);
        }

        /// <summary>
        /// Update room details
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] RoomUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
                return NotFound(new { message = $"Room with ID {id} not found." });

            if (dto.DormitoryId.HasValue)
            {
                var dormExists = await _context.Dormitories.AnyAsync(d => d.DormitoryId == dto.DormitoryId.Value);
                if (!dormExists)
                    return BadRequest(new { message = $"Dormitory with ID {dto.DormitoryId} does not exist." });
            }

            // Check duplicate room number
            var duplicate = await _context.Rooms.AnyAsync(r =>
                r.RoomId != id &&
                r.DormitoryId == dto.DormitoryId &&
                r.RoomNumber.ToLower() == dto.RoomNumber.ToLower());

            if (duplicate)
                return Conflict(new { message = $"Another room '{dto.RoomNumber}' already exists in this dormitory." });

            room.DormitoryId = dto.DormitoryId;
            room.RoomNumber = dto.RoomNumber.Trim();
            room.RoomType = dto.RoomType.Trim();
            room.Capacity = dto.Capacity;
            room.CurrentOccupants = dto.CurrentOccupants;
            room.MonthlyRate = dto.MonthlyRate;
            room.Status = dto.Status.Trim();

            // Auto-adjust status if full
            if (room.CurrentOccupants >= room.Capacity && room.Status == "Available")
                room.Status = "Occupied";
            else if (room.CurrentOccupants < room.Capacity && room.Status == "Occupied")
                room.Status = "Available";

            await _context.SaveChangesAsync();
            return Ok(new { message = "Room updated successfully.", room });
        }

        /// <summary>
        /// Quick update of room status (e.g. Available, Occupied, Maintenance)
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateRoomStatus(int id, [FromBody] RoomStatusUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
                return NotFound(new { message = $"Room with ID {id} not found." });

            room.Status = dto.Status.Trim();
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Room status changed to '{room.Status}'.", roomId = room.RoomId, status = room.Status });
        }

        /// <summary>
        /// Delete a room (only if there are no active leases)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.Leases)
                .FirstOrDefaultAsync(r => r.RoomId == id);

            if (room == null)
                return NotFound(new { message = $"Room with ID {id} not found." });

            var hasActiveLease = room.Leases.Any(l => l.Status == "Active");
            if (hasActiveLease)
                return BadRequest(new { message = "Cannot delete a room with an active lease. Terminate the lease first." });

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Room '{room.RoomNumber}' deleted successfully." });
        }
    }
}
