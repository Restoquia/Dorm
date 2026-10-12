using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Boarding_House_Management_System.Data;
using Boarding_House_Management_System.DTOs;
using Boarding_House_Management_System.Model;
using Boarding_House_Management_System.Services;

namespace Boarding_House_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all users with optional role and search filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllUsers(
            [FromQuery] string? role,
            [FromQuery] string? search)
        {
            var query = _context.Users
                .Include(u => u.Leases)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(role))
                query = query.Where(u => u.Role.ToLower() == role.ToLower());

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(u => u.FullName.ToLower().Contains(s) ||
                                         u.Username.ToLower().Contains(s) ||
                                         u.Email.ToLower().Contains(s));
            }

            var users = await query
                .OrderBy(u => u.FullName)
                .Select(u => new UserResponseDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                    ActiveLeasesCount = u.Leases.Count(l => l.Status == "Active")
                })
                .ToListAsync();

            return Ok(users);
        }

        /// <summary>
        /// Get detailed user profile including leases, reservations, and maintenance requests
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDetailDto>> GetUserById(int id)
        {
            var user = await _context.Users
                .Include(u => u.Leases)
                    .ThenInclude(l => l.Room)
                        .ThenInclude(r => r!.Dormitory)
                .Include(u => u.Leases)
                    .ThenInclude(l => l.Payments)
                .Include(u => u.Reservations)
                    .ThenInclude(res => res.Room)
                        .ThenInclude(r => r!.Dormitory)
                .Include(u => u.MaintenanceRequests)
                    .ThenInclude(mr => mr.Room)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found." });

            var result = new UserDetailDto
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                ActiveLeasesCount = user.Leases.Count(l => l.Status == "Active"),
                Leases = user.Leases.Select(l => new LeaseResponseDto
                {
                    LeaseId = l.LeaseId,
                    UserId = l.UserId,
                    TenantName = user.FullName,
                    RoomId = l.RoomId,
                    RoomNumber = l.Room?.RoomNumber ?? "Unknown",
                    RoomType = l.Room?.RoomType,
                    MonthlyRate = l.Room?.MonthlyRate ?? 0,
                    DormitoryName = l.Room?.Dormitory?.Name ?? "Unassigned",
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    Status = l.Status,
                    TotalAmountPaid = l.Payments.Where(p => p.Status == "Completed").Sum(p => p.AmountPaid),
                    PaymentCount = l.Payments.Count
                }).ToList(),
                Reservations = user.Reservations.Select(res => new ReservationResponseDto
                {
                    ReservationId = res.ReservationId,
                    UserId = res.UserId,
                    UserName = user.FullName,
                    RoomId = res.RoomId,
                    RoomNumber = res.Room?.RoomNumber ?? "Unknown",
                    RoomType = res.Room?.RoomType,
                    MonthlyRate = res.Room?.MonthlyRate ?? 0,
                    DormitoryName = res.Room?.Dormitory?.Name ?? "Unassigned",
                    ReservationDate = res.ReservationDate,
                    Status = res.Status
                }).ToList(),
                MaintenanceRequests = user.MaintenanceRequests.Select(mr => new MaintenanceRequestResponseDto
                {
                    RequestId = mr.RequestId,
                    RoomId = mr.RoomId,
                    RoomNumber = mr.Room?.RoomNumber ?? "Unknown",
                    UserId = mr.UserId,
                    RequesterName = user.FullName,
                    IssueDescription = mr.IssueDescription,
                    DateReported = mr.DateReported,
                    Status = mr.Status,
                    ResolvedAt = mr.ResolvedAt
                }).ToList()
            };

            return Ok(result);
        }

        /// <summary>
        /// Register a new user (tenant, landlord, or admin) with hashed password
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<UserResponseDto>> CreateUser([FromBody] UserCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usernameExists = await _context.Users.AnyAsync(u => u.Username.ToLower() == dto.Username.Trim().ToLower());
            if (usernameExists)
                return Conflict(new { message = $"Username '{dto.Username}' is already taken." });

            var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.Trim().ToLower());
            if (emailExists)
                return Conflict(new { message = $"Email '{dto.Email}' is already registered." });

            var user = new User
            {
                Username = dto.Username.Trim(),
                PasswordHash = PasswordHasher.HashPassword(dto.Password),
                FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim(),
                PhoneNumber = dto.PhoneNumber?.Trim(),
                Role = string.IsNullOrWhiteSpace(dto.Role) ? "Tenant" : dto.Role.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var response = new UserResponseDto
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                ActiveLeasesCount = 0
            };

            return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, response);
        }

        /// <summary>
        /// Authenticate user credentials and return profile
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<UserResponseDto>> Login([FromBody] UserLoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users
                .Include(u => u.Leases)
                .FirstOrDefaultAsync(u => u.Username.ToLower() == dto.Username.Trim().ToLower());

            if (user == null || !PasswordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            var response = new UserResponseDto
            {
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                ActiveLeasesCount = user.Leases.Count(l => l.Status == "Active")
            };

            return Ok(response);
        }

        /// <summary>
        /// Update user profile details
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found." });

            var emailConflict = await _context.Users.AnyAsync(u => u.UserId != id && u.Email.ToLower() == dto.Email.Trim().ToLower());
            if (emailConflict)
                return Conflict(new { message = $"Email '{dto.Email}' is already in use by another user." });

            user.FullName = dto.FullName.Trim();
            user.Email = dto.Email.Trim();
            user.PhoneNumber = dto.PhoneNumber?.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Role))
                user.Role = dto.Role.Trim();

            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
                user.PasswordHash = PasswordHasher.HashPassword(dto.NewPassword);

            await _context.SaveChangesAsync();

            return Ok(new { message = "User profile updated successfully." });
        }

        /// <summary>
        /// Delete user
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Leases)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound(new { message = $"User with ID {id} not found." });

            if (user.Leases.Any(l => l.Status == "Active"))
                return BadRequest(new { message = "Cannot delete user with an active lease. Terminate their lease first." });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"User '{user.FullName}' deleted successfully." });
        }
    }
}
