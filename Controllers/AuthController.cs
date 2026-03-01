using ImageProcessing.Data;
using ImageProcessing.DTOs;
using ImageProcessing.Entities;
using ImageProcessing.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ImageProcessing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        //private static readonly Dictionary<string, string> Users = new();
        private readonly ApplicationDbContext _context;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(ApplicationDbContext context, IJwtTokenService jwtTokenService)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("AuthController is working");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new AuthResponse
                {
                    Message = "Username and password are required."
                });
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (existingUser != null)
            {
                return BadRequest(new AuthResponse
                {
                    Message = "User already exists."
                });
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                PasswordHash = passwordHash,
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _jwtTokenService.GenerateToken(user.Username, user.Role);

            return Ok(new AuthResponse
            {
                Message = "User registered successfully.",
                Token = token
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null ||
                !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new AuthResponse
                {
                    Message = "Invalid username or password."
                });
            }

            var token = _jwtTokenService.GenerateToken(user.Username, user.Role);

            return Ok(new AuthResponse
            {
                Message = "User logged in successfully.",
                Token = token
            });
        }
    }
}
