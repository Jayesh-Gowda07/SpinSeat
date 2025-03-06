using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using spinApp.Data;
using spinApp.Models;
using Microsoft.EntityFrameworkCore;

namespace spinApp.Controllers
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

        [HttpPost]
        public async Task<ActionResult<User>> PostUser([FromBody] User user)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                // Normalize input name for case-insensitive comparison
                var normalizedName = user.Name.Trim().ToUpperInvariant();

                // Check for existing user with case-insensitive comparison
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => EF.Functions.ILike(u.Name, normalizedName));

                if (existingUser != null)
                {
                    return existingUser;
                }

                // Enforce 11-user limit with transaction-level lock
                var userCount = await _context.Users.CountAsync();
                if (userCount >= 11)
                {
                    return BadRequest("Maximum 11 users allowed. Cannot create new user.");
                }

                // Create new user with original case preservation
                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    Name = user.Name.Trim() // Preserve original case
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(PostUser), new { id = newUser.Id }, newUser);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
