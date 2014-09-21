﻿using System;
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
            ViewBag.Message = "Modifique este modelo para iniciar rapidamente seu aplicativo ASP.NET MVC.";

            Models.JogoDoNilsonEntities db = new Models.JogoDoNilsonEntities();

            var cartas = db.jdn_cartas;

            return View(cartas);
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
    }
}
