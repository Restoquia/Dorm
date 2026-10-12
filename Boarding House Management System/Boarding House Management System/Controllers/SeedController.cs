using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Boarding_House_Management_System.Data;
using Boarding_House_Management_System.Services;

namespace Boarding_House_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SeedController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Seed sample test data (Dormitories, Rooms, Tenants, Leases, Payments, Maintenance)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Seed()
        {
            await DbSeeder.SeedAsync(_context);
            return Ok(new
            {
                message = "Sample data seeded successfully!",
                dormitories = await _context.Dormitories.CountAsync(),
                rooms = await _context.Rooms.CountAsync(),
                users = await _context.Users.CountAsync(),
                leases = await _context.Leases.CountAsync(),
                payments = await _context.Payments.CountAsync(),
                maintenance = await _context.MaintenanceRequests.CountAsync(),
                reservations = await _context.Reservations.CountAsync()
            });
        }

        /// <summary>
        /// Reset database and reseed fresh test data
        /// </summary>
        [HttpPost("reset")]
        public async Task<IActionResult> ResetAndSeed()
        {
            // Remove child records first
            _context.Payments.RemoveRange(_context.Payments);
            _context.MaintenanceRequests.RemoveRange(_context.MaintenanceRequests);
            _context.Reservations.RemoveRange(_context.Reservations);
            _context.Leases.RemoveRange(_context.Leases);
            _context.Rooms.RemoveRange(_context.Rooms);
            _context.Users.RemoveRange(_context.Users);
            _context.Dormitories.RemoveRange(_context.Dormitories);

            await _context.SaveChangesAsync();

            await DbSeeder.SeedAsync(_context);

            return Ok(new { message = "Database reset and seeded with fresh sample data." });
        }
    }
}
