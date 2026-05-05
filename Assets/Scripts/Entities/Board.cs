using System.Collections.Generic;
using UMMonopoly.Data;

namespace UMMonopoly.Entities
{
    public class Board
    {
        public List<Tile> Tiles { get; }

        public Board(List<TileDataSO> tileData)
        {
            Tiles = new List<Tile>(tileData.Count);
            foreach (var data in tileData)
            {
                Tiles.Add(BuildTile(data));
            }
        }

        public Tile GetTile(int position) => Tiles[position];

        public IEnumerable<PropertyTile> AllPropertiesInGroup(ColorGroup group)
        {
            foreach (var t in Tiles)
            {
                if (t is PropertyTile p && p.Data.colorGroup == group)
                    yield return p;
            }
        }

        public IEnumerable<StationTile> AllStations()
        {
            foreach (var t in Tiles)
                if (t is StationTile s) yield return s;
        }

        public IEnumerable<UtilityTile> AllUtilities()
        {
            foreach (var t in Tiles)
                if (t is UtilityTile u) yield return u;
        }

        private static Tile BuildTile(TileDataSO data)
        {
            switch (data.type)
            {
                case TileType.Property: return new PropertyTile(data);
                case TileType.Station: return new StationTile(data);
                case TileType.Utility: return new UtilityTile(data);
                case TileType.Tax: return new TaxTile(data);
                case TileType.Card: return new CardTile(data);
                case TileType.Jail: return new JailTile(data);
                case TileType.FreeParking: return new FreeParkingTile(data);
                case TileType.GoToJail: return new GoToJailTile(data);
                case TileType.Go: return new GoTile(data);
                default: return new GoTile(data);
            }
        }
    }
}
