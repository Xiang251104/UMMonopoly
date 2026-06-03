using System;
using UnityEngine;

namespace UMMonopoly.Entities
{
    public static class EventBus
    {
        public static event Action<Player, int> OnDiceRolled;             // player, total
        public static event Action<Player, int> OnPlayerMoved;            // player, new position
        public static event Action<Player, int> OnMoneyChanged;           // player, delta
        public static event Action<Player, Card> OnCardDrawn;
        public static event Action<Player, PropertyTile> OnPropertyBought;
        public static event Action<Player, PropertyTile> OnPropertyUpgraded;
        public static event Action<Player, Player, int> OnRentPaid;       // from, to, amount
        public static event Action<Player> OnSentToJail;
        public static event Action<Player> OnPlayerBankrupt;
        public static event Action<Player> OnGameWon;
        public static event Action<int> OnTurnStarted;                    // player index
        public static event Action<int> OnTurnEnded;
        public static event Action OnTileResolved;                        // fired after DecisionPhase is set
        public static event Action<Vector3> OnDiceRollStarted;            // world-space midpoint of dice for camera focus
        public static event Action OnDiceRollEnded;                       // fired after dice anim + pauseAfterRoll
        public static event Action<Player, UMMonopoly.Data.TileDataSO> OnTilePurchased; // any tile bought (property/station/utility)
        public static event Action<Player, int> OnPassedGo;                // player, salary amount — fired whenever a player crosses GO
        public static event Action<TradeOffer>  OnTradeCompleted;          // fired after Bank.ExecuteTrade succeeds

        public static void RaiseDiceRolled(Player p, int total) => OnDiceRolled?.Invoke(p, total);
        public static void RaisePlayerMoved(Player p, int pos) => OnPlayerMoved?.Invoke(p, pos);
        public static void RaiseMoneyChanged(Player p, int delta) => OnMoneyChanged?.Invoke(p, delta);
        public static void RaiseCardDrawn(Player p, Card c) => OnCardDrawn?.Invoke(p, c);
        public static void RaisePropertyBought(Player p, PropertyTile t) => OnPropertyBought?.Invoke(p, t);
        public static void RaisePropertyUpgraded(Player p, PropertyTile t) => OnPropertyUpgraded?.Invoke(p, t);
        public static void RaiseRentPaid(Player from, Player to, int amount) => OnRentPaid?.Invoke(from, to, amount);
        public static void RaiseSentToJail(Player p) => OnSentToJail?.Invoke(p);
        public static void RaisePlayerBankrupt(Player p) => OnPlayerBankrupt?.Invoke(p);
        public static void RaiseGameWon(Player p) => OnGameWon?.Invoke(p);
        public static void RaiseTurnStarted(int idx) => OnTurnStarted?.Invoke(idx);
        public static void RaiseTurnEnded(int idx) => OnTurnEnded?.Invoke(idx);
        public static void RaiseTileResolved() => OnTileResolved?.Invoke();
        public static void RaiseDiceRollStarted(Vector3 center) => OnDiceRollStarted?.Invoke(center);
        public static void RaiseDiceRollEnded() => OnDiceRollEnded?.Invoke();
        public static void RaiseTilePurchased(Player p, UMMonopoly.Data.TileDataSO t) => OnTilePurchased?.Invoke(p, t);
        public static void RaisePassedGo(Player p, int salary) => OnPassedGo?.Invoke(p, salary);
        public static void RaiseTradeCompleted(TradeOffer t) => OnTradeCompleted?.Invoke(t);
    }
}
