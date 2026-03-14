using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

//MainWindow.xaml.cs

namespace KatalogKsiazek
{
    public partial class MainWindow : Window
    {
        List<Ksiazka> ksiazki = new List<Ksiazka>();

        public MainWindow()
        {
            InitializeComponent();

            ksiazki.Add(new Ksiazka { Id = 1, Tytul = "Hobbit", Autor = "J.R.R. Tolkien", Rok = 1937, Gatunek = "Fantasy", Przeczytana = true, Uwagi = "Podróż Bilba" });
            ksiazki.Add(new Ksiazka { Id = 2, Tytul = "Drużyna Pierścienia", Autor = "J.R.R. Tolkien", Rok = 1954, Gatunek = "Fantasy", Przeczytana = true, Uwagi = "" });
            ksiazki.Add(new Ksiazka { Id = 3, Tytul = "Dwie Wieże", Autor = "J.R.R. Tolkien", Rok = 1954, Gatunek = "Fantasy", Przeczytana = false, Uwagi = "" });
            ksiazki.Add(new Ksiazka { Id = 4, Tytul = "Powrót Króla", Autor = "J.R.R. Tolkien", Rok = 1955, Gatunek = "Fantasy", Przeczytana = false, Uwagi = "" });
            ksiazki.Add(new Ksiazka { Id = 5, Tytul = "Silmarillion", Autor = "J.R.R. Tolkien", Rok = 1977, Gatunek = "Fantasy", Przeczytana = false, Uwagi = "Historia Śródziemia" });
            ksiazki.Add(new Ksiazka { Id = 6, Tytul = "Dzieci Húrina", Autor = "J.R.R. Tolkien", Rok = 2007, Gatunek = "Fantasy", Przeczytana = false, Uwagi = "" });
            ksiazki.Add(new Ksiazka { Id = 7, Tytul = "Niedokończone opowieści", Autor = "J.R.R. Tolkien", Rok = 1980, Gatunek = "Fantasy", Przeczytana = false, Uwagi = "" });
            ListaKsiazek.ItemsSource = ksiazki;
        }

        private void DodajKsiazke_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(RokBox.Text, out int rok))
            {
                MessageBox.Show("Podaj poprawny rok");
                return;
            }

            Ksiazka nowa = new Ksiazka
            {
                Id = ksiazki.Count + 1,
                Tytul = TytulBox.Text,
                Autor = AutorBox.Text,
                Rok = rok,
                Gatunek = GatunekBox.Text,
                Przeczytana = PrzeczytanaCheck.IsChecked == true,
                Uwagi = UwagiBox.Text
            };

            ksiazki.Add(nowa);
            ListaKsiazek.Items.Refresh();

            TytulBox.Clear();
            AutorBox.Clear();
            RokBox.Clear();
            GatunekBox.Clear();
            UwagiBox.Clear();
            PrzeczytanaCheck.IsChecked = false;
        }

        private void UsunKsiazke_Click(object sender, RoutedEventArgs e)
        {
            Ksiazka wybrana = (Ksiazka)ListaKsiazek.SelectedItem;

            if (wybrana != null)
            {
                ksiazki.Remove(wybrana);
                ListaKsiazek.Items.Refresh();
            }
            TytulBox.Clear();
            AutorBox.Clear();
            RokBox.Clear();
            GatunekBox.Clear();
            UwagiBox.Clear();
            PrzeczytanaCheck.IsChecked = false;
        }

        private void ListaKsiazek_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Ksiazka wybrana = (Ksiazka)ListaKsiazek.SelectedItem;

            if (wybrana != null)
            {
                TytulBox.Text = wybrana.Tytul;
                AutorBox.Text = wybrana.Autor;
                RokBox.Text = wybrana.Rok.ToString();
                GatunekBox.Text = wybrana.Gatunek;
                PrzeczytanaCheck.IsChecked = wybrana.Przeczytana;
                UwagiBox.Text = wybrana.Uwagi;
            }
        }

        private void EdytujKsiazke_Click(object sender, RoutedEventArgs e)
        {
            Ksiazka wybrana = (Ksiazka)ListaKsiazek.SelectedItem;

            if (wybrana != null)
            {
                wybrana.Tytul = TytulBox.Text;
                wybrana.Autor = AutorBox.Text;
                wybrana.Gatunek = GatunekBox.Text;
                wybrana.Przeczytana = PrzeczytanaCheck.IsChecked == true;
                wybrana.Uwagi = UwagiBox.Text;

                if (int.TryParse(RokBox.Text, out int rok))
                    wybrana.Rok = rok;

                wybrana.Uwagi = UwagiBox.Text;

                ListaKsiazek.Items.Refresh();

                TytulBox.Clear();
                AutorBox.Clear();
                RokBox.Clear();
                GatunekBox.Clear();
                UwagiBox.Clear();
                PrzeczytanaCheck.IsChecked = false;
            }
        }

    }
}