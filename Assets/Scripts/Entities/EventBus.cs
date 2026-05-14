using System;

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
        public static event Action<Player, Tile> OnTileResolved;
        public static event Action<Player> OnPlayerBankrupt;
        public static event Action<Player> OnGameWon;
        public static event Action<int> OnTurnStarted;                    // player index
        public static event Action<int> OnTurnEnded;

        public static void RaiseDiceRolled(Player p, int total) => OnDiceRolled?.Invoke(p, total);
        public static void RaisePlayerMoved(Player p, int pos) => OnPlayerMoved?.Invoke(p, pos);
        public static void RaiseMoneyChanged(Player p, int delta) => OnMoneyChanged?.Invoke(p, delta);
        public static void RaiseCardDrawn(Player p, Card c) => OnCardDrawn?.Invoke(p, c);
        public static void RaisePropertyBought(Player p, PropertyTile t) => OnPropertyBought?.Invoke(p, t);
        public static void RaisePropertyUpgraded(Player p, PropertyTile t) => OnPropertyUpgraded?.Invoke(p, t);
        public static void RaiseRentPaid(Player from, Player to, int amount) => OnRentPaid?.Invoke(from, to, amount);
        public static void RaiseSentToJail(Player p) => OnSentToJail?.Invoke(p);
        public static void RaiseTileResolved(Player p, Tile t) => OnTileResolved?.Invoke(p, t);
        public static void RaisePlayerBankrupt(Player p) => OnPlayerBankrupt?.Invoke(p);
        public static void RaiseGameWon(Player p) => OnGameWon?.Invoke(p);
        public static void RaiseTurnStarted(int idx) => OnTurnStarted?.Invoke(idx);
        public static void RaiseTurnEnded(int idx) => OnTurnEnded?.Invoke(idx);
    }
}
