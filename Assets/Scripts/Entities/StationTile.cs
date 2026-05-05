using UMMonopoly.Data;

namespace UMMonopoly.Entities
{
    public class StationTile : Tile
    {
        public Player Owner { get; set; }

        public StationTile(TileDataSO data) : base(data) { }

        public int CurrentRent(Board board)
        {
            if (Owner == null) return 0;
            int owned = 0;
            foreach (var t in board.AllStations())
            {
                if (t.Owner == Owner) owned++;
            }
            int idx = owned - 1;
            if (Data.rentTable == null || Data.rentTable.Length == 0) return 0;
            if (idx < 0) idx = 0;
            if (idx >= Data.rentTable.Length) idx = Data.rentTable.Length - 1;
            return Data.rentTable[idx];
        }

        public override void OnPlayerLanded(Player player, GameContext ctx)
        {
            if (Owner == null || Owner == player) return;
            ctx.Bank.PayRent(player, Owner, CurrentRent(ctx.Board));
        }
    }
}
