using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using KatalogKsiazek.Helpers;
using KatalogKsiazek.Models;

namespace KatalogKsiazek.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Ksiazka> Ksiazki { get; } = new();

        public ObservableCollection<string> Gatunki { get; } = new()
        {
            "Fantasy", "Sci-Fi", "Kryminał", "Romans", "Horror",
            "Biografia", "Historia", "Popularnonaukowa", "Thriller", "Inne"
        };

        private Ksiazka? _wybrana;
        public Ksiazka? Wybrana
        {
            get => _wybrana;
            set
            {
                _wybrana = value;
                OnPropertyChanged();
                if (value != null)
                    WczytajDoFormularza(value);
                else
                    CzyscFormularz();
            }
        }

        private string _tytul = "";
        public string Tytul
        {
            get => _tytul;
            set { _tytul = value; OnPropertyChanged(); }
        }

        private string _autor = "";
        public string Autor
        {
            get => _autor;
            set { _autor = value; OnPropertyChanged(); }
        }

        private string _rokText = "";
        public string RokText
        {
            get => _rokText;
            set { _rokText = value; OnPropertyChanged(); }
        }

        private string _gatunek = "";
        public string Gatunek
        {
            get => _gatunek;
            set { _gatunek = value; OnPropertyChanged(); }
        }

        private StanKsiazki _stan = StanKsiazki.Nowa;
        public StanKsiazki Stan
        {
            get => _stan;
            set
            {
                _stan = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StanNowa));
                OnPropertyChanged(nameof(StanWTrakcie));
                OnPropertyChanged(nameof(StanPrzeczytana));
            }
        }

        public bool StanNowa
        {
            get => Stan == StanKsiazki.Nowa;
            set { if (value) Stan = StanKsiazki.Nowa; }
        }
        public bool StanWTrakcie
        {
            get => Stan == StanKsiazki.WTrakcie;
            set { if (value) Stan = StanKsiazki.WTrakcie; }
        }
        public bool StanPrzeczytana
        {
            get => Stan == StanKsiazki.Przeczytana;
            set { if (value) Stan = StanKsiazki.Przeczytana; }
        }

        private int _ocena = 5;
        public int Ocena
        {
            get => _ocena;
            set { _ocena = value; OnPropertyChanged(); OnPropertyChanged(nameof(OcenaOpis)); }
        }
        public string OcenaOpis => $"{_ocena}/10";

        private string _uwagi = "";
        public string Uwagi
        {
            get => _uwagi;
            set { _uwagi = value; OnPropertyChanged(); }
        }

        public ICommand DodajCommand { get; }
        public ICommand EdytujCommand { get; }
        public ICommand UsunCommand { get; }

        public MainViewModel()
        {
            var dane = new[]
            {
                new Ksiazka { Id=1, Tytul="Hobbit",                  Autor="J.R.R. Tolkien", Rok=1937, Gatunek="Fantasy", Stan=StanKsiazki.Przeczytana, Ocena=9,  Uwagi="Podróż Bilba" },
                new Ksiazka { Id=2, Tytul="Drużyna Pierścienia",     Autor="J.R.R. Tolkien", Rok=1954, Gatunek="Fantasy", Stan=StanKsiazki.Przeczytana, Ocena=10, Uwagi="" },
                new Ksiazka { Id=3, Tytul="Dwie Wieże",              Autor="J.R.R. Tolkien", Rok=1954, Gatunek="Fantasy", Stan=StanKsiazki.WTrakcie,    Ocena=8,  Uwagi="" },
                new Ksiazka { Id=4, Tytul="Powrót Króla",            Autor="J.R.R. Tolkien", Rok=1955, Gatunek="Fantasy", Stan=StanKsiazki.Nowa,        Ocena=5,  Uwagi="" },
                new Ksiazka { Id=5, Tytul="Silmarillion",            Autor="J.R.R. Tolkien", Rok=1977, Gatunek="Fantasy", Stan=StanKsiazki.Nowa,        Ocena=5,  Uwagi="Historia Śródziemia" },
                new Ksiazka { Id=6, Tytul="Dzieci Húrina",           Autor="J.R.R. Tolkien", Rok=2007, Gatunek="Fantasy", Stan=StanKsiazki.Nowa,        Ocena=5,  Uwagi="" },
                new Ksiazka { Id=7, Tytul="Niedokończone opowieści", Autor="J.R.R. Tolkien", Rok=1980, Gatunek="Fantasy", Stan=StanKsiazki.Nowa,        Ocena=5,  Uwagi="" },
            };
            foreach (var k in dane)
                Ksiazki.Add(k);

            DodajCommand = new RelayCommand(_ => DodajKsiazke(), _ => MozeDodac());
            EdytujCommand = new RelayCommand(_ => EdytujKsiazke(), _ => MozeEdytowac());
            UsunCommand = new RelayCommand(_ => UsunKsiazke(), _ => MozeUsunac());
        }

        private void DodajKsiazke()
        {
            if (!int.TryParse(RokText, out int rok))
            {
                MessageBox.Show("Podaj poprawny rok.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int noweId = Ksiazki.Count == 0 ? 1 : Ksiazki.Max(k => k.Id) + 1;

            Ksiazki.Add(new Ksiazka
            {
                Id = noweId,
                Tytul = Tytul,
                Autor = Autor,
                Rok = rok,
                Gatunek = Gatunek,
                Stan = Stan,
                Ocena = Ocena,
                Uwagi = Uwagi
            });

            CzyscFormularz();
        }

        private void EdytujKsiazke()
        {
            if (Wybrana == null) return;

            if (!int.TryParse(RokText, out int rok))
            {
                MessageBox.Show("Podaj poprawny rok.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Wybrana.Tytul = Tytul;
            Wybrana.Autor = Autor;
            Wybrana.Rok = rok;
            Wybrana.Gatunek = Gatunek;
            Wybrana.Stan = Stan;
            Wybrana.Ocena = Ocena;
            Wybrana.Uwagi = Uwagi;

            CzyscFormularz();
            Wybrana = null;
        }

        private void UsunKsiazke()
        {
            if (Wybrana == null) return;
            Ksiazki.Remove(Wybrana);
            CzyscFormularz();
        }

        private bool MozeDodac() => !string.IsNullOrWhiteSpace(Tytul);
        private bool MozeEdytowac() => Wybrana != null;
        private bool MozeUsunac() => Wybrana != null;

        private void WczytajDoFormularza(Ksiazka k)
        {
            Tytul = k.Tytul;
            Autor = k.Autor;
            RokText = k.Rok.ToString();
            Gatunek = k.Gatunek;
            Stan = k.Stan;
            Ocena = k.Ocena;
            Uwagi = k.Uwagi;
        }

        private void CzyscFormularz()
        {
            Tytul = "";
            Autor = "";
            RokText = "";
            Gatunek = "";
            Stan = StanKsiazki.Nowa;
            Ocena = 5;
            Uwagi = "";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}