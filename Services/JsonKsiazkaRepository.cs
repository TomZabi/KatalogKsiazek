using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using KatalogKsiazek.Models;

namespace KatalogKsiazek.Services
{
    public class JsonKsiazkaRepository : IKsiazkaRepository
    {
        private static readonly string _sciezka = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "wpf",
            "ksiazki.json");

        private static readonly JsonSerializerOptions _opcje = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() }
        };

        public IList<Ksiazka> Wczytaj()
        {
            if (!File.Exists(_sciezka))
                return new List<Ksiazka>();

            try
            {
                string json = File.ReadAllText(_sciezka, System.Text.Encoding.UTF8);
                return JsonSerializer.Deserialize<List<Ksiazka>>(json, _opcje)
                       ?? new List<Ksiazka>();
            }
            catch
            {
                return new List<Ksiazka>();
            }
        }

        public void Zapisz(IEnumerable<Ksiazka> ksiazki)
        {
            string? katalog = Path.GetDirectoryName(_sciezka);
            if (katalog != null)
                Directory.CreateDirectory(katalog);

            string json = JsonSerializer.Serialize(ksiazki, _opcje);
            File.WriteAllText(_sciezka, json, System.Text.Encoding.UTF8);
        }
    }
}
