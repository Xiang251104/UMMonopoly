using UMMonopoly.Data;

namespace UMMonopoly.Entities
{
    public class GoTile : Tile
    {
        public GoTile(TileDataSO data) : base(data) { }

        public override void OnPlayerLanded(Player player, GameContext ctx)
        {
            // Salary is awarded on Move() crossing GO; landing exactly on GO already triggered it.
            // Nothing else to do.
        }
    }
}
