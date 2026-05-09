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
using KatalogKsiazek.Services;

namespace KatalogKsiazek.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private readonly IKsiazkaRepository _repository;
        private readonly ICollectionView _widokKsiazek;
        private readonly HashSet<string> _tkniete = new();
        private bool _resetowanieFormularza = false;
        private bool _formularzZmodyfikowany = false;
        private bool _trybNowa = false;
        private Ksiazka? _tymczasowaKsiazka;

        public ObservableCollection<Ksiazka> Ksiazki { get; } = new();
        public ICollectionView WidokKsiazek => _widokKsiazek;

        public ObservableCollection<string> Gatunki { get; } = new()
        {
            "Fantasy", "Sci-Fi", "Kryminał", "Romans", "Horror",
            "Biografia", "Historia", "Popularnonaukowa", "Thriller", "Inne"
        };

        public IReadOnlyList<string> GatunkiFiltr { get; } = new[]
        {
            "(wszystkie)", "Fantasy", "Sci-Fi", "Kryminał", "Romans", "Horror",
            "Biografia", "Historia", "Popularnonaukowa", "Thriller", "Inne"
        };

        public IReadOnlyList<string> StanyFiltr { get; } = new[]
        {
            "(wszystkie)", "Nowa", "W trakcie", "Przeczytana"
        };

        private Ksiazka? _wybrana;
        public Ksiazka? Wybrana
        {
            get => _wybrana;
            set
            {
                if (ReferenceEquals(_wybrana, value)) return;

                // Jeśli formularz ma niezapisane zmiany – zapytaj przed opuszczeniem
                if (_formularzZmodyfikowany && _wybrana != null)
                {
                    var wynik = MessageBox.Show(
                        "Masz niezapisane zmiany.\nCzy na pewno chcesz wyjść bez zapisywania?",
                        "Niezapisane zmiany",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (wynik != MessageBoxResult.Yes)
                    {
                        // Cofnij zaznaczenie w ListBox z powrotem do starego
                        OnPropertyChanged();
                        return;
                    }
                }

                // Switching away from unsaved new book – remove the placeholder
                if (_trybNowa && _tymczasowaKsiazka != null && !ReferenceEquals(value, _tymczasowaKsiazka))
                {
                    var temp = _tymczasowaKsiazka;
                    _trybNowa = false;
                    _tymczasowaKsiazka = null;
                    Ksiazki.Remove(temp);
                }

                _wybrana = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MaWybrana));
                OnPropertyChanged(nameof(JestNowaKsiazka));

                if (value != null)
                    WczytajDoFormularza(value);
                else
                    CzyscFormularz();
            }
        }

        public bool MaWybrana => _wybrana != null;
        public bool JestNowaKsiazka => _trybNowa;

        // ── Pola formularza ──────────────────────────────────────────────

        private string _tytul = "";
        public string Tytul
        {
            get => _tytul;
            set
            {
                if (!_resetowanieFormularza) { _tkniete.Add(nameof(Tytul)); _formularzZmodyfikowany = true; }
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
                if (!_resetowanieFormularza) { _tkniete.Add(nameof(Autor)); _formularzZmodyfikowany = true; }
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
                if (!_resetowanieFormularza) { _tkniete.Add(nameof(RokText)); _formularzZmodyfikowany = true; }
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
                if (!_resetowanieFormularza) { _tkniete.Add(nameof(Gatunek)); _formularzZmodyfikowany = true; }
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

        public bool StanNowa        { get => Stan == StanKsiazki.Nowa;        set { if (value) { if (!_resetowanieFormularza) _formularzZmodyfikowany = true; Stan = StanKsiazki.Nowa; } } }
        public bool StanWTrakcie    { get => Stan == StanKsiazki.WTrakcie;    set { if (value) { if (!_resetowanieFormularza) _formularzZmodyfikowany = true; Stan = StanKsiazki.WTrakcie; } } }
        public bool StanPrzeczytana { get => Stan == StanKsiazki.Przeczytana; set { if (value) { if (!_resetowanieFormularza) _formularzZmodyfikowany = true; Stan = StanKsiazki.Przeczytana; } } }

        private int _ocena = 5;
        public int Ocena
        {
            get => _ocena;
            set { if (!_resetowanieFormularza) _formularzZmodyfikowany = true; _ocena = value; OnPropertyChanged(); OnPropertyChanged(nameof(OcenaOpis)); }
        }
        public string OcenaOpis => $"{_ocena}/10";

        private string _uwagi = "";
        public string Uwagi
        {
            get => _uwagi;
            set { if (!_resetowanieFormularza) _formularzZmodyfikowany = true; _uwagi = value; OnPropertyChanged(); }
        }

        // ── Filtry ───────────────────────────────────────────────────────

        private string _filtr = "";
        public string Filtr
        {
            get => _filtr;
            set { _filtr = value; OnPropertyChanged(); _widokKsiazek.Refresh(); }
        }

        private string _filtrGatunek = "(wszystkie)";
        public string FiltrGatunek
        {
            get => _filtrGatunek;
            set { _filtrGatunek = value ?? "(wszystkie)"; OnPropertyChanged(); _widokKsiazek.Refresh(); }
        }

        private string _filtrStan = "(wszystkie)";
        public string FiltrStan
        {
            get => _filtrStan;
            set { _filtrStan = value ?? "(wszystkie)"; OnPropertyChanged(); _widokKsiazek.Refresh(); }
        }

        private string _filtrRok = "";
        public string FiltrRok
        {
            get => _filtrRok;
            set { _filtrRok = value; OnPropertyChanged(); _widokKsiazek.Refresh(); }
        }

        // ── Walidacja ────────────────────────────────────────────────────

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

        // ── Komendy ──────────────────────────────────────────────────────

        public ICommand NowaKsiazkaCommand       { get; }
        public ICommand ZapiszCommand            { get; }
        public ICommand AnulujCommand            { get; }
        public ICommand UsunCommand              { get; }
        public ICommand CzyscWyszukiwanieCommand { get; }
        public ICommand CzyscFiltryCommand       { get; }

        public MainViewModel() : this(new JsonKsiazkaRepository()) { }

        public MainViewModel(IKsiazkaRepository repository)
        {
            _repository = repository;

            var zapisane = _repository.Wczytaj();

            if (zapisane.Count > 0)
            {
                foreach (var k in zapisane)
                    Ksiazki.Add(k);
            }
            else
            {
                // Dane przykładowe przy pierwszym uruchomieniu
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
                _repository.Zapisz(Ksiazki);
            }

            _widokKsiazek = CollectionViewSource.GetDefaultView(Ksiazki);
            _widokKsiazek.Filter = FiltrujKsiazki;

            NowaKsiazkaCommand       = new RelayCommand(_ => DodajNowaKsiazke());
            ZapiszCommand            = new RelayCommand(_ => ZapiszKsiazke(),  _ => MaWybrana && FormularzPoprawny);
            AnulujCommand            = new RelayCommand(_ => AnulujEdycje(),   _ => MaWybrana);
            UsunCommand              = new RelayCommand(_ => UsunKsiazke(),    _ => MaWybrana && !_trybNowa);
            CzyscWyszukiwanieCommand = new RelayCommand(_ => Filtr = "",       _ => !string.IsNullOrEmpty(Filtr));
            CzyscFiltryCommand       = new RelayCommand(_ => CzyscFiltry(),
                _ => FiltrGatunek != "(wszystkie)" || FiltrStan != "(wszystkie)"
                     || !string.IsNullOrEmpty(FiltrRok) || !string.IsNullOrEmpty(Filtr));
        }

        // ── Filtrowanie ──────────────────────────────────────────────────

        private bool FiltrujKsiazki(object obj)
        {
            if (obj is not Ksiazka k) return false;

            if (!string.IsNullOrWhiteSpace(Filtr))
            {
                var f = Filtr.ToLowerInvariant();
                if (!k.Tytul.ToLowerInvariant().Contains(f) && !k.Autor.ToLowerInvariant().Contains(f))
                    return false;
            }

            if (FiltrGatunek != "(wszystkie)" && k.Gatunek != FiltrGatunek)
                return false;

            if (FiltrStan != "(wszystkie)")
            {
                bool pasuje = FiltrStan switch
                {
                    "Nowa"       => k.Stan == StanKsiazki.Nowa,
                    "W trakcie"  => k.Stan == StanKsiazki.WTrakcie,
                    "Przeczytana"=> k.Stan == StanKsiazki.Przeczytana,
                    _            => true
                };
                if (!pasuje) return false;
            }

            if (!string.IsNullOrWhiteSpace(FiltrRok)
                && int.TryParse(FiltrRok, out int rok)
                && k.Rok != rok)
                return false;

            return true;
        }

        // ── Akcje ────────────────────────────────────────────────────────

        private void DodajNowaKsiazke()
        {
            // Jeśli jest już niezapisana tymczasowa – usuń ją bez pytania
            if (_trybNowa && _tymczasowaKsiazka != null)
            {
                Ksiazki.Remove(_tymczasowaKsiazka);
                _tymczasowaKsiazka = null;
                _trybNowa = false;
            }

            int noweId = Ksiazki.Count == 0 ? 1 : Ksiazki.Max(k => k.Id) + 1;
            _tymczasowaKsiazka = new Ksiazka
            {
                Id      = noweId,
                Tytul   = "",
                Autor   = "",
                Rok     = DateTime.Now.Year,
                Gatunek = "",
                Stan    = StanKsiazki.Nowa,
                Ocena   = 5,
                Uwagi   = ""
            };
            _trybNowa = true;
            Ksiazki.Add(_tymczasowaKsiazka);

            // Wyczyść formularz ręcznie (omijamy setter Wybrana, by nie wywołać WczytajDoFormularza)
            _resetowanieFormularza = true;
            _tkniete.Clear();
            Tytul   = "";
            Autor   = "";
            RokText = DateTime.Now.Year.ToString();
            Gatunek = "";
            Stan    = StanKsiazki.Nowa;
            Ocena   = 5;
            Uwagi   = "";
            _resetowanieFormularza = false;
            NotyfikujBledy();

            _wybrana = _tymczasowaKsiazka;
            OnPropertyChanged(nameof(Wybrana));
            OnPropertyChanged(nameof(MaWybrana));
            OnPropertyChanged(nameof(JestNowaKsiazka));
        }

        private void ZapiszKsiazke()
        {
            if (_wybrana == null) return;
            if (!int.TryParse(RokText, out int rok)) return;

            // Wymuś pokazanie wszystkich błędów walidacji
            foreach (var pole in new[] { nameof(Tytul), nameof(Autor), nameof(RokText), nameof(Gatunek) })
                _tkniete.Add(pole);
            NotyfikujBledy();
            if (!FormularzPoprawny) return;

            var potwierdzenie = MessageBox.Show(
                $"Czy na pewno chcesz zapisać zmiany dla:\n\"{Tytul}\"?",
                "Potwierdzenie zapisu",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (potwierdzenie != MessageBoxResult.Yes) return;

            _wybrana.Tytul   = Tytul;
            _wybrana.Autor   = Autor;
            _wybrana.Rok     = rok;
            _wybrana.Gatunek = Gatunek;
            _wybrana.Stan    = Stan;
            _wybrana.Ocena   = Ocena;
            _wybrana.Uwagi   = Uwagi;

            _formularzZmodyfikowany = false;
            _trybNowa = false;
            _tymczasowaKsiazka = null;
            OnPropertyChanged(nameof(JestNowaKsiazka));
            _repository.Zapisz(Ksiazki);
        }

        private void AnulujEdycje()
        {
            if (_wybrana == null) return;

            string komunikat = _trybNowa
                ? "Czy na pewno chcesz odrzucić nową książkę? Dane zostaną utracone."
                : "Czy na pewno chcesz anulować edycję? Niezapisane zmiany zostaną utracone.";

            var wynik = MessageBox.Show(
                komunikat,
                "Potwierdzenie anulowania",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (wynik != MessageBoxResult.Yes) return;

            if (_trybNowa && _tymczasowaKsiazka != null)
            {
                var temp = _tymczasowaKsiazka;
                _trybNowa = false;
                _tymczasowaKsiazka = null;
                _wybrana = null;
                OnPropertyChanged(nameof(Wybrana));
                OnPropertyChanged(nameof(MaWybrana));
                OnPropertyChanged(nameof(JestNowaKsiazka));
                Ksiazki.Remove(temp);
                CzyscFormularz();
            }
            else
            {
                // Przywróć oryginalne wartości z obiektu książki
                WczytajDoFormularza(_wybrana);
            }
        }

        private void UsunKsiazke()
        {
            if (_wybrana == null) return;

            var wynik = MessageBox.Show(
                $"Czy na pewno chcesz usunąć książkę:\n\"{_wybrana.Tytul}\"?",
                "Potwierdzenie usunięcia",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (wynik != MessageBoxResult.Yes) return;

            var doUsuniecia = _wybrana;
            _wybrana = null;
            OnPropertyChanged(nameof(Wybrana));
            OnPropertyChanged(nameof(MaWybrana));
            Ksiazki.Remove(doUsuniecia);
            CzyscFormularz();
            _repository.Zapisz(Ksiazki);
        }

        private void CzyscFiltry()
        {
            Filtr        = "";
            FiltrGatunek = "(wszystkie)";
            FiltrStan    = "(wszystkie)";
            FiltrRok     = "";
        }

        private void WczytajDoFormularza(Ksiazka k)
        {
            _resetowanieFormularza = true;
            _tkniete.Clear();

            Tytul   = k.Tytul;
            Autor   = k.Autor;
            RokText = k.Rok > 0 ? k.Rok.ToString() : "";
            Gatunek = k.Gatunek;
            Stan    = k.Stan;
            Ocena   = k.Ocena;
            Uwagi   = k.Uwagi;

            NotyfikujBledy();
            _formularzZmodyfikowany = false;
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
            _formularzZmodyfikowany = false;
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