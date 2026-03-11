using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Kurs.Views
{
    public partial class OfferWindow : Window
    {
        private readonly Models.Calculation _calculation;

        public OfferWindow(Models.Calculation calculation)
        {
            InitializeComponent();
            _calculation = calculation;
            OfferTextBlock.Text = calculation.CalculationDetails;
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    FlowDocument document = new FlowDocument();
                    Paragraph paragraph = new Paragraph(new Run(_calculation.CalculationDetails));
                    paragraph.FontFamily = new System.Windows.Media.FontFamily("Consolas");
                    paragraph.FontSize = 12;
                    document.Blocks.Add(paragraph);

                    document.PagePadding = new Thickness(50);
                    document.ColumnWidth = printDialog.PrintableAreaWidth;

                    printDialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator,
                        $"Коммерческое предложение {_calculation.OfferNumber}");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при печати: {ex.Message}\n\nПопробуйте сохранить предложение в файл.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();
                saveDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";
                saveDialog.FileName = $"Offer_{_calculation.OfferNumber}.txt";

                if (saveDialog.ShowDialog() == true)
                {
                    System.IO.File.WriteAllText(saveDialog.FileName, _calculation.CalculationDetails);
                    MessageBox.Show("Файл успешно сохранён!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}