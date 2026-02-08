using ImageProcessing.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ImageProcessing.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private static readonly Dictionary<string, string> Users = new();

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("AuthController is working");
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterRequest request)
        {
            if (Users.ContainsKey(request.Username))
            {
                return BadRequest("User already exists");
            }

            Users[request.Username] = request.Password;

            return Ok(new AuthResponse
            {
                Message = "User registered successfully"
            });
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            if (!Users.ContainsKey(request.Username))
            {
                return Unauthorized(new AuthResponse
                {
                    Message = "User not found"
                });
            }

            if (Users[request.Username] != request.Password)
            {
                return Unauthorized(new AuthResponse
                {
                    Message = "Invalid username or password"
                });
            }

            return Ok(new AuthResponse
            {
                Message = "User logged in successfully!"
            });
        }
    }
}
