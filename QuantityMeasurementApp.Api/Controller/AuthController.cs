using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuantityMeasurementAppModelLayer.DTOs;
using QuantityMeasurementAppBusinessLayer.Interface;
using QuantityMeasurementAppModelLayer.Entity;
using System.Security.Claims;

namespace QuantityMeasurementApp.Api.Controller
{
    [Route("/api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        // POST /api/auth/register
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDTO register)
        {
            string result = _auth.Register(register);
            return Ok(new { message = result });
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO login)
        {
            var user = _auth.Login(login);
            if (user == null) return BadRequest(new { message = "Invalid credentials" });
            string token = _auth.GenerateJwtToken(user);
            return Ok(new { message = "Success: Login", user.UserName, token });
        }

        // POST /api/auth/google
        // Body: { "idToken": "<google-id-token-from-frontend>" }
        // Returns the same JWT as login so client can use it for all secured endpoints
        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.IdToken))
                return BadRequest(new { message = "idToken is required" });

            var user = await _auth.LoginWithGoogle(dto.IdToken);
            if (user == null) return Unauthorized(new { message = "Invalid Google token" });

            string token = _auth.GenerateJwtToken(user);

            // Returns idToken (the original Google one) + our JWT token
            return Ok(new
            {
                message = "Success: Google Login",
                user.UserName,
                user.Email,
                idToken = dto.IdToken,   // original Google ID token
                token                    // our JWT for subsequent API calls
            });
        }

        // GET /api/auth/me   [Authorized]
        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (idClaim == null) return Unauthorized();

            int userId = int.Parse(idClaim);
            var user = _auth.GetUserById(userId);
            if (user == null) return NotFound(new { message = "User not found" });

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.Phone,
                user.CreatedAt
            });
        }
    }
}
