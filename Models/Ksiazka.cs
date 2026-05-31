using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace KatalogKsiazek.Models
{
    public enum StanKsiazki
    {
        Nowa,
        WTrakcie,
        Przeczytana
    }

    public class Ksiazka : INotifyPropertyChanged
    {
        private int _id;
        private string _tytul = "";
        private string _autor = "";
        private int _rok;
        private string _gatunek = "";
        private StanKsiazki _stan = StanKsiazki.Nowa;
        private int _ocena = 5;
        private string _uwagi = "";

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string Tytul
        {
            get => _tytul;
            set { _tytul = value; OnPropertyChanged(); }
        }

        public string Autor
        {
            get => _autor;
            set { _autor = value; OnPropertyChanged(); }
        }

        public int Rok
        {
            get => _rok;
            set { _rok = value; OnPropertyChanged(); }
        }

        public string Gatunek
        {
            get => _gatunek;
            set { _gatunek = value; OnPropertyChanged(); }
        }

        public StanKsiazki Stan
        {
            get => _stan;
            set { _stan = value; OnPropertyChanged(); OnPropertyChanged(nameof(StanOpis)); OnPropertyChanged(nameof(StanIkona)); }
        }

        [JsonIgnore]
        public string StanOpis => Stan switch
        {
            StanKsiazki.Nowa => "📚 Nowa",
            StanKsiazki.WTrakcie => "📖 W trakcie",
            StanKsiazki.Przeczytana => "✅ Przeczytana",
            _ => ""
        };

        [JsonIgnore]
        public string StanIkona => Stan switch
        {
            StanKsiazki.Nowa => "📚",
            StanKsiazki.WTrakcie => "📖",
            StanKsiazki.Przeczytana => "✅",
            _ => ""
        };

        public int Ocena
        {
            get => _ocena;
            set { _ocena = value; OnPropertyChanged(); OnPropertyChanged(nameof(OcenaOpis)); }
        }

        [JsonIgnore]
        public string OcenaOpis => $"{_ocena}/10";

        public string Uwagi
        {
            get => _uwagi;
            set { _uwagi = value; OnPropertyChanged(); }
        }

        private DateTime _dataDodania = DateTime.Today;
        public DateTime DataDodania
        {
            get => _dataDodania;
            set { _dataDodania = value; OnPropertyChanged(); }
        }

        private string _okladkaSciezka = "";
        public string OkladkaSciezka
        {
            get => _okladkaSciezka;
            set { _okladkaSciezka = value ?? ""; OnPropertyChanged(); }
        }

        private DateTime? _dataPrzeczytania;
        public DateTime? DataPrzeczytania
        {
            get => _dataPrzeczytania;
            set { _dataPrzeczytania = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataPrzeczytaniaOpis)); }
        }

        private string _wydawnictwo = "";
        public string Wydawnictwo
        {
            get => _wydawnictwo;
            set { _wydawnictwo = value; OnPropertyChanged(); }
        }

        private int _liczbaStron;
        public int LiczbaStron
        {
            get => _liczbaStron;
            set { _liczbaStron = value; OnPropertyChanged(); OnPropertyChanged(nameof(LiczbaStronOpis)); }
        }

        private string _isbn = "";
        public string ISBN
        {
            get => _isbn;
            set { _isbn = value; OnPropertyChanged(); }
        }

        [JsonIgnore]
        public string LiczbaStronOpis => _liczbaStron > 0 ? $"{_liczbaStron} str." : "–";

        [JsonIgnore]
        public string DataPrzeczytaniaOpis => _dataPrzeczytania.HasValue
            ? _dataPrzeczytania.Value.ToString("d", System.Globalization.CultureInfo.CurrentCulture)
            : "–";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}