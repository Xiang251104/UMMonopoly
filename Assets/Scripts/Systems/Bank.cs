using UMMonopoly.Entities;

namespace UMMonopoly.Systems
{
    public class Bank
    {
        public int LastDiceTotal { get; set; }

        public bool BuyProperty(Player buyer, PropertyTile prop)
        {
            if (prop.Owner != null) return false;
            if (!buyer.TryPay(prop.Data.purchasePrice)) return false;
            prop.Owner = buyer;
            buyer.OwnedProperties.Add(prop);
            EventBus.RaiseMoneyChanged(buyer, -prop.Data.purchasePrice);
            EventBus.RaisePropertyBought(buyer, prop);
            return true;
        }

        public bool BuyStation(Player buyer, StationTile station)
        {
            if (station.Owner != null) return false;
            if (!buyer.TryPay(station.Data.purchasePrice)) return false;
            station.Owner = buyer;
            EventBus.RaiseMoneyChanged(buyer, -station.Data.purchasePrice);
            return true;
        }

        public bool BuyUtility(Player buyer, UtilityTile util)
        {
            if (util.Owner != null) return false;
            if (!buyer.TryPay(util.Data.purchasePrice)) return false;
            util.Owner = buyer;
            EventBus.RaiseMoneyChanged(buyer, -util.Data.purchasePrice);
            return true;
        }

        public bool UpgradeProperty(PropertyTile prop, Board board)
        {
            if (!prop.CanUpgrade(board)) return false;
            if (!prop.Owner.TryPay(prop.Data.upgradeCost)) return false;
            prop.UpgradeLevel++;
            EventBus.RaiseMoneyChanged(prop.Owner, -prop.Data.upgradeCost);
            EventBus.RaisePropertyUpgraded(prop.Owner, prop);
            return true;
        }

        public void PayRent(Player from, Player to, int amount)
        {
            if (amount <= 0 || from == to) return;
            int paid = amount;
            if (!from.TryPay(amount))
            {
                paid = from.Money;          // pay what you can; bankruptcy handled by GameManager
                from.TryPay(paid);
            }
            to.Receive(paid);
            EventBus.RaiseMoneyChanged(from, -paid);
            EventBus.RaiseMoneyChanged(to, paid);
            EventBus.RaiseRentPaid(from, to, paid);
        }

        public void CollectTax(Player from, int amount)
        {
            if (amount <= 0) return;
            int paid = amount;
            if (!from.TryPay(amount))
            {
                paid = from.Money;
                from.TryPay(paid);
            }
            EventBus.RaiseMoneyChanged(from, -paid);
        }

        public void Award(Player to, int amount)
        {
            if (amount <= 0) return;
            to.Receive(amount);
            EventBus.RaiseMoneyChanged(to, amount);
        }
    }
}
