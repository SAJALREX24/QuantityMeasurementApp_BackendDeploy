using System.ComponentModel.DataAnnotations;

namespace QuantityMeasurementAppModelLayer.DTOs
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Username is required.")]
        public string? Username { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }

        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        public string? Phone { get; set; }
    }
}
