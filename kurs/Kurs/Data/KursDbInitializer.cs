using System;
using System.Data.Entity;
using System.Linq;
using Kurs.Models;

namespace Kurs.Data
{
    public class KursDbInitializer : CreateDatabaseIfNotExists<KursDbContext>
    {
        protected override void Seed(KursDbContext context)
        {
            // Добавление типов грузов
            var cargoTypes = new CargoType[]
            {
                new CargoType { Name = "Обычный груз", Coefficient = 1.0, Description = "Стандартный груз без особенностей", IsActive = true, CreatedDate = DateTime.Now },
                new CargoType { Name = "Хрупкий груз", Coefficient = 1.3, Description = "Требует особой осторожности", IsActive = true, CreatedDate = DateTime.Now },
                new CargoType { Name = "Скоропортящийся", Coefficient = 1.5, Description = "Требует специальных условий", IsActive = true, CreatedDate = DateTime.Now },
                new CargoType { Name = "Опасный груз", Coefficient = 2.0, Description = "Требует специального разрешения", IsActive = true, CreatedDate = DateTime.Now }
            };

            foreach (var cargoType in cargoTypes)
            {
                context.CargoTypes.Add(cargoType);
            }

            // Добавление тарифов
            var tariffs = new Tariff[]
            {
                new Tariff { Name = "Стандартный", CostPerKm = 50m, CostPerTon = 100m, CostPerHourDowntime = 500m, IsActive = true, CreatedDate = DateTime.Now },
                new Tariff { Name = "Эконом", CostPerKm = 40m, CostPerTon = 80m, CostPerHourDowntime = 400m, IsActive = true, CreatedDate = DateTime.Now },
                new Tariff { Name = "Бизнес", CostPerKm = 70m, CostPerTon = 150m, CostPerHourDowntime = 700m, IsActive = true, CreatedDate = DateTime.Now }
            };

            foreach (var tariff in tariffs)
            {
                context.Tariffs.Add(tariff);
            }

            // Добавление дополнительных услуг
            var services = new AdditionalService[]
            {
                new AdditionalService { Name = "Погрузка/разгрузка", Price = 2000m, Description = "Погрузо-разгрузочные работы", IsActive = true, CreatedDate = DateTime.Now },
                new AdditionalService { Name = "Экспедирование", Price = 1500m, Description = "Сопровождение груза экспедитором", IsActive = true, CreatedDate = DateTime.Now },
                new AdditionalService { Name = "Страхование", Price = 3000m, Description = "Страхование груза", IsActive = true, CreatedDate = DateTime.Now }
            };

            foreach (var service in services)
            {
                context.AdditionalServices.Add(service);
            }

            // Добавление маршрутов
            var routes = new Route[]
            {
                new Route { DeparturePoint = "Москва", DestinationPoint = "Санкт-Петербург", DistanceKm = 700, UsageCount = 0, LastUpdated = DateTime.Now },
                new Route { DeparturePoint = "Москва", DestinationPoint = "Казань", DistanceKm = 820, UsageCount = 0, LastUpdated = DateTime.Now },
                new Route { DeparturePoint = "Санкт-Петербург", DestinationPoint = "Москва", DistanceKm = 700, UsageCount = 0, LastUpdated = DateTime.Now }
            };

            foreach (var route in routes)
            {
                context.Routes.Add(route);
            }

            context.SaveChanges();
        }
    }
}