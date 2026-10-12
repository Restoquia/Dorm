using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Boarding_House_Management_System.Data;
using Boarding_House_Management_System.DTOs;
using Boarding_House_Management_System.Model;

namespace Boarding_House_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DormitoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DormitoriesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all dormitories with occupancy and room capacity statistics
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DormitoryResponseDto>>> GetAllDormitories()
        {
            var dormitories = await _context.Dormitories
                .Include(d => d.Rooms)
                .OrderBy(d => d.Name)
                .Select(d => new DormitoryResponseDto
                {
                    DormitoryId = d.DormitoryId,
                    Name = d.Name,
                    Address = d.Address,
                    ContactNumber = d.ContactNumber,
                    Description = d.Description,
                    CreatedAt = d.CreatedAt,
                    TotalRooms = d.Rooms.Count,
                    AvailableRooms = d.Rooms.Count(r => r.Status == "Available" && r.CurrentOccupants < r.Capacity),
                    TotalCapacity = d.Rooms.Sum(r => r.Capacity),
                    CurrentOccupants = d.Rooms.Sum(r => r.CurrentOccupants)
                })
                .ToListAsync();

            return Ok(dormitories);
        }

        /// <summary>
        /// Get dormitory by ID with its full room list
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<DormitoryDetailDto>> GetDormitoryById(int id)
        {
            var d = await _context.Dormitories
                .Include(dm => dm.Rooms)
                .FirstOrDefaultAsync(dm => dm.DormitoryId == id);

            if (d == null)
                return NotFound(new { message = $"Dormitory with ID {id} not found." });

            var result = new DormitoryDetailDto
            {
                DormitoryId = d.DormitoryId,
                Name = d.Name,
                Address = d.Address,
                ContactNumber = d.ContactNumber,
                Description = d.Description,
                CreatedAt = d.CreatedAt,
                TotalRooms = d.Rooms.Count,
                AvailableRooms = d.Rooms.Count(r => r.Status == "Available" && r.CurrentOccupants < r.Capacity),
                TotalCapacity = d.Rooms.Sum(r => r.Capacity),
                CurrentOccupants = d.Rooms.Sum(r => r.CurrentOccupants),
                Rooms = d.Rooms.Select(r => new RoomResponseDto
                {
                    RoomId = r.RoomId,
                    DormitoryId = r.DormitoryId,
                    DormitoryName = d.Name,
                    RoomNumber = r.RoomNumber,
                    RoomType = r.RoomType,
                    Capacity = r.Capacity,
                    CurrentOccupants = r.CurrentOccupants,
                    MonthlyRate = r.MonthlyRate,
                    Status = r.Status
                }).ToList()
            };

            return Ok(result);
        }

        /// <summary>
        /// Create a new dormitory
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<DormitoryResponseDto>> CreateDormitory([FromBody] DormitoryCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var duplicate = await _context.Dormitories.AnyAsync(d => d.Name.ToLower() == dto.Name.Trim().ToLower());
            if (duplicate)
                return Conflict(new { message = $"Dormitory with name '{dto.Name}' already exists." });

            var dorm = new Dormitory
            {
                Name = dto.Name.Trim(),
                Address = dto.Address?.Trim(),
                ContactNumber = dto.ContactNumber?.Trim(),
                Description = dto.Description?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Dormitories.Add(dorm);
            await _context.SaveChangesAsync();

            var response = new DormitoryResponseDto
            {
                DormitoryId = dorm.DormitoryId,
                Name = dorm.Name,
                Address = dorm.Address,
                ContactNumber = dorm.ContactNumber,
                Description = dorm.Description,
                CreatedAt = dorm.CreatedAt,
                TotalRooms = 0,
                AvailableRooms = 0,
                TotalCapacity = 0,
                CurrentOccupants = 0
            };

            return CreatedAtAction(nameof(GetDormitoryById), new { id = dorm.DormitoryId }, response);
        }

        /// <summary>
        /// Update dormitory information
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDormitory(int id, [FromBody] DormitoryUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dorm = await _context.Dormitories.FindAsync(id);
            if (dorm == null)
                return NotFound(new { message = $"Dormitory with ID {id} not found." });

            var duplicate = await _context.Dormitories.AnyAsync(d => d.DormitoryId != id && d.Name.ToLower() == dto.Name.Trim().ToLower());
            if (duplicate)
                return Conflict(new { message = $"Another dormitory with name '{dto.Name}' already exists." });

            dorm.Name = dto.Name.Trim();
            dorm.Address = dto.Address?.Trim();
            dorm.ContactNumber = dto.ContactNumber?.Trim();
            dorm.Description = dto.Description?.Trim();

            await _context.SaveChangesAsync();
            return Ok(new { message = "Dormitory updated successfully.", dormitory = dorm });
        }

        /// <summary>
        /// Delete a dormitory
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDormitory(int id)
        {
            var dorm = await _context.Dormitories
                .Include(d => d.Rooms)
                .FirstOrDefaultAsync(d => d.DormitoryId == id);

            if (dorm == null)
                return NotFound(new { message = $"Dormitory with ID {id} not found." });

            if (dorm.Rooms.Any(r => r.CurrentOccupants > 0))
                return BadRequest(new { message = "Cannot delete dormitory with occupied rooms. Terminate active leases first." });

            _context.Dormitories.Remove(dorm);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Dormitory '{dorm.Name}' deleted successfully." });
        }
    }
}
