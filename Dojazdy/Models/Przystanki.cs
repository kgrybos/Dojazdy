﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dojazdy.Models
{
    public class Przystanki
    {
        public List<Przystanek> Lista;

        public Przystanki(List<Przystanek> nowy)
        {
            Lista = nowy;
        }
    }
}