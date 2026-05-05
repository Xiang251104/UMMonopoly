using UMMonopoly.Data;

namespace UMMonopoly.Entities
{
    public class TaxTile : Tile
    {
        public TaxTile(TileDataSO data) : base(data) { }

        public override void OnPlayerLanded(Player player, GameContext ctx)
        {
            ctx.Bank.CollectTax(player, Data.taxAmount);
        }
    }
}
