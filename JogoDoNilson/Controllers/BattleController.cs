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

            //gambi pra passar turno
            Player player = battle.Turn.Player;

            if (player.IsAIControlled)
                battle.EndTurn();

            DrawCard();

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

        public int DrawCard()
        {
            GameEngine engine = new GameEngine(Session);

            Battle battle = engine.Battle;
            Player player = battle.Turn.Player;

            if (player.IsAIControlled)
                return 0;

            if (battle.Phase != BattlePhase.Draw)
                return 0;

            player.DrawCard();
            battle.EndPhase();

            return 1; 
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

            var cards = player.ChooseAttackers(cardIds);

            battle.Turn.SetAttackers(cards);
            battle.EndPhase();

            return 1;
        }

        public int ChooseDefenders(int atkCardId, int[]defCardIds)
        {
            GameEngine engine = new GameEngine(Session);

            Battle battle = engine.Battle;
            Player defPlayer = (battle.Turn.Player == battle.player1)?battle.player2:battle.player1;

            if (defPlayer.IsAIControlled || defCardIds.Count() > 2)
                return 0;

            if (battle.Phase != BattlePhase.Defense)
                return 0;

            //todo: deffend cards
            Carta atkCard = battle.Turn.Atackers.First(x => x.Id == atkCardId);

            List<Carta> defCards = defPlayer.ChooseDefenders(defCardIds);

            BattleFight battleFight = new BattleFight(atkCard, defCards);

            //todo: battleResult

            return 1;
        }
    }
}
