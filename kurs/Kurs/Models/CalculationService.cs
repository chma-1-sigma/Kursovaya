using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kurs.Models
{
    [Table("CalculationServices")]
    public class CalculationService
    {
        [Key]
        public int Id { get; set; }

        public int CalculationId { get; set; }

        [ForeignKey("CalculationId")]
        public virtual Calculation Calculation { get; set; }

        public int ServiceId { get; set; }

        [ForeignKey("ServiceId")]
        public virtual AdditionalService Service { get; set; }

        public decimal PriceAtCalculation { get; set; }

        [MaxLength(200)]
        public string ServiceName { get; set; }
    }
}