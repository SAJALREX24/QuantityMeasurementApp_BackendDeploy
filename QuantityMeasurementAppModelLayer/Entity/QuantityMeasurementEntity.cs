using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuantityMeasurementAppModelLayer.Entity
{
    [Table("Quantity")]
    public class QuantityMeasurementEntity
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Users")]
        public int UserId{get; set;}
        public UserEntity? Users {get; set;}

        [Required]
        public double Value1 { get; set; }

        [Required]
        public double Value2 { get; set; }

        [Required]
        public string Unit1 { get; set; } = string.Empty;

        [Required]
        public string Unit2 { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public string Operation { get; set; } = string.Empty;

        [Required]
        public double Result { get; set; }
    }
}
