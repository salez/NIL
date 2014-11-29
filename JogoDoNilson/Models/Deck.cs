using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JogoDoNilson.Models
{

    public enum CardPowerRating
    {
        Strong = 1, Medium, Weak
    }
    public partial class Carta
    {
        public CardPowerRating PowerRate
        {
            get
            {
                return (CardPowerRating)this.classificacao;
            }
        }
        public bool IsDead
        {
            get
            {
                return this.Defesa <= 0;
            }
        }

        public static List<Carta> GetAllCards()
        {
            using (JogoDoNilsonEntities db = new JogoDoNilsonEntities())
            {
                return db.jdn_cartas.ToList();
            }
        }
        public bool CanBeMoved { get; set; }
    }


    /// <summary>
    /// Player Deck of Cards
    /// </summary>
    public class Deck
    {
        private Stack<Carta> _Deck = new Stack<Carta>();
        public int OwnerPlayer { get; private set; }

        #region constructors

        public Deck(IEnumerable<Carta> deck, int OwnerNumber)
        {
            this._Deck = new Stack<Carta>(deck);
            this.OwnerPlayer = OwnerNumber;
        }

        public Deck(Stack<Carta> deck, int OwnerNumber)
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
        public List<Carta> GetInitialHands()
        {
            var InitialHand = new List<Carta>();
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
        public Carta RetrieveCard()
        {
            if (_Deck.Count() == 0)
                return null;

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

        public IEnumerable<Carta> AllCards(){
            return _Deck.ToList().OrderBy(x=> x.PowerRate);
        }

        /// <summary>
        /// Shuffle Cards
        /// </summary>
        private void ShuffleCards()
        {
            var RandomFactor = new Random();
            List<Carta> Cards = _Deck.ToList();
            var TotalCardsCount = Cards.Count;
            _Deck.Clear();
            for (int i = 0; i < TotalCardsCount; i++)
            {
                var CardIndex = RandomFactor.Next(TotalCardsCount - i);
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
        public static List<Deck> BuildDecks(IEnumerable<Carta> AllCards)
        {

            List<Carta> DeckCards = GetNewGameCards(AllCards);
            var DeckCollection = BuildRandomDecks(DeckCards);

            return DeckCollection;
        }

        /// <summary>
        /// Build Two Random Decks Based on a Cards Collection
        /// </summary>
        /// <param name="DeckCards">COllection To Use</param>
        /// <returns></returns>
        private static List<Deck> BuildRandomDecks(IEnumerable<Carta> DeckCards)
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
        private static List<Deck> BuildDeck(IEnumerable<Carta> DeckCards)
        {
            var DeckCollection = new List<Deck>();
            List<Stack<Carta>> PlayersCards = new List<Stack<Carta>>(2);


            Stack<Carta> PlayerOneCards = new Stack<Carta>();
            Stack<Carta> PlayerTwoCards = new Stack<Carta>();
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
        private static List<Carta> GetNewGameCards(IEnumerable<Carta> Cards)
        {
            var _Cards = Cards.ToList();
            List<Carta> DeckCards = new List<Carta>();
            Random RandomFactor = new Random();
            var TotalCardsCount = _Cards.Count;
            var UsedCardsCount = GameSettings.InitialDeckCardsCount * 2;
            for (int i = 0; i < UsedCardsCount; i++)
            {
                var CardIndex = RandomFactor.Next(TotalCardsCount - i);
                DeckCards.Add(_Cards[CardIndex]);
                _Cards.RemoveAt(CardIndex);
            }
            return DeckCards;
        }
        #endregion
    }


    //OLD Card Class Used on GameEngine And Deck Class
    //public class Carta
    //{
    //    public string Nome { get; set; }
    //    public int Custo { get; set; }
    //    public int Ataque { get; set; }
    //    public int Defesa { get; set; }
    //    public string Type { get; set; }
    //    public string Image { get; set; }

    //    public bool IsDead
    //    {
    //        get
    //        {
    //            return this.Defesa <= 0;
    //        }
    //    }
    //    public CardPowerRating PowerRate { get; set; }
    //    public Race Race { get; set; }


    //    public Carta() { }
    //    public Carta(Carta DbObject)
    //    {
    //        this.Nome = DbObject.Nome;
    //        this.Defesa = DbObject.Defesa;
    //        this.Ataque = DbObject.Ataque;
    //        this.Custo = DbObject.Custo;
    //        this.Image = DbObject.Id + ".jpg";
    //        this.PowerRate = (CardPowerRating)DbObject.classificacao;
    //        this.Race = new Race() { Id = 0, Name = DbObject.Raça };
    //        this.Type = DbObject.Classe;
    //    }

    //    /// <summary>
    //    /// Get All Cards From the DataBase
    //    /// </summary>
    //    /// <returns>List of Card Objects</returns>
    //    public static List<Carta> GetAllCards()
    //    {
    //        using (JogoDoNilsonEntities db = new JogoDoNilsonEntities())
    //        {
    //            return db.jdn_cartas.ToList();
    //        }
    //    }
    //}

}