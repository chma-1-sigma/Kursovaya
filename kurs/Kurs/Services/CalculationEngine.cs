using System;
using System.Linq;
using System.Collections.Generic;
using Kurs.Models;

namespace Kurs.Services
{
    public class CalculationEngine
    {
        private const decimal URGENT_SURCHARGE_PERCENT = 0.20m; // 20% за срочность

        public Calculation CalculateCost(
            Route route,
            CargoType cargoType,
            Tariff tariff,
            double weightTons,
            bool isUrgent,
            List<AdditionalService> selectedServices)
        {
            var calculation = new Calculation
            {
                CalculationNumber = GenerateCalculationNumber(),
                CalculationDate = DateTime.Now,
                DeparturePoint = route.DeparturePoint,
                DestinationPoint = route.DestinationPoint,
                DistanceKm = route.DistanceKm,
                CargoType = cargoType,
                CargoTypeId = cargoType.Id,
                WeightTons = weightTons,
                Tariff = tariff,
                TariffId = tariff.Id,
                IsUrgent = isUrgent,
                CargoCoefficient = cargoType.Coefficient
            };

            try
            {
                // Расчёт базовой стоимости
                calculation.BaseCost = (decimal)route.DistanceKm * tariff.CostPerKm +
                                      (decimal)weightTons * tariff.CostPerTon;

                // Корректировка на тип груза
                calculation.AdjustedCost = calculation.BaseCost * (decimal)calculation.CargoCoefficient;

                // Стоимость дополнительных услуг
                calculation.ServicesCost = selectedServices?.Sum(s => s.Price) ?? 0;

                // Надбавка за срочность
                if (isUrgent)
                {
                    calculation.UrgentSurcharge = calculation.AdjustedCost * URGENT_SURCHARGE_PERCENT;
                }

                // Итоговая стоимость
                calculation.TotalCost = calculation.AdjustedCost +
                                       calculation.ServicesCost +
                                       calculation.UrgentSurcharge;

                // Формирование детализации
                calculation.CalculationDetails = GenerateDetails(calculation, route, selectedServices);

                // Генерация номера предложения
                calculation.OfferNumber = GenerateOfferNumber();

                return calculation;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при расчёте стоимости: {ex.Message}", ex);
            }
        }

        private string GenerateDetails(Calculation calc, Route route, List<AdditionalService> services)
        {
            var details = $"=========================================\n" +
                         $"     КОММЕРЧЕСКОЕ ПРЕДЛОЖЕНИЕ\n" +
                         $"=========================================\n" +
                         $"Номер расчёта: {calc.CalculationNumber}\n" +
                         $"Дата: {DateTime.Now:dd.MM.yyyy HH:mm}\n" +
                         $"=========================================\n\n" +
                         $"МАРШРУТ:\n" +
                         $"  {route.DeparturePoint} → {route.DestinationPoint}\n" +
                         $"  Расстояние: {route.DistanceKm:F0} км\n\n" +
                         $"ПАРАМЕТРЫ ГРУЗА:\n" +
                         $"  Тип груза: {calc.CargoType?.Name}\n" +
                         $"  Коэффициент: {calc.CargoCoefficient:F2}\n" +
                         $"  Вес: {calc.WeightTons:F2} т\n\n" +
                         $"ТАРИФ: {calc.Tariff?.Name}\n\n" +
                         $"=========================================\n" +
                         $"ДЕТАЛИЗАЦИЯ СТОИМОСТИ:\n" +
                         $"=========================================\n" +
                         $"БАЗОВАЯ СТОИМОСТЬ:\n" +
                         $"  За расстояние: {calc.Tariff?.CostPerKm:C} × {route.DistanceKm:F0} км = {((decimal)route.DistanceKm * calc.Tariff.CostPerKm):C}\n" +
                         $"  За вес: {calc.Tariff?.CostPerTon:C} × {calc.WeightTons:F2} т = {((decimal)calc.WeightTons * calc.Tariff.CostPerTon):C}\n" +
                         $"  Итого базовая: {calc.BaseCost:C}\n\n" +
                         $"КОРРЕКТИРОВКА НА ТИП ГРУЗА:\n" +
                         $"  Коэффициент: {calc.CargoCoefficient:F2}\n" +
                         $"  Стоимость с учётом коэф.: {calc.AdjustedCost:C}\n";

            if (calc.IsUrgent)
            {
                details += $"\nСРОЧНОСТЬ (20%):\n" +
                          $"  Надбавка: {calc.UrgentSurcharge:C}\n";
            }

            if (services != null && services.Any())
            {
                details += $"\nДОПОЛНИТЕЛЬНЫЕ УСЛУГИ:\n";
                foreach (var service in services)
                {
                    details += $"  {service.Name}: {service.Price:C}\n";
                }
                details += $"  Всего услуги: {calc.ServicesCost:C}\n";
            }

            details += $"\n=========================================\n" +
                      $"ИТОГОВАЯ СТОИМОСТЬ: {calc.TotalCost:C}\n" +
                      $"=========================================\n" +
                      $"Номер предложения: {calc.OfferNumber}\n";

            return details;
        }

        private string GenerateCalculationNumber()
        {
            return $"CALC-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        }

        private string GenerateOfferNumber()
        {
            return $"OFFER-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
        }
    }
}