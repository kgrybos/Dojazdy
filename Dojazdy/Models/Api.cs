using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Globalization;

namespace Dojazdy.Models
{
    public class Api
    {
        private readonly HttpClient http = new HttpClient();

        public async Task<string> GetPojazdy()
        {
            string url = "https://api.um.warszawa.pl/api/action/busestrams_get/?resource_id=f2e5503e-927d-4ad3-9500-4ab9e55deb59&apikey=3ce8d61c-5650-42dd-bf86-7a0b34b9faa6&type=1";
            HttpResponseMessage response = await http.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<List<Przystanek>> GetPrzystanki()
        {
            string url = "https://api.um.warszawa.pl/api/action/dbstore_get/?id=ab75c33d-3a26-4342-b36a-6e5fef0a3ac3&apikey=3ce8d61c-5650-42dd-bf86-7a0b34b9faa6";
            HttpResponseMessage response = await http.GetAsync(url);
            dynamic content = await response.Content.ReadAsStringAsync();
            return ((JObject)JObject.Parse(content))
                .SelectToken("result")
                .Children()
                .Select(x => Rozpakuj(x))
                .Select(x =>
                {
                    string Lat = x["szer_geo"];
                    string Lon = x["dlug_geo"];
                    return new Przystanek
                    {
                        Id = x["zespol"],
                        Slupek = x["slupek"],
                        Nazwa = x["nazwa_zespolu"],
                        IdUlicy = x["id_ulicy"],
                        Lat = Lat != "null" ? double.Parse(Lat, CultureInfo.InvariantCulture) : 0,
                        Lon = Lon != "null" ? double.Parse(Lon, CultureInfo.InvariantCulture) : 0,
                        Kierunek = x["kierunek"],
                        ObowiazujeOd = DateTime.Parse(x["obowiazuje_od"]),
                    };
                })
                .ToList();
        }

        public async Task<List<string>> GetLinie(Przystanek p)
        {
            string url = $"https://api.um.warszawa.pl/api/action/dbtimetable_get/?id=88cd555f-6f31-43ca-9de4-66c479ad5942&busstopId={p.Id}&busstopNr={p.Slupek}&apikey=3ce8d61c-5650-42dd-bf86-7a0b34b9faa6";
            HttpResponseMessage response = await http.GetAsync(url);
            dynamic content = await response.Content.ReadAsStringAsync();
            return ((JObject)JObject.Parse(content))
                .SelectToken("result")
                .Children()
                .Select(x => Rozpakuj(x))
                .Select(x => x["linia"])
                .ToList();
        }

        public async Task<List<Odjazd>[]> GetOdjazdy(Przystanek p)
        {
            return await Task.WhenAll(
                (await GetLinie(p)).Select(async linia =>
                {
                    string url = $"https://api.um.warszawa.pl/api/action/dbtimetable_get/?id=e923fa0e-d96c-43f9-ae6e-60518c9f3238&busstopId={p.Id}&busstopNr={p.Slupek}&line={linia}&apikey=3ce8d61c-5650-42dd-bf86-7a0b34b9faa6";
                    HttpResponseMessage response = await http.GetAsync(url);
                    dynamic content = await response.Content.ReadAsStringAsync();
                    return ((JObject)JObject.Parse(content))
                        .SelectToken("result")
                        .Children()
                        .Select(x => Rozpakuj(x))
                        .Select(x => NaprawCzas(x))
                        .Select(x => new Odjazd
                        {
                            Brygada = x["brygada"],
                            Kierunek = x["kierunek"],
                            Trasa = x["trasa"],
                            Czas = DateTime.Parse(x["czas"]),
                            Linia = linia
                        })
                        .ToList();
                }
            ));
        }

        private Dictionary<string, string> Rozpakuj(JToken wejscie)
        {
            return wejscie
                .SelectToken("values")
                .Children()
                .ToDictionary(x => x.Value<string>("key"), x => x.Value<string>("value"));
        }

        private Dictionary<string, string> NaprawCzas(Dictionary<string, string> wejscie)
        {
            string[] ciagi = wejscie["czas"].Split(':');
            int godzina = int.Parse(ciagi[0]);
            if (godzina > 23)
            {
                wejscie["czas"] = (godzina % 24).ToString() + ":" + ciagi[1] + ":" + ciagi[2];
                return wejscie;
            }
            else
            {
                return wejscie;
            }
        }
    }
}