using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using KatalogKsiazek.ViewModels;

namespace KatalogKsiazek.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Kliknięcie w puste miejsce listy – odznacza wybraną książkę
        private void ListaKsiazek_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var listBox = (ListBox)sender;
            var element = e.OriginalSource as DependencyObject;

            while (element != null && !ReferenceEquals(element, listBox))
            {
                if (element is ListBoxItem) return; // trafiono w element – nie odznaczaj
                element = VisualTreeHelper.GetParent(element);
            }

            listBox.SelectedItem = null;
        }

        // Zamykanie okna z niezapisanymi zmianami
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (DataContext is MainViewModel vm && vm.FormularzZmodyfikowany)
            {
                var wynik = MessageBox.Show(
                    "Masz niezapisane zmiany. Czy na pewno chcesz zamknąć aplikację?",
                    "Niezapisane zmiany",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (wynik == MessageBoxResult.No)
                    e.Cancel = true;
            }
        }
    }
}