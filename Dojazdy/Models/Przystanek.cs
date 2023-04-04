using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;

namespace Dojazdy.Models
{
    public class Przystanek
    {
        public string Id { get; set; }
        public string Slupek { get; set; }
        public string Nazwa;
        public string IdUlicy;
        public double Lat;
        public double Lon;
        public string Kierunek;
        public DateTime ObowiazujeOd;
    }
}