using System.Collections.Generic;
using UnityEngine;
using UMMonopoly.Data;
using UMMonopoly.Entities;
using UMMonopoly.Systems;

namespace UMMonopoly.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Config")]
        public GameConfigSO config;

        [Header("Runtime State")]
        public List<Player> Players = new List<Player>();
        public Board Board;
        public Bank Bank;
        public CardDeck AkademikDeck;
        public CardDeck KampusDeck;
        public DiceRoller Dice;
        public GameContext Context;

        public GameState CurrentState { get; private set; } = GameState.MainMenu;
        public int CurrentPlayerIndex { get; private set; }
        public int TurnNumber { get; private set; }
        public int DoublesInARow { get; private set; }

        public Player CurrentPlayer => Players[CurrentPlayerIndex];

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void StartGame(List<string> playerNames)
        {
            if (config == null)
            {
                Debug.LogError("GameManager: config is not assigned.");
                return;
            }

            Players.Clear();
            for (int i = 0; i < playerNames.Count; i++)
            {
                Players.Add(new Player(i, playerNames[i], config.startingMoney));
            }

            Board = new Board(config.tiles);
            Bank = new Bank();
            AkademikDeck = new CardDeck(CardDeckType.Akademik, config.akademikDeck);
            KampusDeck = new CardDeck(CardDeckType.Kampus, config.kampusDeck);
            Dice = new DiceRoller(config.diceCount, config.diceSides);
            Context = new GameContext(Board, Bank, AkademikDeck, KampusDeck, config, Players);

            CurrentPlayerIndex = 0;
            TurnNumber = 1;
            DoublesInARow = 0;
            SetState(GameState.TurnStart);
            EventBus.RaiseTurnStarted(CurrentPlayerIndex);
        }

        public void SetState(GameState s) => CurrentState = s;

        public int RollAndMove()
        {
            int total = Dice.Roll();
            Bank.LastDiceTotal = total;
            EventBus.RaiseDiceRolled(CurrentPlayer, total);

            if (Dice.LastWasDoubles)
            {
                DoublesInARow++;
                if (DoublesInARow >= config.maxDoublesBeforeJail)
                {
                    SendCurrentPlayerToJail();
                    DoublesInARow = 0;
                    return total;
                }
            }
            else
            {
                DoublesInARow = 0;
            }

            if (CurrentPlayer.InJail)
            {
                HandleJailRoll();
                return total;
            }

            CurrentPlayer.Move(total, config.boardSize, awardSalary: true, config.salaryOnGo);
            EventBus.RaisePlayerMoved(CurrentPlayer, CurrentPlayer.BoardPosition);
            ResolveCurrentTile();
            return total;
        }

        public void ResolveCurrentTile()
        {
            SetState(GameState.ResolveTilePhase);
            var tile = Board.GetTile(CurrentPlayer.BoardPosition);
            tile.OnPlayerLanded(CurrentPlayer, Context);
            CheckBankruptcy(CurrentPlayer);
            SetState(GameState.DecisionPhase);
        }

        public bool TryBuyCurrentTile()
        {
            var tile = Board.GetTile(CurrentPlayer.BoardPosition);
            if (tile is PropertyTile p && p.Owner == null) return Bank.BuyProperty(CurrentPlayer, p);
            if (tile is StationTile s && s.Owner == null) return Bank.BuyStation(CurrentPlayer, s);
            if (tile is UtilityTile u && u.Owner == null) return Bank.BuyUtility(CurrentPlayer, u);
            return false;
        }

        public bool TryUpgrade(PropertyTile p) => Bank.UpgradeProperty(p, Board);

        public void EndTurn()
        {
            EventBus.RaiseTurnEnded(CurrentPlayerIndex);

            if (Dice.LastWasDoubles && DoublesInARow > 0 && !CurrentPlayer.InJail)
            {
                SetState(GameState.RollPhase);  // same player rolls again
                return;
            }

            DoublesInARow = 0;
            AdvanceToNextLivePlayer();

            if (TryDetectGameOver(out var winner))
            {
                SetState(GameState.GameOver);
                EventBus.RaiseGameWon(winner);
                return;
            }

            TurnNumber++;
            SetState(GameState.TurnStart);
            EventBus.RaiseTurnStarted(CurrentPlayerIndex);
        }

        public void AttemptJailExit(bool payFine)
        {
            if (!CurrentPlayer.InJail) return;
            if (CurrentPlayer.GetOutOfJailCards > 0)
            {
                CurrentPlayer.GetOutOfJailCards--;
                CurrentPlayer.InJail = false;
                CurrentPlayer.JailTurnsRemaining = 0;
                return;
            }
            if (payFine && CurrentPlayer.TryPay(config.jailFine))
            {
                EventBus.RaiseMoneyChanged(CurrentPlayer, -config.jailFine);
                CurrentPlayer.InJail = false;
                CurrentPlayer.JailTurnsRemaining = 0;
            }
        }

        private void HandleJailRoll()
        {
            if (Dice.LastWasDoubles)
            {
                CurrentPlayer.InJail = false;
                CurrentPlayer.JailTurnsRemaining = 0;
                CurrentPlayer.Move(Dice.LastTotal, config.boardSize, awardSalary: true, config.salaryOnGo);
                EventBus.RaisePlayerMoved(CurrentPlayer, CurrentPlayer.BoardPosition);
                ResolveCurrentTile();
                return;
            }

            CurrentPlayer.JailTurnsRemaining--;
            if (CurrentPlayer.JailTurnsRemaining <= 0)
            {
                if (CurrentPlayer.TryPay(config.jailFine))
                {
                    EventBus.RaiseMoneyChanged(CurrentPlayer, -config.jailFine);
                }
                CurrentPlayer.InJail = false;
                CurrentPlayer.Move(Dice.LastTotal, config.boardSize, awardSalary: true, config.salaryOnGo);
                EventBus.RaisePlayerMoved(CurrentPlayer, CurrentPlayer.BoardPosition);
                ResolveCurrentTile();
            }
        }

        private void SendCurrentPlayerToJail()
        {
            CurrentPlayer.InJail = true;
            CurrentPlayer.JailTurnsRemaining = config.maxJailTurns;
            CurrentPlayer.TeleportTo(config.jailTilePosition, config.boardSize, 0);
            EventBus.RaiseSentToJail(CurrentPlayer);
        }

        private void CheckBankruptcy(Player p)
        {
            if (p.IsBankrupt) return;
            if (p.Money < 0 || (p.Money == 0 && p.LiquidationValue() <= 0))
            {
                p.DeclareBankrupt();
                EventBus.RaisePlayerBankrupt(p);
            }
        }

        private void AdvanceToNextLivePlayer()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count;
                if (!CurrentPlayer.IsBankrupt) return;
            }
        }

        private bool TryDetectGameOver(out Player winner)
        {
            int alive = 0;
            Player last = null;
            foreach (var p in Players)
            {
                if (!p.IsBankrupt) { alive++; last = p; }
            }
            if (alive <= 1)
            {
                winner = last;
                return true;
            }

            if (config.targetWinScore > 0)
            {
                foreach (var p in Players)
                {
                    if (!p.IsBankrupt && p.LiquidationValue() >= config.targetWinScore)
                    {
                        winner = p;
                        return true;
                    }
                }
            }

            winner = null;
            return false;
        }
    }
}
