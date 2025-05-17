
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minimal_API_Project.Domain.Entities
{
    public class Vehicle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(7)]
        public string Plate { get; set; } = default!;
        [Required]
        [StringLength(39)]
        public string Color { get; set; } = default!;
        [Required]
        [StringLength(50)]
        public string Brand { get; set; } = default!;
        [Required]
        [StringLength(50)]
        public string Model { get; set; } = default!;
        [Required]
        public int Year { get; set; } = default;
    }
}
