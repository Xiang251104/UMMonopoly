using UMMonopoly.Entities;

namespace UMMonopoly.Systems
{
    public class Bank
    {
        public int LastDiceTotal { get; set; }

        // NOTE: Player.TryPay and Player.Receive now raise OnMoneyChanged themselves,
        // so the Bank methods below do NOT raise it explicitly (would double-fire).

        public bool BuyProperty(Player buyer, PropertyTile prop)
        {
            if (prop.Owner != null) return false;
            if (!buyer.TryPay(prop.Data.purchasePrice)) return false;
            prop.Owner = buyer;
            buyer.OwnedProperties.Add(prop);
            EventBus.RaisePropertyBought(buyer, prop);
            EventBus.RaiseTilePurchased(buyer, prop.Data);
            return true;
        }

        public bool BuyStation(Player buyer, StationTile station)
        {
            if (station.Owner != null) return false;
            if (!buyer.TryPay(station.Data.purchasePrice)) return false;
            station.Owner = buyer;
            buyer.OwnedStations.Add(station);
            EventBus.RaiseTilePurchased(buyer, station.Data);
            return true;
        }

        public bool BuyUtility(Player buyer, UtilityTile util)
        {
            if (util.Owner != null) return false;
            if (!buyer.TryPay(util.Data.purchasePrice)) return false;
            util.Owner = buyer;
            buyer.OwnedUtilities.Add(util);
            EventBus.RaiseTilePurchased(buyer, util.Data);
            return true;
        }

        public bool UpgradeProperty(PropertyTile prop, Board board)
        {
            if (!prop.CanUpgrade(board)) return false;
            if (!prop.Owner.TryPay(prop.Data.upgradeCost)) return false;
            prop.UpgradeLevel++;
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
            EventBus.RaiseRentPaid(from, to, paid);
        }

        public void CollectTax(Player from, int amount)
        {
            if (amount <= 0) return;
            if (!from.TryPay(amount))
            {
                from.TryPay(from.Money);    // pay what you can
            }
        }

        public void Award(Player to, int amount)
        {
            if (amount <= 0) return;
            to.Receive(amount);
        }

        // ── Trading ────────────────────────────────────────────────────────────

        /// <summary>
        /// Validates and executes a tile + cash swap between two players.
        /// Returns false if either side doesn't actually own what they're offering
        /// or can't afford the net cash they owe.
        /// </summary>
        public bool ExecuteTrade(TradeOffer offer)
        {
            if (offer == null || offer.From == null || offer.To == null) return false;
            if (offer.From == offer.To) return false;
            if (offer.FromCash < 0 || offer.ToCash < 0) return false;

            // Ownership check
            foreach (var t in offer.FromGives) if (!OwnsTile(offer.From, t)) return false;
            foreach (var t in offer.ToGives)   if (!OwnsTile(offer.To,   t)) return false;

            // Net cash flow: positive = From pays net, negative = To pays net
            int netFromPays = offer.FromCash - offer.ToCash;
            if (netFromPays > 0 && offer.From.Money < netFromPays) return false;
            if (netFromPays < 0 && offer.To.Money   < -netFromPays) return false;

            // Cash transfer (single net payment)
            if (netFromPays > 0)
            {
                if (!offer.From.TryPay(netFromPays)) return false;
                offer.To.Receive(netFromPays);
            }
            else if (netFromPays < 0)
            {
                int amount = -netFromPays;
                if (!offer.To.TryPay(amount)) return false;
                offer.From.Receive(amount);
            }

            // Tile transfer
            foreach (var t in offer.FromGives) TransferTile(t, offer.From, offer.To);
            foreach (var t in offer.ToGives)   TransferTile(t, offer.To,   offer.From);

            EventBus.RaiseTradeCompleted(offer);
            return true;
        }

        private bool OwnsTile(Player p, Tile tile)
        {
            if (tile is PropertyTile pp) return p.OwnedProperties.Contains(pp);
            if (tile is StationTile  s)  return p.OwnedStations.Contains(s);
            if (tile is UtilityTile  u)  return p.OwnedUtilities.Contains(u);
            return false;
        }

        private void TransferTile(Tile tile, Player from, Player to)
        {
            if (tile is PropertyTile pp)
            {
                pp.Owner = to;
                from.OwnedProperties.Remove(pp);
                to.OwnedProperties.Add(pp);
            }
            else if (tile is StationTile s)
            {
                s.Owner = to;
                from.OwnedStations.Remove(s);
                to.OwnedStations.Add(s);
            }
            else if (tile is UtilityTile u)
            {
                u.Owner = to;
                from.OwnedUtilities.Remove(u);
                to.OwnedUtilities.Add(u);
            }
        }
    }
}
