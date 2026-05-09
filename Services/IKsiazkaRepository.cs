using System.Collections.Generic;
using KatalogKsiazek.Models;

namespace KatalogKsiazek.Services
{
    public interface IKsiazkaRepository
    {
        IList<Ksiazka> Wczytaj();
        void Zapisz(IEnumerable<Ksiazka> ksiazki);
    }
}
