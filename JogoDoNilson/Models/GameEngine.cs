﻿using System;
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
        }


        public Deck Deck { get; private set; }
        public List<Carta> Hands { get; private set; }
        public int Number { get; private set; }
        public string AvatarImage { get; private set; }
        public bool IsAIControlled { get; private set; }

        public int Life { get; set; }
        public int Mana { get; set; }
    }

    public class Battle
    {
        public Battle()
        {

        }

        public enum Phase
        {
            Draw = 1,
            Main = 2,
            Atack = 3,
            Defense = 4,
            End = 5
        }

        public Turn Turn { get; private set; }

        public Phase Phase { get; private set; }

        public bool isStarted { get; private set; }

        
    }

    

    public class Turn{
        public int Count{get; private set;}
        public Player PlayerTurn{get;private set;}
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

        private AtackResult _result;
        public AtackResult Result
        {
            get
            {
                return _result ?? (_result = this.CalculateResult());
            }
        }


        private AtackResult CalculateResult()
        {
            if (Attacker == null)
                return new AtackResult(null, Defender, 0);
            if (Defender == null)
                return new AtackResult(Attacker, null, Attacker.Ataque);

            int ResultDamage = Math.Abs(Defender.Defesa - Attacker.Ataque);
            Defender.Defesa = ResultDamage;
            Attacker.Defesa = Attacker.Defesa - Defender.Ataque;

            if (Attacker.IsDead)
            {
                if (!Defender.IsDead)
                {
                    return new AtackResult(null, Defender, 0);
                }
                else
                {
                    return new AtackResult(null, null, 0);
                }
            }
            else
            {
                if (!Defender.IsDead)
                {
                    return new AtackResult(Attacker, Defender, 0);
                }
                else
                {
                    return new AtackResult(Attacker, null, ResultDamage);
                }
            }
        }

    }

    public class AtackResult
    {
        public AtackResult(Carta Atacker, Carta Defender, int DelledDamage)
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

        public void StartBattle()
        {
            if (!this.isStarted)
            {
                this.isStarted = true;

                //todo: inicializar batalha
                this.Turn = new Turn();
                this.Turn.Count = 1;

                Random rd = new Random();
                
                if(rd.Next(1,2) == 1){
                    this.Turn.PlayerTurn = this.
                }
            }
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
