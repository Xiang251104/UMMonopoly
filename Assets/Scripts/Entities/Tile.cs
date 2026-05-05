using UMMonopoly.Data;

namespace UMMonopoly.Entities
{
    public abstract class Tile
    {
        public TileDataSO Data { get; }
        public int Position => Data.position;
        public string Name => Data.tileName;

        protected Tile(TileDataSO data)
        {
            Data = data;
        }

        public abstract void OnPlayerLanded(Player player, GameContext ctx);
    }
}
