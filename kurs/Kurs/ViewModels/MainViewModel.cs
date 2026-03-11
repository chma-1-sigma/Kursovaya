using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Data.Entity;
using System.Windows;
using Kurs.Data;
using Kurs.Models;
using Kurs.Services;

namespace Kurs.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly KursDbContext _context;
        private readonly CalculationEngine _calculationEngine;
        private readonly DistanceService _distanceService;

        // Команды
        public ICommand CalculateCommand { get; }
        public ICommand SaveCalculationCommand { get; }
        public ICommand GenerateOfferCommand { get; }
        public ICommand ClearFormCommand { get; }
        public ICommand OpenHistoryCommand { get; }

        // Свойства для ввода
        private string _departurePoint;
        public string DeparturePoint
        {
            get => _departurePoint;
            set => SetProperty(ref _departurePoint, value);
        }

        private string _destinationPoint;
        public string DestinationPoint
        {
            get => _destinationPoint;
            set => SetProperty(ref _destinationPoint, value);
        }

        private CargoType _selectedCargoType;
        public CargoType SelectedCargoType
        {
            get => _selectedCargoType;
            set => SetProperty(ref _selectedCargoType, value);
        }

        private Tariff _selectedTariff;
        public Tariff SelectedTariff
        {
            get => _selectedTariff;
            set => SetProperty(ref _selectedTariff, value);
        }

        private double _weightTons;
        public double WeightTons
        {
            get => _weightTons;
            set => SetProperty(ref _weightTons, value);
        }

        private double _volumeM3;
        public double VolumeM3
        {
            get => _volumeM3;
            set => SetProperty(ref _volumeM3, value);
        }

        private bool _isUrgent;
        public bool IsUrgent
        {
            get => _isUrgent;
            set => SetProperty(ref _isUrgent, value);
        }

        private string _specialConditions;
        public string SpecialConditions
        {
            get => _specialConditions;
            set => SetProperty(ref _specialConditions, value);
        }

        // Результат расчёта
        private Calculation _currentCalculation;
        public Calculation CurrentCalculation
        {
            get => _currentCalculation;
            set => SetProperty(ref _currentCalculation, value);
        }

        // Статус
        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        // Коллекции для справочников
        public ObservableCollection<CargoType> CargoTypes { get; set; }
        public ObservableCollection<Tariff> Tariffs { get; set; }
        public ObservableCollection<AdditionalService> AdditionalServices { get; set; }
        public ObservableCollection<AdditionalService> SelectedServices { get; set; }

        public MainViewModel()
        {
            try
            {
                _context = new KursDbContext();
                _calculationEngine = new CalculationEngine();
                _distanceService = new DistanceService(_context);

                // Загрузка справочников
                LoadReferenceData();

                // Инициализация команд
                CalculateCommand = new RelayCommand(Calculate, CanCalculate);
                SaveCalculationCommand = new RelayCommand(SaveCalculation, () => CurrentCalculation != null);
                GenerateOfferCommand = new RelayCommand(GenerateOffer, () => CurrentCalculation != null);
                ClearFormCommand = new RelayCommand(ClearForm);
                OpenHistoryCommand = new RelayCommand(OpenHistory);

                // Инициализация коллекций
                SelectedServices = new ObservableCollection<AdditionalService>();

                StatusMessage = "Готов к работе";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка инициализации: {ex.Message}";
                MessageBox.Show($"Ошибка при запуске программы:\n{ex.Message}\n\n" +
                    "Приложение будет закрыто.", "Критическая ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                Application.Current.Shutdown();
            }
        }

        private void LoadReferenceData()
        {
            try
            {
                CargoTypes = new ObservableCollection<CargoType>(
                    _context.CargoTypes.Where(ct => ct.IsActive).ToList());

                Tariffs = new ObservableCollection<Tariff>(
                    _context.Tariffs.Where(t => t.IsActive).ToList());

                AdditionalServices = new ObservableCollection<AdditionalService>(
                    _context.AdditionalServices.Where(s => s.IsActive).ToList());

                OnPropertyChanged(nameof(CargoTypes));
                OnPropertyChanged(nameof(Tariffs));
                OnPropertyChanged(nameof(AdditionalServices));
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки справочников: {ex.Message}";
            }
        }

        private bool CanCalculate()
        {
            return !string.IsNullOrWhiteSpace(DeparturePoint) &&
                   !string.IsNullOrWhiteSpace(DestinationPoint) &&
                   SelectedCargoType != null &&
                   SelectedTariff != null &&
                   WeightTons > 0;
        }

        private void Calculate()
        {
            try
            {
                StatusMessage = "Выполняется расчёт...";

                // Получаем или рассчитываем расстояние
                var route = _distanceService.GetOrCalculateDistance(DeparturePoint, DestinationPoint);

                // Выполняем расчёт
                CurrentCalculation = _calculationEngine.CalculateCost(
                    route,
                    SelectedCargoType,
                    SelectedTariff,
                    WeightTons,
                    IsUrgent,
                    SelectedServices.ToList()
                );

                CurrentCalculation.VolumeM3 = VolumeM3;
                CurrentCalculation.SpecialConditions = SpecialConditions;
                CurrentCalculation.CreatedBy = Environment.UserName;

                OnPropertyChanged(nameof(CurrentCalculation));
                StatusMessage = $"Расчёт выполнен успешно. Итоговая стоимость: {CurrentCalculation.TotalCost:C}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при расчёте: {ex.Message}";
                MessageBox.Show($"Ошибка при расчёте: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveCalculation()
        {
            try
            {
                if (CurrentCalculation != null)
                {
                    // Проверяем, не сохранён ли уже расчёт
                    if (CurrentCalculation.Id == 0)
                    {
                        // Сохраняем выбранные услуги
                        foreach (var service in SelectedServices)
                        {
                            CurrentCalculation.SelectedServices.Add(new CalculationService
                            {
                                Service = service,
                                ServiceId = service.Id,
                                PriceAtCalculation = service.Price,
                                ServiceName = service.Name
                            });
                        }

                        _context.Calculations.Add(CurrentCalculation);
                        _context.SaveChanges();

                        StatusMessage = $"Расчёт сохранён в истории. Номер: {CurrentCalculation.CalculationNumber}";

                        MessageBox.Show($"Расчёт успешно сохранён в истории!\nНомер расчёта: {CurrentCalculation.CalculationNumber}",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Этот расчёт уже сохранён в истории.", "Информация",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при сохранении: {ex.Message}";
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateOffer()
        {
            if (CurrentCalculation != null)
            {
                CurrentCalculation.IsOfferGenerated = true;

                var offerWindow = new Views.OfferWindow(CurrentCalculation);
                offerWindow.Owner = Application.Current.MainWindow;
                offerWindow.ShowDialog();
            }
        }

        private void ClearForm()
        {
            DeparturePoint = string.Empty;
            DestinationPoint = string.Empty;
            SelectedCargoType = null;
            SelectedTariff = null;
            WeightTons = 0;
            VolumeM3 = 0;
            IsUrgent = false;
            SpecialConditions = string.Empty;
            SelectedServices.Clear();
            CurrentCalculation = null;

            StatusMessage = "Форма очищена";
        }

        private void OpenHistory()
        {
            var historyWindow = new Views.HistoryWindow();
            historyWindow.Owner = Application.Current.MainWindow;
            historyWindow.ShowDialog();
        }
    }
}