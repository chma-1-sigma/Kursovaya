using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kurs.Models
{
    [Table("Routes")]
    public class Route
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string DeparturePoint { get; set; }

        [Required]
        [MaxLength(200)]
        public string DestinationPoint { get; set; }

        [Required]
        public double DistanceKm { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public int UsageCount { get; set; } = 0;
    }
}