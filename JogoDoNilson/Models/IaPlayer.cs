using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace JogoDoNilson.Models
{
    public class AIPlayer
    {

        public Player Player { get; private set; }
        private Battle _battle;

        public AIPlayer(Player Player, Battle Engine)
        {
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

            //var Attackers = PrepareAttack();
            //new JogoDoNilson.Controllers.BattleController().ChooseAttackers(Attackers);
        }

        private List<Carta> PurCardsOnField()
        {
            var CanDrawCards = this.Player.Hands.Where(x => x.Custo <= this.Player.ManaCurrent).OrderBy(x => x.Custo);
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

        public int[] PrepareAttack()
        {
            List<int> Attackers = new List<int>();
            this.Player.DefenseField.ForEach(c =>
            {
                if (c.CanBeMoved)
                {
                    Attackers.Add(c.Id);
                }
            });

            Attackers.AddRange(this.Player.AtackField.Select(x => x.Id).ToList());

            return Attackers.ToArray();
        }


    }
}