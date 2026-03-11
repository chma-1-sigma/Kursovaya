using System.Windows;
using Kurs.Data;
using System.Data.Entity;

namespace Kurs
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Инициализация базы данных
            Database.SetInitializer(new KursDbInitializer());

            // Создание базы данных при первом запуске
            using (var context = new KursDbContext())
            {
                context.Database.CreateIfNotExists();
            }
        }
    }
}