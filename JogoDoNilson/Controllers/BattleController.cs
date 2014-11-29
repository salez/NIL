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

        //Todo Init Game Engine and Battle on Controller Constructor

        public ActionResult Battle()
        {
            GameEngine engine = new GameEngine(Session);

            var battle = engine.StartBattle();

            //gambi pra passar turno
            Player player = battle.Turn.Player;

            if (player.IsAIControlled)
            {
                player.DrawCard();
                battle.EndTurn();
            }

            return View(engine);
        }

        public int ComputerAction()
        {
            GameEngine engine = new GameEngine(Session);

            Battle battle = engine.Battle;
            Player player = battle.Turn.Player;

            if (!player.IsAIControlled)
                return 0;

            switch (engine.Battle.Phase)
            {
                case BattlePhase.Draw:

                    battle.Turn.Player.DrawCard();

                    battle.EndPhase();

                    break;
                case BattlePhase.Main:

                    //todo: computer actions

                    break;
                case BattlePhase.Attack:

                    //todo: computer actions

                    break;
                case BattlePhase.Defense:

                    //todo: computer actions

                    break;
                case BattlePhase.End:

                    //todo: computer actions

                    break;
                default:
                    break;
            }

            return 1;
        }

        public ActionResult DrawCard()
        {
            GameEngine engine = new GameEngine(Session);

            Battle battle = engine.Battle;
            Player player = battle.Turn.Player;

            if (player.IsAIControlled)
                return Content("0");

            if (battle.Phase != BattlePhase.Draw)
                return Content("0");

            Carta card = player.DrawCard();
            battle.EndPhase();
            ViewBag.Left = ((player.Hands.Count - 1) * 150) + 200;
            return PartialView("card", card);
        }

        public int PutCardInField(int cardId)
        {
            GameEngine engine = new GameEngine(Session);

            Battle battle = engine.Battle;
            Player player = battle.Turn.Player;

            if (player.IsAIControlled)
                return 0;

            if (battle.Phase != BattlePhase.Main)
                return 0;

            var result = player.PutCardInField(cardId);

            if (result.Status == EnumResult.Error)
                return 0;

            return 1;
        }

        public int MoveCardToAtackField(int cardId)
        {
            GameEngine engine = new GameEngine(Session);

            Battle battle = engine.Battle;
            Player player = battle.Turn.Player;

            if (player.IsAIControlled)
                return 0;

            if (battle.Phase != BattlePhase.Main)
                return 0;

            var result = player.MoveCardToAtackField(cardId);

            if (result.Status == EnumResult.Error)
                return 0;

            return 1;
        }

        public int MoveCardToDefenseField(int cardId)
        {
            GameEngine engine = new GameEngine(Session);

            Battle battle = engine.Battle;
            Player player = battle.Turn.Player;

            if (player.IsAIControlled)
                return 0;

            if (battle.Phase != BattlePhase.Main)
                return 0;

            var result = player.MoveCardToDefenseField(cardId);

            if (result.Status == EnumResult.Error)
                return 0;

            return 1;
        }

        public int ChooseAttackers(int[] cardIds)
        {
            GameEngine engine = new GameEngine(Session);

            Battle battle = engine.Battle;
            Player player = battle.Turn.Player;

            if (player.IsAIControlled)
                return 0;

            if (battle.Phase == BattlePhase.Main)
                battle.EndPhase();

            if (battle.Phase != BattlePhase.Attack)
                return 0;
            if (cardIds != null)
            {
                var cards = player.ChooseAttackers(cardIds);

                battle.Turn.SetAttackers(cards);
                battle.EndPhase();
                var ai = new AIPlayer(battle.player1, battle);
                ai.PrepareDefense(cardIds);
            }
            else
            {
                battle.EndTurn();
            }

            return 1;
        }


        //Original Returns Int
        public JsonResult ChooseDefenders(int atkCardId, int[] defCardIds)
        {
            GameEngine engine = new GameEngine(Session);

            Battle battle = engine.Battle;
            Player atkPlayer = battle.Turn.Player;
            Player defPlayer = (battle.Turn.Player == battle.player1) ? battle.player2 : battle.player1;


            if (defCardIds.Count() > 2)
                return null;

            if (battle.Phase != BattlePhase.Defense)
                return null;

            //todo: deffend cards
            Carta atkCard = battle.Turn.Atackers.First(x => x.Id == atkCardId);

            if (defCardIds[0] == 0)
            {
                DirectHit(atkCardId);
                return Json(new
                {
                    atkId = atkCard.Id,
                    atkIsDead = atkCard.IsDead,
                    atkLife = atkCard.Defesa,
                    defIds = 0,
                    defIsDead = true,
                    defLife = 0
                });
            }

            List<Carta> defCards = defPlayer.ChooseDefenders(defCardIds);

            BattleFight battleFight = new BattleFight(atkCard, defCards);
            var result = battleFight.Result;

            if (atkCard.IsDead)
            {
                atkPlayer.Graveyard.Add(atkCard);
                atkPlayer.AtackField.Remove(atkCard);
            }

            foreach (var defCard in defCards)
            {
                if (defCard.IsDead)
                {
                    defPlayer.Graveyard.Add(defCard);
                    defPlayer.DefenseField.Remove(defCard);
                }
            }
            defPlayer.Life -= result.PlayerLifeDamage;
            //todo: battleResult


            return Json(new
            {
                atkId = result.Atacker.Id,
                atkIsDead = result.Atacker.IsDead,
                atkLife = result.Atacker.Defesa,
                defIds = result.Defender.Select(x => x.Id),
                defIsDead = result.Defender.Select(x => x.IsDead),
                defLife = result.Defender.Select(x => x.Defesa)
            });
        }
        public void DirectHit(int atkCardId)
        {
            GameEngine engine = new GameEngine(Session);

            Battle battle = engine.Battle;
            Player atkPlayer = battle.Turn.Player;
            Player defPlayer = (battle.Turn.Player == battle.player1) ? battle.player2 : battle.player1;

            Carta atkCard = battle.Turn.Atackers.First(x => x.Id == atkCardId);

            defPlayer.Life -= atkCard.Ataque;

        }

        public JsonResult GetPlayerLifes()
        {
            GameEngine engine = new GameEngine(Session);


            return Json(new
            {
                computer = engine.PlayerOne.Life,
                player = engine.PlayerTwo.Life
            });
        }

        public JsonResult GetPlayerMana()
        {

            GameEngine engine = new GameEngine(Session);


            return Json(new
            {
                computer = engine.PlayerOne.ManaCurrent,
                player = engine.PlayerTwo.ManaCurrent
            });
        }

        public void EndTurn()
        {
            GameEngine g = new GameEngine(Session);
            g.Battle.EndTurn();
        }

        public JsonResult VerifyPhase()
        {
            GameEngine engine = new GameEngine(Session);

            Battle battle = engine.Battle;
            Player player = battle.Turn.Player;

            if (player.IsAIControlled)
            {
                var notification = engine.PlayerOne.RetrieveFirstNotification();
                var result = Json(new
                {
                    phase = notification.Key,
                    isYourTurn = player == engine.PlayerTwo,
                    data = notification.Value
                });


                return result;
            }
            else if (!player.IsAIControlled && battle.Phase == BattlePhase.Defense)
            {
                var notification = engine.PlayerOne.RetrieveFirstNotification(battle.Phase);
                var result = Json(new
                {
                    phase = notification.Key,
                    isYourTurn = player == engine.PlayerTwo,
                    data = notification.Value
                });
                return result;
            }
            else
            {
                return Json(new
                {
                    phase = battle.Phase,
                    isYourTurn = battle.Turn.Player == engine.PlayerTwo,
                    data = ""
                });
            }
        }
    }
}
