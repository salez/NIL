using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;



namespace NilGame
{
    public class Card
    {
        public string Name { get; set; }
        public int Cost { get; set; }
        public int Atack { get; set; }
        public int Life { get; set; }
        public int Type { get; set; }
        public string Image { get; set; }

        public bool IsDead
        {
            get
            {
                return this.Life <= 0;
            }
        }
        public CardPowerRating PowerRate { get; set; }
        public Race Race { get; set; }
    }


    public enum CardPowerRating
    {
        Strong = 1, Medium, Weak
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

    /// <summary>
    /// Player Deck of Cards
    /// </summary>
    public class Deck
    {
        private Stack<Card> _Deck = new Stack<Card>();
        public int OwnerPlayer { get; private set; }

        #region constructors

        public Deck(IEnumerable<Card> deck, int OwnerNumber)
        {
            this._Deck = new Stack<Card>(deck);
            this.OwnerPlayer = OwnerNumber;
        }

        public Deck(Stack<Card> deck, int OwnerNumber)
        {
            this._Deck = deck;
            this.OwnerPlayer = OwnerNumber;
        } 
        #endregion

        public int CardsCount
        {
            get { return _Deck.Count; }
        }

        #region methods
        /// <summary>
        /// Remove and returns the initial cards of the player
        /// </summary>
        /// <returns>Initial Player Cards</returns>
        public List<Card> GetInitialHands()
        {
            var InitialHand = new List<Card>();
            for (int i = 0; i < GameSettings.InitialHandsCardsCount; i++)
            {
                InitialHand.Add(_Deck.Pop());
            }
            return InitialHand;
        }

        /// <summary>
        /// Get and removes the on top
        /// </summary>
        /// <returns>Card on top</returns>
        public Card RetrieveCard()
        {
            return _Deck.Pop();
        }
        /// <summary>
        /// Shuffle Deck
        /// </summary>
        public void Shuffle()
        {
            ShuffleCards();
            ShuffleCards();
            ShuffleCards();
        }
        /// <summary>
        /// Shuffle Cards
        /// </summary>
        private void ShuffleCards()
        {
            var RandomFactor = new Random();
            List<Card> Cards = new List<Card>();
            for (int i = 0; i < _Deck.Count; i++)
            {
                Cards.Add(_Deck.Pop());
            }
            for (int i = 0; i < Cards.Count; i++)
            {

                var CardIndex = RandomFactor.Next(Cards.Count - i);
                _Deck.Push(Cards[CardIndex]);
                Cards.RemoveAt(CardIndex);
            }
        } 
        #endregion

        #region StaticMethods
        /// <summary>
        /// Build Two random decks to start the Game.
        /// </summary>
        /// <param name="AllCards">All Cards on the database</param>
        /// <returns></returns>
        public static List<Deck> BuildDecks(IEnumerable<Card> AllCards)
        {

            List<Card> DeckCards = GetNewGameCards(AllCards);
            var DeckCollection = BuildRandomDecks(DeckCards);

            return DeckCollection;
        }

        /// <summary>
        /// Build Two Random Decks Based on a Cards Collection
        /// </summary>
        /// <param name="DeckCards">COllection To Use</param>
        /// <returns></returns>
        private static List<Deck> BuildRandomDecks(IEnumerable<Card> DeckCards)
        {
            var DeckCollection = BuildDeck(DeckCards);
            DeckCollection[0].Shuffle();
            DeckCollection[1].Shuffle();
            return DeckCollection;
        }

        /// <summary>
        /// Separeta the Cards in two Groups
        /// </summary>
        /// <param name="DeckCards">Cards Collection To separate</param>
        /// <returns></returns>
        private static List<Deck> BuildDeck(IEnumerable<Card> DeckCards)
        {
            var DeckCollection = new List<Deck>();
            List<Stack<Card>> PlayersCards = new List<Stack<Card>>(2);


            Stack<Card> PlayerOneCards = new Stack<Card>();
            Stack<Card> PlayerTwoCards = new Stack<Card>();
            foreach (var PowerRate in (CardPowerRating[])Enum.GetValues(typeof(CardPowerRating)))
            {
                foreach (var Card in DeckCards.Where(x => x.PowerRate == PowerRate))
                {
                    if (PlayerOneCards.Count <= PlayerTwoCards.Count)
                        PlayerOneCards.Push(Card);
                    else
                        PlayerTwoCards.Push(Card);
                }
            }
            DeckCollection.Add(new Deck(PlayerOneCards, 1));
            DeckCollection.Add(new Deck(PlayerTwoCards, 2));
            return DeckCollection;
        }


        /// <summary>
        /// Get the Correct Number of random cards needed to build two decks
        /// </summary>
        /// <param name="Cards">Cards Collection</param>
        /// <returns></returns>
        private static List<Card> GetNewGameCards(IEnumerable<Card> Cards)
        {
            var _Cards = Cards.ToList();
            List<Card> DeckCards = new List<Card>();
            Random RandomFactor = new Random();
            var TotalCardsCount = _Cards.Count;
            var UsedCardsCount = GameSettings.InitialDeckCardsCount * 2;
            for (int i = 0; i < UsedCardsCount; i++)
            {
                var CardIndex = RandomFactor.Next(TotalCardsCount - 1);
                DeckCards.Add(_Cards[CardIndex]);
                _Cards.RemoveAt(CardIndex);
            }
            return DeckCards;
        }
        #endregion
    }


    public class Player
    {
        public Player(Deck deck, int number,bool IsAI)
        {
            this.Deck = deck;
            this.Number = number;
            this.Life = GameSettings.PlayerSettings.InitialLife;
            this.Mana = GameSettings.PlayerSettings.InitialMana;
            this.Hands = this.Deck.GetInitialHands();
            this.IsAIControlled = IsAI;
        }


        public Deck Deck { get; private set; }
        public List<Card> Hands { get; private set; }
        public int Number { get; private set; }
        public bool IsAIControlled { get; private set; }

        public int Life { get; set; }
        public int Mana { get; set; }
    }

    public class Battle
    {
        public Battle(Card AttackerCard, Card DefenderCard)
        {
            this.Attacker = AttackerCard;
            this.Defender = DefenderCard;
        }
        public Card Attacker { get; private set; }
        public Card Defender { get; private set; }

        private BattleResult _result;
        public BattleResult Result
        {
            get
            {
                return _result ?? (_result = this.CalculateResult());
            }
        }


        private BattleResult CalculateResult() 
        {
            if (Attacker == null)
                return new BattleResult(null, Defender, 0);
            if (Defender == null)
                return new BattleResult(Attacker, null, Attacker.Atack);

            int ResultDamage = Math.Abs(Defender.Life - Attacker.Atack);
            Defender.Life = ResultDamage;
            Attacker.Life = Attacker.Life - Defender.Atack;

            if (Attacker.IsDead)
            {
                if (!Defender.IsDead)
                {
                    return new BattleResult(null, Defender, 0);
                }
                else
                {
                    return new BattleResult(null, null, 0);
                }
            }
            else
            {
                if (!Defender.IsDead)
                {
                    return new BattleResult(Attacker, Defender, 0);
                }
                else
                {
                    return new BattleResult(Attacker, null, ResultDamage);
                }
            }
        }
        
    }

    public class BattleResult
    {
        public BattleResult(Card Atacker, Card Defender, int DelledDamage)
        {
            this.Atacker = Atacker;
            this.Defender = Defender;
            this.DelledDamage = DelledDamage;
        }
        public Card Atacker { get; private set; }
        public Card Defender { get; private set; }
        public int DelledDamage { get; private set; }
    }

    /// <summary>
    /// This class do basic Game Tasks like Build Decks and Battles 
    /// </summary> 
    public class GameEngine
    {

        public void StartGame()
        {
            var  Decks = Deck.BuildDecks(new List<Card>());
            GameSession.PlayerOne = new Player(Decks[0], 1,true);
            GameSession.PlayerTwo = new Player(Decks[1], 2,false);
        }

        private class GameSession
        {
            private static HttpContext Context = HttpContext.Current;

            public static Player PlayerOne
            {
                get
                {
                    return (Player)Context.Session["PLAYERONE"];
                }
                set
                {
                    Context.Session["PLAYERONE"] = value;
                }
            }
            public static Player PlayerTwo
            {
                get
                {
                    return (Player)Context.Session["PLAYERTWO"];
                }
                set
                {
                    Context.Session["PLAYERTWO"] = value;
                }
            }
        }
    }
}
