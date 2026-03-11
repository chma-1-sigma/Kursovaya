using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Kurs.Data;
using Kurs.Models;

namespace Kurs.Views
{
    public partial class HistoryWindow : Window, INotifyPropertyChanged
    {
        private readonly KursDbContext _context;
        private ObservableCollection<Calculation> _calculations;
        private Calculation _selectedCalculation;
        private string _searchText;

        public event PropertyChangedEventHandler PropertyChanged;

        public HistoryWindow()
        {
            InitializeComponent();
            _context = new KursDbContext();
            LoadCalculations();
        }

        public ObservableCollection<Calculation> Calculations
        {
            get { return _calculations; }
            set
            {
                _calculations = value;
                OnPropertyChanged(nameof(Calculations));
            }
        }

        public Calculation SelectedCalculation
        {
            get { return _selectedCalculation; }
            set
            {
                _selectedCalculation = value;
                OnPropertyChanged(nameof(SelectedCalculation));

                // Обновляем детали
                if (value != null)
                {
                    DetailsTextBlock.Text = FormatCalculationDetails(value);
                    ViewDetailsButton.IsEnabled = true;
                    GenerateOfferButton.IsEnabled = true;
                    DeleteButton.IsEnabled = true;
                }
                else
                {
                    DetailsTextBlock.Text = "Выберите расчёт для просмотра деталей";
                    ViewDetailsButton.IsEnabled = false;
                    GenerateOfferButton.IsEnabled = false;
                    DeleteButton.IsEnabled = false;
                }
            }
        }

        private void LoadCalculations()
        {
            try
            {
                var calculations = _context.Calculations
                    .Include("CargoType")
                    .Include("Tariff")
                    .Include("SelectedServices")
                    .Include("SelectedServices.Service")
                    .OrderByDescending(c => c.CalculationDate)
                    .ToList();

                Calculations = new ObservableCollection<Calculation>(calculations);
                HistoryDataGrid.ItemsSource = Calculations;

                DetailsTextBlock.Text = "Выберите расчёт для просмотра деталей";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке истории: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterCalculations()
        {
            if (Calculations == null) return;

            if (string.IsNullOrWhiteSpace(_searchText))
            {
                HistoryDataGrid.ItemsSource = Calculations;
            }
            else
            {
                var filtered = Calculations.Where(c =>
                    c.CalculationNumber.ToLower().Contains(_searchText.ToLower()) ||
                    c.DeparturePoint.ToLower().Contains(_searchText.ToLower()) ||
                    c.DestinationPoint.ToLower().Contains(_searchText.ToLower()) ||
                    (c.CargoType != null && c.CargoType.Name.ToLower().Contains(_searchText.ToLower())) ||
                    c.CalculationDate.ToString("dd.MM.yyyy").Contains(_searchText)
                ).ToList();

                HistoryDataGrid.ItemsSource = filtered;
            }
        }

        private string FormatCalculationDetails(Calculation calc)
        {
            if (calc == null) return string.Empty;

            string services = calc.SelectedServices != null && calc.SelectedServices.Any()
                ? string.Join("\n", calc.SelectedServices.Select(s => $"  • {s.ServiceName}: {s.PriceAtCalculation:C}"))
                : "  • Нет дополнительных услуг";

            return $"=========================================\n" +
                   $"         ДЕТАЛИ РАСЧЁТА\n" +
                   $"=========================================\n" +
                   $"Номер расчёта: {calc.CalculationNumber}\n" +
                   $"Номер предложения: {calc.OfferNumber}\n" +
                   $"Дата: {calc.CalculationDate:dd.MM.yyyy HH:mm}\n" +
                   $"=========================================\n\n" +
                   $"МАРШРУТ:\n" +
                   $"  {calc.DeparturePoint} → {calc.DestinationPoint}\n" +
                   $"  Расстояние: {calc.DistanceKm:F0} км\n\n" +
                   $"ПАРАМЕТРЫ ГРУЗА:\n" +
                   $"  Тип груза: {calc.CargoType?.Name ?? "Не указан"}\n" +
                   $"  Коэффициент: {calc.CargoCoefficient:F2}\n" +
                   $"  Вес: {calc.WeightTons:F2} т\n" +
                   $"  Объём: {calc.VolumeM3:F2} м³\n\n" +
                   $"ТАРИФ: {calc.Tariff?.Name ?? "Не указан"}\n\n" +
                   $"=========================================\n" +
                   $"ФИНАНСОВЫЕ ДЕТАЛИ:\n" +
                   $"=========================================\n" +
                   $"Базовая стоимость: {calc.BaseCost:C}\n" +
                   $"С учётом коэф. груза: {calc.AdjustedCost:C}\n" +
                   $"Дополнительные услуги: {calc.ServicesCost:C}\n" +
                   (calc.IsUrgent ? $"Надбавка за срочность: {calc.UrgentSurcharge:C}\n" : "") +
                   $"=========================================\n" +
                   $"ИТОГОВАЯ СТОИМОСТЬ: {calc.TotalCost:C}\n" +
                   $"=========================================\n\n" +
                   (string.IsNullOrEmpty(calc.SpecialConditions) ? "" :
                       $"ОСОБЫЕ УСЛОВИЯ:\n{calc.SpecialConditions}\n\n") +
                   $"УСЛУГИ:\n{services}";
        }

        private void ViewDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCalculation != null)
            {
                var detailsWindow = new Window
                {
                    Title = $"Детали расчёта {SelectedCalculation.CalculationNumber}",
                    Width = 600,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    Content = new ScrollViewer
                    {
                        Padding = new Thickness(20),
                        Content = new TextBlock
                        {
                            Text = FormatCalculationDetails(SelectedCalculation),
                            TextWrapping = TextWrapping.Wrap,
                            FontSize = 13
                        }
                    }
                };
                detailsWindow.ShowDialog();
            }
        }

        private void GenerateOfferButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCalculation != null)
            {
                var offerWindow = new OfferWindow(SelectedCalculation);
                offerWindow.Owner = this;
                offerWindow.ShowDialog();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCalculation == null) return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить расчёт №{SelectedCalculation.CalculationNumber}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Удаляем связанные услуги
                    foreach (var service in SelectedCalculation.SelectedServices.ToList())
                    {
                        _context.CalculationServices.Remove(service);
                    }

                    _context.Calculations.Remove(SelectedCalculation);
                    _context.SaveChanges();

                    Calculations.Remove(SelectedCalculation);
                    SelectedCalculation = null;

                    MessageBox.Show("Расчёт успешно удалён", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadCalculations();
            SearchBox.Text = string.Empty;
        }

        private void HistoryDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedCalculation = HistoryDataGrid.SelectedItem as Calculation;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchText = SearchBox.Text;
            FilterCalculations();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            FilterCalculations();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}