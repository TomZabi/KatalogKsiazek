using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
    }
}