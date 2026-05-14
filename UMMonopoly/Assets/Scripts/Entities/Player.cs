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
        public bool InJail { get; set; }
        public int JailTurnsRemaining { get; set; }
        public bool IsBankrupt { get; private set; }
        public int GetOutOfJailCards { get; set; }

        public Player(int id, string name, int startingMoney)
        {
            Id = id;
            Name = name;
            Money = startingMoney;
            BoardPosition = 0;
        }

        public void Move(int steps, int boardSize, bool awardSalary, int salary)
        {
            int newPos = (BoardPosition + steps) % boardSize;
            if (awardSalary && newPos < BoardPosition && steps > 0)
            {
                Receive(salary);
            }
            BoardPosition = newPos;
        }

        public void TeleportTo(int position, int boardSize, int salary)
        {
            if (position < BoardPosition)
            {
                Receive(salary);
            }
            BoardPosition = position;
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

        public int LiquidationValue()
        {
            int total = Money;
            foreach (var p in OwnedProperties)
            {
                total += p.Data.purchasePrice / 2;
                total += p.UpgradeLevel * (p.Data.upgradeCost / 2);
            }
            return total;
        }

        public void DeclareBankrupt()
        {
            IsBankrupt = true;
            Money = 0;
            foreach (var p in OwnedProperties)
            {
                p.Owner = null;
                p.UpgradeLevel = 0;
            }
            OwnedProperties.Clear();
        }
    }
}
