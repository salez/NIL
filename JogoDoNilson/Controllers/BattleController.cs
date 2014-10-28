using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JogoDoNilson.Models;

namespace JogoDoNilson.Controllers
{
    public class BattleController : Controller
    {
        //
        // GET: /Battle/

        public ActionResult battle()
        {
            GameEngine engine = new GameEngine(Session);

            engine.Battle.StartBattle(engine);

            return View(engine);
        }

        public int drawCard()
        {
            return 0; 
        }

    }
}
