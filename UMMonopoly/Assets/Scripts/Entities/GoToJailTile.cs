using UMMonopoly.Data;

namespace UMMonopoly.Entities
{
    public class GoToJailTile : Tile
    {
        public GoToJailTile(TileDataSO data) : base(data) { }

        public override void OnPlayerLanded(Player player, GameContext ctx)
        {
            player.InJail = true;
            player.JailTurnsRemaining = ctx.Config.maxJailTurns;
            // No salary on direct teleport to jail.
            player.TeleportTo(ctx.Config.jailTilePosition, ctx.Config.boardSize, 0);
            EventBus.RaiseSentToJail(player);
        }
    }
}
