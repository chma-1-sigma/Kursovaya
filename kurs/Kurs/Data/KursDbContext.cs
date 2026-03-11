using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using Kurs.Models;

namespace Kurs.Data
{
    public class KursDbContext : DbContext
    {
        public KursDbContext() : base("name=KursConnection")
        {
            // Настройка инициализатора базы данных
            Database.SetInitializer(new KursDbInitializer());

            // Включение ленивой загрузки
            Configuration.LazyLoadingEnabled = true;
            Configuration.ProxyCreationEnabled = true;
        }

        public DbSet<CargoType> CargoTypes { get; set; }
        public DbSet<Tariff> Tariffs { get; set; }
        public DbSet<AdditionalService> AdditionalServices { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<Calculation> Calculations { get; set; }
        public DbSet<CalculationService> CalculationServices { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Удаление соглашения о множественных именах
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // Настройка связей
            modelBuilder.Entity<CalculationService>()
                .HasRequired(cs => cs.Calculation)
                .WithMany(c => c.SelectedServices)
                .HasForeignKey(cs => cs.CalculationId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<CalculationService>()
                .HasRequired(cs => cs.Service)
                .WithMany()
                .HasForeignKey(cs => cs.ServiceId)
                .WillCascadeOnDelete(false);

            // Уникальный индекс для маршрутов
            modelBuilder.Entity<Route>()
                .HasIndex(r => new { r.DeparturePoint, r.DestinationPoint })
                .IsUnique();
        }
    }
}