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
        public Player(Deck deck, int number, bool IsAI, string Avatar = "")
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

        private Dictionary<BattlePhase, string> notifications = new Dictionary<BattlePhase, string>();


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

            if (card == null)
                return new ReturnStatus(EnumResult.Error, "Invalid Card");

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

            if (!card.CanBeMoved)
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

        public void IncrementTurnCount()
        {
            this.Turn.IncrementCount();
        }

        public void AddNotification(BattlePhase Phase, string Data)
        {
            if (notifications.Count(x => x.Key == Phase) > 0)
                return;

            notifications.Add(Phase, Data);
        }

        public KeyValuePair<BattlePhase, string> RetrieveFirstNotification(BattlePhase Phase)
        {
            var scope = notifications.Where(x => x.Key == Phase);
            if (scope.Count() == 0)
                return new KeyValuePair<BattlePhase, string>(BattlePhase.Draw, "ERROR");

            var notification = notifications.OrderBy(x => x.Key).First();
            notifications.Remove(notification.Key);
            return notification;
        }

        public KeyValuePair<BattlePhase, string> RetrieveFirstNotification()
        {
            if (notifications.Count() == 0)
                return new KeyValuePair<BattlePhase, string>(BattlePhase.Draw, "ERROR");

            var notification = notifications.OrderBy(x => x.Key).First();
            notifications.Remove(notification.Key);
            return notification;
        }

        public void ClearNotificationList()
        {
            this.notifications.Clear();
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

        public Battle(GameEngine Engine)
        {
            this.Turn = new BattleTurn();
            this.Turn.SetCount(0);
            this.Phase = BattlePhase.Draw;

            this.player1 = Engine.PlayerOne;
            this.player2 = Engine.PlayerTwo;
        }

        private GameEngine _engine;

        public Player player1 { get; private set; }

        public Player player2 { get; private set; }


        public BattleTurn Turn { get; private set; }

        public BattlePhase Phase { get; private set; }

        public bool isStarted { get; private set; }

        public void InitNewTurn()
        {
            Phase = BattlePhase.Draw;
            this.Turn.Player.DefenseField.ForEach(c => c.CanBeMoved = true);
            this.Turn.Player.AtackField.ForEach(c => c.CanBeMoved = true);
            if (this.Turn.Player.IsAIControlled)
            {
                AIPlayer _player = new AIPlayer(this.Turn.Player, this);
                _player.PrepareOffense();
            }
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
            this.Turn.EndTurn(player1, player2);
            InitNewTurn();
        }
    }


    public class BattleTurn
    {
        public int Count { get; private set; }
        public Player Player { get; private set; }
        public List<Carta> Atackers { get; private set; }

        public void SetCount(int count)
        {
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
            player1.ClearNotificationList();
            player2.ClearNotificationList();
            if (this.Player == player1)
            {
                this.Player = player2;
            }
            else if (this.Player == player2)
            {
                this.Player = player1;
            }
            if (this.Count > 1)
            {
                this.Player.ManaTotal++;
            }
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
        public PlayerTurn()
        {
            this.Count = 0;
        }

        public int Count { get; private set; }

        public void IncrementCount()
        {
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
            /*
            foreach (var defender in Defenders)
            {
                int ResultDamage = Math.Abs(defender.Defesa - Attacker.Ataque);
                defender.Defesa = ResultDamage;
            }

            Attacker.Defesa = Attacker.Defesa - Defenders.Sum(d => d.Ataque);

            return new BattleFightResult(Attacker,Defenders);
             */

            int ResultDamage = 0;
            foreach (var defender in Defenders)
            {
                int ParcialResultDamage = Math.Abs(defender.Defesa - Attacker.Ataque);
                ResultDamage = ParcialResultDamage > ResultDamage ?
                    ParcialResultDamage : ResultDamage;
                defender.Defesa = defender.Defesa - Attacker.Ataque;
            }

            Attacker.Defesa = Attacker.Defesa - Defenders.Sum(d => d.Ataque);

            return new BattleFightResult(Attacker, Defenders, ResultDamage);
        }

    }

    public class BattleFightResult
    {
        public BattleFightResult(Carta Atacker, List<Carta> Defenders, int PlayerLifeDamage)
        {
            this.Atacker = Atacker;
            this.Defender = Defenders;
            this.PlayerLifeDamage = PlayerLifeDamage;
        }
        public Carta Atacker { get; private set; }
        public List<Carta> Defender { get; private set; }

        int _playerLifeDamage;
        public int PlayerLifeDamage
        {
            get
            {
                if (!Atacker.IsDead && Defender.All(x => x.IsDead))
                    return _playerLifeDamage;
                else
                    return 0;
            }
            private set
            {
                _playerLifeDamage = value;
            }
        }
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

                GameState.PlayerOne = new Player(Decks[0], 1, true, string.Format("~/Images/Avatars/{0}.jpg", rd.Next(1, 8)));
                GameState.PlayerTwo = new Player(Decks[1], 2, false, PlayerAvatar);
            }
        }

        public Battle StartBattle()
        {
            if (this.Battle == null)
            {
                GameState.Battle = new Models.Battle(this);

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
