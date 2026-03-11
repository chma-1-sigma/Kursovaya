using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Kurs.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ServicesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel viewModel)
            {
                viewModel.SelectedServices.Clear();
                foreach (var item in ((ListBox)sender).SelectedItems)
                {
                    viewModel.SelectedServices.Add((Models.AdditionalService)item);
                }
            }
        }
    }
}