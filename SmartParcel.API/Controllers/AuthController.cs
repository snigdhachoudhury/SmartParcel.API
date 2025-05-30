using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SmartParcel.API.Data;
using SmartParcel.API.DTOs;
using SmartParcel.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims; // Added for ClaimTypes
using System.Text;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using System.Linq; // Added for LINQ extension methods
using System.Threading.Tasks; // Added for Task
using Microsoft.EntityFrameworkCore; // Added for async EF Core methods

namespace SmartParcel.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly PasswordHasher<User> _hasher = new();

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request) // Added [FromBody] and async
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Use AnyAsync for asynchronous database check
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("User already exists.");

            var user = new User
            {
                Email = request.Email,
                // Ensure the role is consistently TitleCase before saving
                Role = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(request.Role.ToLowerInvariant())
            };

            user.PasswordHash = _hasher.HashPassword(user, request.Password);

            await _context.Users.AddAsync(user); // Use AddAsync
            await _context.SaveChangesAsync(); // Use SaveChangesAsync

            return Ok(new { Message = "User registered successfully.", Email = request.Email });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request) // Added [FromBody] and async
        {
            // Basic validation, consider using ModelState.IsValid if LoginRequest has data annotations
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Email and password are required.");

            // Use FirstOrDefaultAsync for asynchronous database query
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                return Unauthorized("Invalid credentials.");

            var passwordMatch = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (passwordMatch == PasswordVerificationResult.Failed)
                return Unauthorized("Incorrect password.");

            // Ensure the role is consistently TitleCase before generating the token,
            // in case it was stored incorrectly or retrieved with different casing.
            user.Role = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(user.Role.ToLowerInvariant());

            var token = GenerateJwtToken(user); // This method can remain synchronous if it doesn't do I/O
            return Ok(new { token });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new InvalidOperationException("JWT key is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                // FIX: Use the actual user's role from the database, ensuring it's TitleCase
                new Claim(ClaimTypes.Role, user.Role ?? string.Empty)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}