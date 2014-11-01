using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;



namespace JogoDoNilson.Models
{
    public enum EnumResult
    {
        Error = 0,
        Ok = 1
    }

    public class ReturnStatus
    {
        public ReturnStatus(EnumResult result)
        {
            this.Status = result;
            this.Message = null;
        }

        public ReturnStatus(EnumResult result, string msg)
        {
            this.Status = result;
            this.Message = msg;
        }

        public EnumResult Status { get; set; }

        public string Message { get; set; }
    }
    public class Race
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public sealed class GameSettings
    {

        public static int InitialDeckCardsCount { get { return 20; } }
        public static int InitialHandsCardsCount { get { return 4; } }
        public static int HandsGrowRate { get { return 1; } }

        internal class PlayerSettings
        {
            public static int InitialLife { get { return 1000; } }
            public static int InitialMana { get { return 2; } }
            public static int ManaGrowRate { get { return 1; } }
        }
    }

    public class Player
    {
        public Player(Deck deck, int number, bool IsAI,string Avatar = "")
        {
            this.Deck = deck;
            this.Number = number;
            this.AvatarImage = Avatar;
            this.Life = GameSettings.PlayerSettings.InitialLife;
            this.ManaCurrent = this.ManaTotal = GameSettings.PlayerSettings.InitialMana;
            this.Hands = this.Deck.GetInitialHands();
            this.IsAIControlled = IsAI;
            this.Turn = new PlayerTurn();
            this.AtackField = new List<Carta>();
            this.DefenseField = new List<Carta>();
            this.Graveyard = new List<Carta>();
        }

        public Deck Deck { get; private set; }
        public List<Carta> Hands { get; private set; }
        public List<Carta> AtackField { get; private set; }
        public List<Carta> DefenseField { get; private set; }
        public List<Carta> Graveyard { get; private set; }
        public int Number { get; private set; }
        public string AvatarImage { get; private set; }
        public bool IsAIControlled { get; private set; }
        public PlayerTurn Turn { get; private set; }

        public int Life { get; set; }
        public int ManaCurrent { get; set; }
        public int ManaTotal { get; set; }

        public Carta DrawCard()
        {
            var card = this.Deck.RetrieveCard();

            this.Hands.Add(card);
            return card;
        }

        public ReturnStatus PutCardInField(int cardId)
        {
            Carta card = this.Hands.FirstOrDefault(c => c.Id == cardId);

            if(card == null)
                return new ReturnStatus(EnumResult.Error,"Invalid Card");

            if (this.ManaCurrent < card.Custo)
                return new ReturnStatus(EnumResult.Error, "Insuficcient Mana");

            this.DefenseField.Add(card);
            this.Hands.Remove(card);

            this.ManaCurrent = this.ManaCurrent - card.Custo;

            card.CanBeMoved = false;

            return new ReturnStatus(EnumResult.Ok);
        }

        public List<Carta> ChooseAttackers(int[] cardIds)
        {
            List<Carta> cards = this.AtackField.Where(x => cardIds.Contains(x.Id)).ToList();

            if (cards.Count() == 0)
                throw new Exception("No cards selected");

            return cards;
        }

        public List<Carta> ChooseDefenders(int[] cardIds)
        {
            List<Carta> cards = this.DefenseField.Where(x => cardIds.Contains(x.Id)).ToList();

            if (cards.Count() == 0)
                throw new Exception("No cards selected");

            return cards;
        }

        public ReturnStatus MoveCardToAtackField(int cardId)
        {
            Carta card = this.DefenseField.FirstOrDefault(c => c.Id == cardId);

            if (card == null)
                return new ReturnStatus(EnumResult.Error, "Invalid card");

            if(!card.CanBeMoved)
                return new ReturnStatus(EnumResult.Error, "Card can't be moved");

            this.AtackField.Add(card);
            this.DefenseField.Remove(card);

            card.CanBeMoved = false;

            return new ReturnStatus(EnumResult.Ok);
        }

        public ReturnStatus MoveCardToDefenseField(int cardId)
        {
            Carta card = this.AtackField.FirstOrDefault(c => c.Id == cardId);

            if (card == null)
                return new ReturnStatus(EnumResult.Error, "Invalid card");

            if (!card.CanBeMoved)
                return new ReturnStatus(EnumResult.Error, "Card can't be moved");

            this.DefenseField.Add(card);
            this.AtackField.Remove(card);

            card.CanBeMoved = false;

            return new ReturnStatus(EnumResult.Ok);
        }

        public void IncrementTurnCount(){
            this.Turn.IncrementCount();
        }
    }

    public enum BattlePhase
    {
        Draw = 1,
        Main = 2,
        Attack = 3,
        Defense = 4,
        End = 5
    }

    public class Battle
    {
        public Battle()
        {
            this.Turn = new BattleTurn();
            this.Turn.SetCount(0);
            this.Phase = BattlePhase.Draw;


        }

        public Battle(Player player1, Player player2)
        {
            this.Turn = new BattleTurn();
            this.Turn.SetCount(0);
            this.Phase = BattlePhase.Draw;

            this.player1 = player1;
            this.player2 = player2;
        }

        public Player player1 { get; private set; }

        public Player player2 { get; private set; }

        public BattleTurn Turn { get; private set; }

        public BattlePhase Phase { get; private set; }

        public bool isStarted { get; private set; }

        public void NewTurn()
        {
            if (this.Turn.Player == player1)
            {
                this.Turn.SetPlayer(player2);
            }
            else
            {
                this.Turn.SetPlayer(player1);
            }

            this.Turn.IncrementCount();
        }

        public void EndPhase()
        {
            //todo: ações de fim de turno baseado na fase?
            switch (this.Phase)
            {
                case BattlePhase.Draw:
                    break;
                case BattlePhase.Main:
                    break;
                case BattlePhase.Attack:
                    break;
                case BattlePhase.Defense:
                    break;
                case BattlePhase.End:
                    break;
                default:
                    break;
            }

            this.Phase++;
            if ((int)this.Phase == 5)
            {
                this.Phase = BattlePhase.Draw;
                EndTurn();
            }
        }

        public void EndTurn()
        {
            this.Turn.EndTurn(player1,player2);
        }
    }


    public class BattleTurn{
        public int Count{get; private set;}
        public Player Player{get;private set;}
        public List<Carta> Atackers { get; private set; }

        public void SetCount(int count){
            this.Count = count;
        }

        public void IncrementCount()
        {
            this.Count++;
        }

        public void SetPlayer(Player player)
        {
            this.Player = player;
            this.Player.Turn.IncrementCount();
            this.IncrementCount();
        }

        public void EndTurn(Player player1, Player player2)
        {
            if (this.Player == player1)
            {
                this.Player = player2;
            }
            else if (this.Player == player2)
            {
                this.Player = player1;
            }

            this.Player.ManaTotal++;
            this.Player.ManaCurrent = this.Player.ManaTotal;

            this.IncrementCount();
        }

        public void SetAttackers(List<Carta> cards)
        {
            this.Atackers = cards;
        }
    }

    public class PlayerTurn
    {
        public PlayerTurn(){
            this.Count = 0;
        }

        public int Count { get; private set; }

        public void IncrementCount(){
            this.Count++;
        }
    }

    public class BattleFight
    {
        public BattleFight(Carta AttackerCard, List<Carta> DefenderCards)
        {
            this.Attacker = AttackerCard;
            this.Defenders = DefenderCards;
        }
        public Carta Attacker { get; private set; }
        public List<Carta> Defenders { get; private set; }

        private BattleFightResult _result;
        public BattleFightResult Result
        {
            get
            {
                return _result ?? (_result = this.CalculateResult());
            }
        }


        private BattleFightResult CalculateResult()
        {
            if (Attacker == null)
                return new BattleFightResult(null, Defenders, 0);
            if (Defenders == null)
                return new BattleFightResult(Attacker, null, Attacker.Ataque);

            foreach (var defender in Defenders)
            {
                int ResultDamage = Math.Abs(defender.Defesa - Attacker.Ataque);
                defender.Defesa = ResultDamage;
            }

            Attacker.Defesa = Attacker.Defesa - Defenders.Sum(d => d.Ataque);

            if (Attacker.IsDead)
            {
                //if (!Defenders.IsDead)
                //{
                //    return new BattleFightResult(null, Defenders, 0);
                //}
                //else
                //{
                //    return new BattleFightResult(null, null, 0);
                //}
            }
            else
            {
                //if (!Defenders.IsDead)
                //{
                //    return new BattleFightResult(Attacker, Defenders, 0);
                //}
                //else
                //{
                //    return new BattleFightResult(Attacker, null, ResultDamage);
                //}
            }

            return new BattleFightResult(null, null, 0);
        }

    }

    public class BattleFightResult
    {
        public BattleFightResult(Carta Atacker, List<Carta> Defenders, int DelledDamage)
        {
            this.Atacker = Atacker;
            this.Defender = Defenders;
            this.DelledDamage = DelledDamage;
        }
        public Carta Atacker { get; private set; }
        public List<Carta> Defender { get; private set; }
        public int DelledDamage { get; private set; }
    }

    /// <summary>
    /// This class do basic Game Tasks like Build Decks and Battles 
    /// </summary> 
    public class GameEngine
    {
        private GameSession GameState;
        public GameEngine(HttpSessionStateBase Session)
        {
            // TODO: Complete member initialization
            GameState = new GameSession(Session);
        }

        public Player PlayerOne
        {
            get
            {
                return GameState.PlayerOne;
            }
        }
        public Player PlayerTwo
        {
            get
            {
                return GameState.PlayerTwo;
            }
        }

        public Battle Battle
        {
            get
            {
                return GameState.Battle;
            }
        }

        public void StartGame(string PlayerAvatar)
        {
            if (!GameState.IsStarted)
            {
                var Decks = Deck.BuildDecks(Carta.GetAllCards());
                Random rd = new Random();
                
                GameState.PlayerOne = new Player(Decks[0], 1, true, string.Format("~/Images/Avatars/{0}.jpg", rd.Next(1,8)));
                GameState.PlayerTwo = new Player(Decks[1], 2, false, PlayerAvatar);
            }
        }

        public Battle StartBattle()
        {
            if (this.Battle == null)
            {
                GameState.Battle = new Models.Battle(this.PlayerOne, this.PlayerTwo);

                var battle = this.Battle;

                //inicializa primeiro turno
                Random rd = new Random();

                if (rd.Next(1, 2) == 1)
                {
                    battle.Turn.SetPlayer(this.PlayerOne);
                }
                else
                {
                    battle.Turn.SetPlayer(this.PlayerTwo);
                }
            }

            return this.Battle;
        }

        private class GameSession
        {
            private static HttpContext Context = HttpContext.Current;
            private HttpSessionStateBase Session;

            public GameSession(HttpSessionStateBase Session)
            {
                // TODO: Complete member initialization
                this.Session = Session;
            }

            public Battle Battle
            {
                get
                {
                    return (Battle)Session["BATTLE"];
                }
                set
                {
                    Session["BATTLE"] = value;
                }
            }

            public Player PlayerOne
            {
                get
                {
                    return (Player)Session["PLAYERONE"];
                }
                set
                {
                    Session["PLAYERONE"] = value;
                }
            }
            public Player PlayerTwo
            {
                get
                {
                    return (Player)Session["PLAYERTWO"];
                }
                set
                {
                    Session["PLAYERTWO"] = value;
                }
            }

            public bool IsStarted
            {
                get
                {
                    if (Session != null)
                    {
                        return this.PlayerOne != null;
                    }
                    return false;
                }
            }
        }
    }
}
