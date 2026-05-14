using UMMonopoly.Data;

namespace UMMonopoly.Entities
{
    public class JailTile : Tile
    {
        public JailTile(TileDataSO data) : base(data) { }

        public override void OnPlayerLanded(Player player, GameContext ctx)
        {
            // Just visiting; no effect. Send-to-jail is handled by GoToJailTile or cards.
        }
    }
}
