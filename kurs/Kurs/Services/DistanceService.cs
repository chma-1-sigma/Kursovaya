using System;
using System.Linq;
using Kurs.Data;
using Kurs.Models;

namespace Kurs.Services
{
    public class DistanceService
    {
        private readonly KursDbContext _context;

        public DistanceService(KursDbContext context)
        {
            _context = context;
        }

        public Route GetOrCalculateDistance(string departure, string destination)
        {
            if (string.IsNullOrWhiteSpace(departure) || string.IsNullOrWhiteSpace(destination))
                throw new ArgumentException("Пункты отправления и назначения должны быть указаны");

            // Нормализация строк
            departure = departure.Trim();
            destination = destination.Trim();

            // Ищем в базе данных
            var existingRoute = _context.Routes
                .FirstOrDefault(r => r.DeparturePoint.ToLower() == departure.ToLower()
                                  && r.DestinationPoint.ToLower() == destination.ToLower());

            if (existingRoute != null)
            {
                existingRoute.UsageCount++;
                existingRoute.LastUpdated = DateTime.Now;
                _context.SaveChanges();

                return existingRoute;
            }

            // Если не нашли, рассчитываем приблизительно
            double distance = CalculateApproximateDistance(departure, destination);

            var newRoute = new Route
            {
                DeparturePoint = departure,
                DestinationPoint = destination,
                DistanceKm = distance,
                LastUpdated = DateTime.Now,
                UsageCount = 1
            };

            _context.Routes.Add(newRoute);
            _context.SaveChanges();

            return newRoute;
        }

        private double CalculateApproximateDistance(string departure, string destination)
        {
            // Демо-расчёт для курсовой работы
            var random = new Random((departure.Length + destination.Length) * 100);

            // Словарь известных расстояний между городами РФ
            var knownRoutes = new (string from, string to, double distance)[]
            {
                ("Москва", "Санкт-Петербург", 700),
                ("Москва", "Казань", 820),
                ("Москва", "Нижний Новгород", 420),
                ("Санкт-Петербург", "Москва", 700)
            };

            // Проверяем, есть ли в известных маршрутах
            foreach (var route in knownRoutes)
            {
                if (route.from.Equals(departure, StringComparison.OrdinalIgnoreCase) &&
                    route.to.Equals(destination, StringComparison.OrdinalIgnoreCase))
                {
                    return route.distance;
                }
            }

            // Если нет в базе, генерируем случайное расстояние
            return random.Next(50, 2000);
        }
    }
}