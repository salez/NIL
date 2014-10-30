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

        public ActionResult Battle()
        {
            GameEngine engine = new GameEngine(Session);

            var battle = engine.StartBattle();

            return View(engine);
        }

        public int ComputerAction()
        {
            return 0;
        }

        public int DrawCard()
        {
            return 0; 
        }

        public int PutCardInField()
        {
            return 0;
        }

        public int MoveCardToAtackField()
        {
            return 0;
        }

        public int MoveCardToDefenseField()
        {
            return 0;
        }

        public int ChooseAtackers()
        {
            return 0;
        }

        public int ChooseDefenders()
        {
            return 0;
        }

    }
}
