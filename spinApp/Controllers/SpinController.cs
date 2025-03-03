using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using spinApp.Data;
using spinApp.Models;

namespace spinApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpinController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SpinController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("check/{userId}")]
        public async Task<ActionResult<bool>> CheckSpin(Guid userId)
        {
            var today = DateTime.UtcNow.Date;
            return await _context.DailyNumbers
                .AnyAsync(dn => dn.UserId == userId && dn.Date == today);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> Spin(Guid userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return NotFound("User not found");

                var today = DateTime.UtcNow.Date;
                if (await _context.DailyNumbers.AnyAsync(dn => dn.UserId == userId && dn.Date == today))
                    return Conflict("Already spun today");

                var usedNumbers = await _context.DailyNumbers
                    .Where(dn => dn.Date == today)
                    .Select(dn => dn.Number)
                    .ToListAsync();

                var availableNumbers = Enumerable.Range(1, 11).Except(usedNumbers).ToList();
                if (availableNumbers.Count == 0) return BadRequest("No numbers left");

                var random = new Random();
                var selectedNumber = availableNumbers[random.Next(availableNumbers.Count)];

                _context.DailyNumbers.Add(new DailyNumber
                {
                    Date = today,
                    Number = selectedNumber,
                    UserId = userId
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(selectedNumber);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}

