using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Dojazdy.Models
{
    public class Rozklad
    {
        public string IdPrzystanku;
        public string Slupek;
        public Dictionary<string, List<Odjazd>> Odjazdy;
        private readonly Api api;

        public Rozklad(string IdPrzystanku, string Slupek, Api api)
        {
            this.IdPrzystanku = IdPrzystanku;
            this.Slupek = Slupek;
            this.api = api;
        }

        public async Task Wczytaj()
        {
            /*List<string> linie = await api.GetLinie(IdPrzystanku, Slupek);
            Odjazdy = (await Task.WhenAll(
                linie.Select(
                    async x => new KeyValuePair<string, List<Odjazd>>(x, await api.GetOdjazdy(IdPrzystanku, Slupek, x))
                )))
                .ToDictionary(x => x.Key, x => x.Value);*/
        }
    }
}