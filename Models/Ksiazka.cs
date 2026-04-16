using System.ComponentModel;
using System.Runtime.CompilerServices;

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
            set { _stan = value; OnPropertyChanged(); OnPropertyChanged(nameof(StanOpis)); }
        }

        public string StanOpis => Stan switch
        {
            StanKsiazki.Nowa => "📚 Nowa",
            StanKsiazki.WTrakcie => "📖 W trakcie",
            StanKsiazki.Przeczytana => "✅ Przeczytana",
            _ => ""
        };

        public int Ocena
        {
            get => _ocena;
            set { _ocena = value; OnPropertyChanged(); OnPropertyChanged(nameof(OcenaOpis)); }
        }

        public string OcenaOpis => $"{_ocena}/10";

        public string Uwagi
        {
            get => _uwagi;
            set { _uwagi = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}