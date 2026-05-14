using UMMonopoly.Data;

namespace UMMonopoly.Entities
{
    public class FreeParkingTile : Tile
    {
        public FreeParkingTile(TileDataSO data) : base(data) { }

        public override void OnPlayerLanded(Player player, GameContext ctx)
        {
            // Sunken Garden — pure rest stop. Optional: collect kitty pool of taxes.
            // MVP: no effect.
        }
    }
}
