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
        private Player Opponent { get; set; }
        private Battle _battle;

        public AIPlayer(Player Player, Battle Engine)
        {
            if (!Player.IsAIControlled)
                throw new Exception("ERROR");
            this.Player = Player;
            this._battle = Engine;

            if (this.Player == _battle.player1)
                Opponent = _battle.player2;
            else
                Opponent = _battle.player1;
        }


        public void PrepareOffense()
        {
            this.Player.DrawCard();
            _battle.EndPhase();

            var AtkCards = PrepareAttack();

            List<Carta> DefensePrep = new List<Carta>();// PurCardsOnField();

            foreach (var card in this.Player.AtackField)
            {
                if (!AtkCards.Contains(card))
                {
                    DefensePrep.Add(card);
                }
            }
            DefensePrep.ForEach(x =>
                {
                    this.Player.AtackField.Remove(x);
                    this.Player.DefenseField.Add(x);
                });


            DefensePrep.AddRange(PurCardsOnField());

            //Basic just to program responses;
            var FEcreatures = (from item in DefensePrep
                               select new
                               {
                                   item.Id,
                                   item.Ataque,
                                   item.Defesa,
                                   Position = "defense"
                               });


            Player.AddNotification(_battle.Phase, JsonConvert.SerializeObject(FEcreatures));

            _battle.EndPhase();


            var Attackers = (from item in AtkCards
                             select new
                               {
                                   item.Id,
                                   item.Ataque,
                                   item.Defesa,
                                   Position = "Offense"
                               });


            MoveAttackerstToAttackField(AtkCards);

            _battle.Turn.SetAttackers(AtkCards);



            Player.AddNotification(_battle.Phase, JsonConvert.SerializeObject(Attackers));
            _battle.EndPhase();

        }





        public List<Carta> PrepareAttack()
        {
            List<Carta> Attackers = new List<Carta>();
            var minimunSafeLife = this.Opponent.DefenseField.Max(x => x.Ataque);

            this.Player.DefenseField.ForEach(c =>
            {
                if (c.CanBeMoved && c.Defesa >= minimunSafeLife)
                {
                    Attackers.Add(c);
                }
            });
            Attackers.AddRange(this.Player.AtackField.Where(x => x.Defesa >= minimunSafeLife));





            return Attackers;
        }

        public void PrepareDefense(IEnumerable<int> Attackers)
        {
            var def = Player.DefenseField;
            var atk = this._battle.Turn.Atackers;
            List<int> usedDeffenders = new List<int>();
            Func<Carta,bool> IsWeak  = c=> {
                return c.Ataque <= 100 || c.Defesa <= 100;
            };

            int WeekCardsCount = def.Count(c =>IsWeak(c));
            
            List<FEBattleMatch> matchUps = new List<FEBattleMatch>();
            foreach (var _attacker in atk.OrderByDescending(x => x.Defesa))
            {
                var defId = 0;

                var prioritaryD = (from item in def
                                   where item.Defesa > _attacker.Ataque &&
                                   item.Ataque > _attacker.Defesa &&
                                   !usedDeffenders.Contains(item.Id)
                                   select item).OrderBy(x => x.Defesa).FirstOrDefault();
                if (prioritaryD != null)
                {
                    defId = prioritaryD.Id;
                }
                else
                {
                    List<Carta> scope = new List<Carta>();
                    if (HasLessCardsThanOpponentInField())
                    {
                        scope = (from item in def
                                 where
                                 item.Ataque > _attacker.Defesa &&
                                 item.Ataque + 50 <= _attacker.Ataque && 
                                 !usedDeffenders.Contains(item.Id)
                                 select item).ToList();
                    }
                    else
                    {
                        scope = (from item in def
                                 where
                                 item.Ataque > _attacker.Defesa &&
                                 !usedDeffenders.Contains(item.Id)
                                 select item).ToList();
                    }


                    if (scope.Count > 0)
                    {
                        defId = scope.OrderBy(x => x.Ataque).First().Id;
                    }
                    else
                    {
                        if (_attacker.Ataque >= 250 || _attacker.Defesa >= 200)
                        {
                            defId = this.Player.DefenseField.OrderByDescending(x => x.Ataque).FirstOrDefault().Id;
                        }
                    }
                }

                if (defId == 0 && WeekCardsCount > 0)
                {
                    var wrapper = this.Player.DefenseField.FirstOrDefault(x=> IsWeak(x) && 
                                 !usedDeffenders.Contains(x.Id));
                    if (wrapper != null)
                        defId = wrapper.Id;
                }

                if (defId != 0)
                {
                    matchUps.Add(new FEBattleMatch()
                    {
                        atkCardId = _attacker.Id,
                        defCardId = defId
                    });
                    usedDeffenders.Add(defId);
                }
            }

            Player.AddNotification(_battle.Phase, JsonConvert.SerializeObject(matchUps));
        }

        private List<Carta> PurCardsOnField()
        {
            var CanDrawCards = this.Player.Hands.Where(x => x.Custo <= this.Player.ManaCurrent);
            List<Carta> DrawCards = new List<Carta>();


            if (HasLessCardsThanOpponentInField())
                CanDrawCards.OrderBy(x => x.Custo);
            else
                CanDrawCards.OrderBy(x => (x.Ataque + x.Defesa) / x.Custo);

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


        private bool HasLessCardsThanOpponentInField()
        {
            return GetOpponentCardsCount() > GetMyCardsCount();
        }

        private void MoveAttackerstToAttackField(List<Carta> AtkCards)
        {
            foreach (var item in AtkCards)
            {
                var idx = Player.DefenseField.IndexOf(item);
                if (idx != -1)
                {
                    Player.DefenseField.Remove(item);
                    Player.AtackField.Add(item);
                }

            }
        }
        private int GetMyCardsCount()
        {
            return this.Player.AtackField.Count + this.Player.DefenseField.Count;
        }
        private int GetOpponentCardsCount()
        {
            return (Opponent.AtackField.Count + Opponent.DefenseField.Count);
        }
    }
}