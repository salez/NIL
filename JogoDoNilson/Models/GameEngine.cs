using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;



namespace JogoDoNilson.Models
{
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
            this.Mana = GameSettings.PlayerSettings.InitialMana;
            this.Hands = this.Deck.GetInitialHands();
            this.IsAIControlled = IsAI;
            this.Turn = new PlayerTurn();
        }


        public Deck Deck { get; private set; }
        public List<Carta> Hands { get; private set; }
        public int Number { get; private set; }
        public string AvatarImage { get; private set; }
        public bool IsAIControlled { get; private set; }
        public PlayerTurn Turn { get; private set; }

        public int Life { get; set; }
        public int Mana { get; set; }
    }

    public enum BattlePhase
    {
        Draw = 1,
        Main = 2,
        Atack = 3,
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
    }


    public class BattleTurn{
        public int Count{get; private set;}
        public Player Player{get;private set;}

        public void SetCount(int count){
            this.Count = count;
        }

        public void IncrementCount()
        {
            this.Count++;
        }

        public void IncrementCount(int count)
        {
            this.Count = count;
        }

        public void SetPlayer(Player player)
        {
            this.Player = player;
            this.Player.Turn.IncrementCount();
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
        public BattleFight(Carta AttackerCard, Carta DefenderCard)
        {
            this.Attacker = AttackerCard;
            this.Defender = DefenderCard;
        }
        public Carta Attacker { get; private set; }
        public Carta Defender { get; private set; }

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
                return new BattleFightResult(null, Defender, 0);
            if (Defender == null)
                return new BattleFightResult(Attacker, null, Attacker.Ataque);

            int ResultDamage = Math.Abs(Defender.Defesa - Attacker.Ataque);
            Defender.Defesa = ResultDamage;
            Attacker.Defesa = Attacker.Defesa - Defender.Ataque;

            if (Attacker.IsDead)
            {
                if (!Defender.IsDead)
                {
                    return new BattleFightResult(null, Defender, 0);
                }
                else
                {
                    return new BattleFightResult(null, null, 0);
                }
            }
            else
            {
                if (!Defender.IsDead)
                {
                    return new BattleFightResult(Attacker, Defender, 0);
                }
                else
                {
                    return new BattleFightResult(Attacker, null, ResultDamage);
                }
            }
        }

    }

    public class BattleFightResult
    {
        public BattleFightResult(Carta Atacker, Carta Defender, int DelledDamage)
        {
            this.Atacker = Atacker;
            this.Defender = Defender;
            this.DelledDamage = DelledDamage;
        }
        public Carta Atacker { get; private set; }
        public Carta Defender { get; private set; }
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
            if (this.Battle != null)
            {
                GameState.Battle = new Models.Battle();

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
