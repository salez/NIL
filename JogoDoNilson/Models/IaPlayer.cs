using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace JogoDoNilson.Models
{

    public class FEBattleMatch
    {
        public int atkCardId { get; set; }
        public int defCardId { get; set; }
    }
    public class AIPlayer
    {

        public Player Player { get; private set; }
        private Battle _battle;

        public AIPlayer(Player Player, Battle Engine)
        {
            if (!Player.IsAIControlled)
                throw new Exception("ERROR");
            this.Player = Player;
            this._battle = Engine;
        }


        public void PrepareOffense()
        {
            this.Player.DrawCard();
            _battle.EndPhase();


            //Basic just to program responses;
            var FEcreatures = (from item in PurCardsOnField()
                               select new
                               {
                                   item.Id,
                                   item.Ataque,
                                   item.Defesa,
                                   Position = "defense"
                               });


            Player.AddNotification(_battle.Phase, JsonConvert.SerializeObject(FEcreatures));

            _battle.EndPhase();

            var AtkCards = PrepareAttack();
            var Attackers = (from item in AtkCards
                             select new
                               {
                                   item.Id,
                                   item.Ataque,
                                   item.Defesa,
                                   Position = "Offense"
                               });


            _battle.Turn.SetAttackers(AtkCards);

            foreach (var item in AtkCards)
            {
                var idx = Player.DefenseField.IndexOf(item);
                if (idx != -1)
                {
                    Player.DefenseField.Remove(item);
                    Player.AtackField.Add(item);
                }

            }

            Player.AddNotification(_battle.Phase, JsonConvert.SerializeObject(Attackers));
            _battle.EndPhase();

        }

        private List<Carta> PurCardsOnField()
        {
            var CanDrawCards = this.Player.Hands.Where(x => x.Custo <= this.Player.ManaCurrent).OrderBy(x => (x.Ataque + x.Defesa)/x.Custo);
            List<Carta> DrawCards = new List<Carta>();

            for (int i = 0; i < CanDrawCards.Count(); i++)
            {
                if (CanDrawCards.Count() <= 0)
                    break;
                if (Player.ManaCurrent < CanDrawCards.Max(x => x.Custo))
                    break;

                var card = CanDrawCards.First();
                DrawCards.Add(card);
                Player.PutCardInField(card.Id);

            }
            return DrawCards;
        }

        public List<Carta> PrepareAttack()
        {
            List<Carta> Attackers = new List<Carta>();
            this.Player.DefenseField.ForEach(c =>
            {
                if (c.CanBeMoved)
                {
                    Attackers.Add(c);
                }
            });

            Attackers.AddRange(this.Player.AtackField);

            return Attackers;
        }

        public void PrepareDefense(IEnumerable<int> Attackers)
        {
            var def = new Stack<Carta>(Player.DefenseField);
            var atk = new Stack<int>(Attackers);
            List<FEBattleMatch> matchUps = new List<FEBattleMatch>();

            while (def.Count > 0 && atk.Count > 0)
            {
                matchUps.Add(new FEBattleMatch()
                {
                    atkCardId = atk.Pop(),
                    defCardId = def.Pop().Id
                });
            }
            Player.AddNotification(_battle.Phase, JsonConvert.SerializeObject(matchUps));
        }

    }
}