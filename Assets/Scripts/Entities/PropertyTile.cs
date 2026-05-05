using UMMonopoly.Data;

namespace UMMonopoly.Entities
{
    public class PropertyTile : Tile
    {
        public Player Owner { get; set; }
        public int UpgradeLevel { get; set; }     // 0 = base, 4 = Postgrad Centre

        public PropertyTile(TileDataSO data) : base(data) { }

        public int CurrentRent(Board board)
        {
            if (Data.rentTable == null || Data.rentTable.Length == 0) return 0;
            int idx = UpgradeLevel;
            if (idx >= Data.rentTable.Length) idx = Data.rentTable.Length - 1;

            int rent = Data.rentTable[idx];

            // Monopoly bonus: double base rent if owner has full color set and no upgrades yet
            if (UpgradeLevel == 0 && Owner != null && OwnsFullSet(board))
            {
                rent *= 2;
            }
            return rent;
        }

        public bool OwnsFullSet(Board board)
        {
            if (Owner == null || Data.colorGroup == ColorGroup.None) return false;
            foreach (var t in board.AllPropertiesInGroup(Data.colorGroup))
            {
                if (t.Owner != Owner) return false;
            }
            return true;
        }

        public bool CanUpgrade(Board board)
        {
            return Owner != null
                && OwnsFullSet(board)
                && UpgradeLevel < 4
                && Owner.Money >= Data.upgradeCost;
        }

        public override void OnPlayerLanded(Player player, GameContext ctx)
        {
            if (Owner == null)
            {
                // Decision deferred to UI/TurnController via game state
                return;
            }
            if (Owner == player) return;

            int rent = CurrentRent(ctx.Board);
            ctx.Bank.PayRent(player, Owner, rent);
        }
    }
}
