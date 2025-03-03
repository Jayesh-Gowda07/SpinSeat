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
                // Existing checks
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Name == user.Name);

                if (existingUser != null)
                {
                    return existingUser;
                }

                var userCount = await _context.Users.CountAsync();
                if (userCount >= 11)
                {
                    return BadRequest("Maximum 11 users allowed. Cannot create new user.");
                }

                // Create user
                user.Id = Guid.NewGuid();
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(PostUser), new { id = user.Id }, user);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
