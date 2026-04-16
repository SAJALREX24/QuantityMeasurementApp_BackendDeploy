using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace QuantityMeasurementAppModelLayer.Entity
{
    [Table("User")]
    public class UserEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string? UserName { get; set; }
        [Required]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }
        [Required]
        public string? Phone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
