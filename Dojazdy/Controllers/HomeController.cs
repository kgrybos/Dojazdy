using Dojazdy.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Dojazdy.Controllers
{
    public class HomeController : Controller
    {

        public async Task<ActionResult> Index()
        {
            using (Baza db = new Baza())
            {
                return View(new Przystanki(db.SelectPrzystanki()));
            }
        }

        [HttpPost]
        public async Task<ActionResult> GetRozklad(Przystanek p)
        {
            using (Baza db = new Baza())
            {
                int idPrzystanku = db.GetPrzystanekId(p);
                if(db.doAktualizacji(p))
                {
                    Api api = new Api();
                    db.InsertOdjazdy(await api.GetOdjazdy(p), idPrzystanku);
                }
                string json = JsonConvert.SerializeObject(db.SelectOdjazdy(idPrzystanku), new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.Write(json);
                Response.End();
                return View();
            }
        }

        public async Task UpdatePrzystanki()
        {
            using(Baza db = new Baza())
            {
                db.DeletePrzystanki();
                Api api = new Api();
                foreach(Przystanek p in await api.GetPrzystanki())
                {
                    db.InsertPrzystanek(p);
                }
            }
        }

        public async Task<ActionResult> Pojazdy()
        {
            //string json = await api.GetPojazdy();
            Response.Clear();
            Response.ContentType = "application/json; charset=utf-8";
            //Response.Write(json);
            Response.End();
            return View();
        }
    }
}