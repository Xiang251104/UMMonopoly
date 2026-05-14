using UMMonopoly.Data;

namespace UMMonopoly.Entities
{
    public class UtilityTile : Tile
    {
        public Player Owner { get; set; }

        public UtilityTile(TileDataSO data) : base(data) { }

        public int CurrentRent(Board board, int diceTotal)
        {
            if (Owner == null) return 0;
            int owned = 0;
            foreach (var t in board.AllUtilities())
            {
                if (t.Owner == Owner) owned++;
            }
            int multiplier = owned == 1 ? 4 : 10;
            return diceTotal * multiplier;
        }

        public override void OnPlayerLanded(Player player, GameContext ctx)
        {
            if (Owner == null || Owner == player) return;
            // Dice total is supplied via the most recent roll, accessible from the bank/turn ctx.
            int rent = CurrentRent(ctx.Board, ctx.Bank.LastDiceTotal);
            ctx.Bank.PayRent(player, Owner, rent);
        }
    }
}
