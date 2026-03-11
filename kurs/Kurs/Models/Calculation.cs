using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kurs.Models
{
    [Table("Calculations")]
    public class Calculation
    {
        public Calculation()
        {
            SelectedServices = new HashSet<CalculationService>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string CalculationNumber { get; set; }

        [Required]
        public DateTime CalculationDate { get; set; } = DateTime.Now;

        // Маршрут
        [Required]
        [MaxLength(200)]
        public string DeparturePoint { get; set; }

        [Required]
        [MaxLength(200)]
        public string DestinationPoint { get; set; }

        public double DistanceKm { get; set; }

        // Груз
        public int? CargoTypeId { get; set; }

        [ForeignKey("CargoTypeId")]
        public virtual CargoType CargoType { get; set; }

        public double WeightTons { get; set; }

        public double VolumeM3 { get; set; }

        // Тариф
        public int? TariffId { get; set; }

        [ForeignKey("TariffId")]
        public virtual Tariff Tariff { get; set; }

        // Срочность
        public bool IsUrgent { get; set; }

        [MaxLength(500)]
        public string SpecialConditions { get; set; }

        // Результаты расчёта
        public decimal BaseCost { get; set; }

        public double CargoCoefficient { get; set; }

        public decimal AdjustedCost { get; set; }

        public decimal ServicesCost { get; set; }

        public decimal UrgentSurcharge { get; set; }

        public decimal TotalCost { get; set; }

        // Детализация
        public string CalculationDetails { get; set; }

        // Коммерческое предложение
        [MaxLength(50)]
        public string OfferNumber { get; set; }

        public bool IsOfferGenerated { get; set; }

        // Пользователь
        [MaxLength(100)]
        public string CreatedBy { get; set; }

        // Навигационное свойство
        public virtual ICollection<CalculationService> SelectedServices { get; set; }
    }
}