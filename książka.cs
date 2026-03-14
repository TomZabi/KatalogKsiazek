using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace KatalogKsiazek
{
    public class Ksiazka
    {
        public int Id { get; set; }
        public string Tytul { get; set; } = "";
        public string Autor { get; set; } = "";
        public int Rok { get; set; }
        public string Gatunek { get; set; } = "";
        public bool Przeczytana { get; set; }
        public string Uwagi { get; set; } = "";
    }
}