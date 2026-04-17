using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using KatalogKsiazek.Helpers;
using KatalogKsiazek.Models;

namespace KatalogKsiazek.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private readonly ICollectionView _widokKsiazek;
        private readonly HashSet<string> _tkniete = new();
        private bool _resetowanieFormularza = false;

        public ObservableCollection<Ksiazka> Ksiazki { get; } = new();
        public ICollectionView WidokKsiazek => _widokKsiazek;

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
            set
            {
                if (!_resetowanieFormularza) _tkniete.Add(nameof(Tytul));
                _tytul = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BladTytul));
            }
        }

        private string _autor = "";
        public string Autor
        {
            get => _autor;
            set
            {
                if (!_resetowanieFormularza) _tkniete.Add(nameof(Autor));
                _autor = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BladAutor));
            }
        }

        private string _rokText = "";
        public string RokText
        {
            get => _rokText;
            set
            {
                if (!_resetowanieFormularza) _tkniete.Add(nameof(RokText));
                _rokText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BladRok));
            }
        }

        private string _gatunek = "";
        public string Gatunek
        {
            get => _gatunek;
            set
            {
                if (!_resetowanieFormularza) _tkniete.Add(nameof(Gatunek));
                _gatunek = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BladGatunek));
            }
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

        private string _filtr = "";
        public string Filtr
        {
            get => _filtr;
            set
            {
                _filtr = value;
                OnPropertyChanged();
                _widokKsiazek.Refresh();
            }
        }

        public string Error => string.Empty;

        public string this[string columnName] => columnName switch
        {
            nameof(Tytul)   when string.IsNullOrWhiteSpace(Tytul)   => "Tytuł jest wymagany.",
            nameof(Autor)   when string.IsNullOrWhiteSpace(Autor)   => "Autor jest wymagany.",
            nameof(RokText) when string.IsNullOrWhiteSpace(RokText) => "Rok jest wymagany.",
            nameof(RokText) when !int.TryParse(RokText, out _)      => "Rok musi być liczbą całkowitą (np. 2024).",
            nameof(RokText) when int.TryParse(RokText, out int r)
                             && (r < 1 || r > DateTime.Now.Year + 1) => $"Rok musi być z zakresu 1–{DateTime.Now.Year + 1}.",
            nameof(Gatunek) when string.IsNullOrWhiteSpace(Gatunek) => "Wybierz gatunek.",
            _ => string.Empty
        };

        public string BladTytul   => _tkniete.Contains(nameof(Tytul))   ? this[nameof(Tytul)]   : string.Empty;
        public string BladAutor   => _tkniete.Contains(nameof(Autor))   ? this[nameof(Autor)]   : string.Empty;
        public string BladRok     => _tkniete.Contains(nameof(RokText)) ? this[nameof(RokText)] : string.Empty;
        public string BladGatunek => _tkniete.Contains(nameof(Gatunek)) ? this[nameof(Gatunek)] : string.Empty;

        private bool FormularzPoprawny
        {
            get
            {
                if (!int.TryParse(RokText, out int rok)) return false;
                return !string.IsNullOrWhiteSpace(Tytul)
                    && !string.IsNullOrWhiteSpace(Autor)
                    && rok >= 1 && rok <= DateTime.Now.Year + 1
                    && !string.IsNullOrWhiteSpace(Gatunek);
            }
        }

        public ICommand DodajCommand             { get; }
        public ICommand EdytujCommand            { get; }
        public ICommand UsunCommand              { get; }
        public ICommand CzyscWyszukiwanieCommand { get; }

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

            _widokKsiazek = CollectionViewSource.GetDefaultView(Ksiazki);
            _widokKsiazek.Filter = FiltrujKsiazki;

            DodajCommand             = new RelayCommand(_ => DodajKsiazke(),   _ => FormularzPoprawny);
            EdytujCommand            = new RelayCommand(_ => EdytujKsiazke(),  _ => Wybrana != null && FormularzPoprawny);
            UsunCommand              = new RelayCommand(_ => UsunKsiazke(),    _ => Wybrana != null);
            CzyscWyszukiwanieCommand = new RelayCommand(_ => Filtr = "",       _ => !string.IsNullOrEmpty(Filtr));
        }

        private bool FiltrujKsiazki(object obj)
        {
            if (string.IsNullOrWhiteSpace(Filtr)) return true;
            if (obj is not Ksiazka k) return false;

            var f = Filtr.ToLowerInvariant();
            return k.Tytul.ToLowerInvariant().Contains(f)
                || k.Autor.ToLowerInvariant().Contains(f)
                || k.Gatunek.ToLowerInvariant().Contains(f)
                || k.StanOpis.ToLowerInvariant().Contains(f);
        }

        private void DodajKsiazke()
        {
            if (!int.TryParse(RokText, out int rok)) return;

            int noweId = Ksiazki.Count == 0 ? 1 : Ksiazki.Max(k => k.Id) + 1;
            Ksiazki.Add(new Ksiazka
            {
                Id      = noweId,
                Tytul   = Tytul,
                Autor   = Autor,
                Rok     = rok,
                Gatunek = Gatunek,
                Stan    = Stan,
                Ocena   = Ocena,
                Uwagi   = Uwagi
            });
            CzyscFormularz();
        }

        private void EdytujKsiazke()
        {
            if (Wybrana == null) return;
            if (!int.TryParse(RokText, out int rok)) return;

            Wybrana.Tytul   = Tytul;
            Wybrana.Autor   = Autor;
            Wybrana.Rok     = rok;
            Wybrana.Gatunek = Gatunek;
            Wybrana.Stan    = Stan;
            Wybrana.Ocena   = Ocena;
            Wybrana.Uwagi   = Uwagi;

            CzyscFormularz();
            Wybrana = null;
        }

        private void UsunKsiazke()
        {
            if (Wybrana == null) return;

            var wynik = MessageBox.Show(
                $"Czy na pewno chcesz usunąć książkę:\n\"{Wybrana.Tytul}\"?",
                "Potwierdzenie usunięcia",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (wynik != MessageBoxResult.Yes) return;

            Ksiazki.Remove(Wybrana);
            CzyscFormularz();
        }

        private void WczytajDoFormularza(Ksiazka k)
        {
            _resetowanieFormularza = true;
            _tkniete.Clear();

            Tytul   = k.Tytul;
            Autor   = k.Autor;
            RokText = k.Rok.ToString();
            Gatunek = k.Gatunek;
            Stan    = k.Stan;
            Ocena   = k.Ocena;
            Uwagi   = k.Uwagi;

            NotyfikujBledy();
            _resetowanieFormularza = false;
        }

        private void CzyscFormularz()
        {
            _resetowanieFormularza = true;
            _tkniete.Clear();

            Tytul   = "";
            Autor   = "";
            RokText = "";
            Gatunek = "";
            Stan    = StanKsiazki.Nowa;
            Ocena   = 5;
            Uwagi   = "";

            NotyfikujBledy();
            _resetowanieFormularza = false;
        }

        private void NotyfikujBledy()
        {
            OnPropertyChanged(nameof(BladTytul));
            OnPropertyChanged(nameof(BladAutor));
            OnPropertyChanged(nameof(BladRok));
            OnPropertyChanged(nameof(BladGatunek));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}