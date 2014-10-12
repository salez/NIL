using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JogoDoNilson.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //ViewBag.Message = "Modifique este modelo para iniciar rapidamente seu aplicativo ASP.NET MVC.";

            //Models.JogoDoNilsonEntities db = new Models.JogoDoNilsonEntities();

            //var cartas = db.jdn_cartas;

            return View();
        }

        public ActionResult ChooseAvatar()
        {
            return View("Characters");
        }

        [HttpPost]
        public ActionResult StartGame(string CharacterId)
        {
            Models.GameEngine Engine = new Models.GameEngine(Session);
            Engine.StartGame(string.Format("~/Images/Cartas/{0}.jpg", Convert.ToInt32(CharacterId) < 10 ? "0" + CharacterId.ToString() : CharacterId.ToString()));
            return RedirectToAction("Battle", "Battle");
        }

        public ActionResult About()
        {
            ViewBag.Message = "A página de descrição do aplicativo.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Sua página de contato.";

            return View();
        }


        //public ActionResult PreviewDeck(string P)
        //{
        //    //Models.GameEngine Engine = new Models.GameEngine(Session);
        //    //Engine.StartGame();
        //    //if(P == "2")
        //    //    return View("Index", Engine.PlayerTwo.Deck.AllCards());

        //    //return View("Index", Engine.PlayerOne.Deck.AllCards());

        //}
    }
}
