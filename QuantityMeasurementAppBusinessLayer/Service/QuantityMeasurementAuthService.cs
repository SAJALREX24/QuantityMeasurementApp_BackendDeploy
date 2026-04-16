using QuantityMeasurementAppBusinessLayer.Interface;
using QuantityMeasurementAppModelLayer.DTOs;
using QuantityMeasurementAppBusinessLayer.Exception;
using QuantityMeasurementAppRepositoryLayer.Interface;
using QuantityMeasurementAppModelLayer.Entity;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace QuantityMeasurementAppBusinessLayer.Service
{
    public class QuantityMeasurementAuthService : IAuthService
    {
        private readonly IQuantityMeasurementRepository _repo;
        private readonly IConfiguration _config;

        public QuantityMeasurementAuthService(IQuantityMeasurementRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }

        public string Register(RegisterDTO user)
        {
            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Email) ||
                string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(user.Phone))
            {
                throw new QuantityMeasurementException("Username, Email, Password and Phone cannot be empty.");
            }

            var newUser = new UserEntity
            {
                UserName = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                Password = BCrypt.Net.BCrypt.HashPassword(user.Password),
                CreatedAt = DateTime.UtcNow
            };

            _repo.Register(newUser);
            return "success: user registered";
        }

        public UserEntity? Login(LoginDTO user)
        {
            var existingUser = _repo.GetUserbyEmail(user.Email!);
            if (existingUser != null && BCrypt.Net.BCrypt.Verify(user.Password, existingUser.Password))
                return existingUser;
            return null;
        }

        public UserEntity? GetUserById(int id)
        {
            return _repo.GetUserById(id);
        }

        public async Task<UserEntity?> LoginWithGoogle(string idToken)
        {
            var clientId = _config["Google:ClientId"];

            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            };

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            }
            catch
            {
                return null;
            }

            // Check if user exists
            var existingUser = _repo.GetUserbyEmail(payload.Email);
            if (existingUser != null) return existingUser;

            // Auto-create account on first Google login
            var newUser = new UserEntity
            {
                UserName = payload.Name ?? payload.Email,
                Email = payload.Email,
                Phone = "",
                Password = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), // random password for OAuth users
                CreatedAt = DateTime.UtcNow
            };

            _repo.Register(newUser);
            return _repo.GetUserbyEmail(payload.Email);
        }

        public string GenerateJwtToken(UserEntity user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("THIS_IS_A_SUPER_SECRET_KEY_1234567890"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "QuantityMeasurementApp.Api",
                audience: "QuantityMeasurementApp.Api",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
