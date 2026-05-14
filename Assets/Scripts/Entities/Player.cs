using System.Collections.Generic;

namespace UMMonopoly.Entities
{
    public class Player
    {
        public int Id { get; }
        public string Name { get; set; }
        public int Money { get; private set; }
        public int BoardPosition { get; private set; }
        public List<PropertyTile> OwnedProperties { get; } = new List<PropertyTile>();
        public List<StationTile> OwnedStations { get; } = new List<StationTile>();
        public List<UtilityTile> OwnedUtilities { get; } = new List<UtilityTile>();
        public bool InJail { get; set; }
        public int JailTurnsRemaining { get; set; }
        public bool IsBankrupt { get; private set; }
        public int GetOutOfJailCards { get; set; }
        public bool HasUnpaidDebt { get; private set; }

        public Player(int id, string name, int startingMoney)
        {
            Id = id;
            Name = name;
            Money = startingMoney;
            BoardPosition = 0;
        }

        public void Move(int steps, int boardSize, bool awardSalary, int salary)
        {
            if (boardSize <= 0) return;
            int rawPosition = BoardPosition + steps;
            int newPos = ((rawPosition % boardSize) + boardSize) % boardSize;
            if (awardSalary && steps > 0 && rawPosition >= boardSize)
            {
                Receive(salary);
            }
            BoardPosition = newPos;
        }

        public void TeleportTo(int position, int boardSize, int salary)
        {
            if (boardSize <= 0) return;
            int newPosition = ((position % boardSize) + boardSize) % boardSize;
            if (salary > 0 && newPosition < BoardPosition)
            {
                Receive(salary);
            }
            BoardPosition = newPosition;
        }

        public bool TryPay(int amount)
        {
            if (amount <= 0) return true;
            if (Money < amount)
            {
                return false;
            }
            Money -= amount;
            return true;
        }

        public void Receive(int amount)
        {
            if (amount <= 0) return;
            Money += amount;
        }

        public void MarkUnpaidDebt()
        {
            HasUnpaidDebt = true;
        }

        public void ClearUnpaidDebt()
        {
            HasUnpaidDebt = false;
        }

        public int LiquidationValue()
        {
            int total = Money;
            foreach (var p in OwnedProperties)
            {
                total += p.Data.purchasePrice / 2;
                total += p.UpgradeLevel * (p.Data.upgradeCost / 2);
            }
            foreach (var s in OwnedStations)
            {
                total += s.Data.purchasePrice / 2;
            }
            foreach (var u in OwnedUtilities)
            {
                total += u.Data.purchasePrice / 2;
            }
            return total;
        }

        public int OwnedAssetCount()
        {
            return OwnedProperties.Count + OwnedStations.Count + OwnedUtilities.Count;
        }

        public void RestoreState(
            string name,
            int money,
            int position,
            bool inJail,
            int jailTurnsRemaining,
            bool isBankrupt,
            int getOutOfJailCards,
            int boardSize)
        {
            Name = name;
            Money = money;
            BoardPosition = boardSize > 0 ? ((position % boardSize) + boardSize) % boardSize : position;
            InJail = inJail;
            JailTurnsRemaining = jailTurnsRemaining;
            IsBankrupt = isBankrupt;
            GetOutOfJailCards = getOutOfJailCards;
            HasUnpaidDebt = false;
            OwnedProperties.Clear();
            OwnedStations.Clear();
            OwnedUtilities.Clear();
        }

        public void DeclareBankrupt()
        {
            IsBankrupt = true;
            Money = 0;
            HasUnpaidDebt = false;
            foreach (var p in OwnedProperties)
            {
                p.Owner = null;
                p.UpgradeLevel = 0;
            }
            foreach (var s in OwnedStations)
            {
                s.Owner = null;
            }
            foreach (var u in OwnedUtilities)
            {
                u.Owner = null;
            }
            OwnedProperties.Clear();
            OwnedStations.Clear();
            OwnedUtilities.Clear();
        }
    }
}
