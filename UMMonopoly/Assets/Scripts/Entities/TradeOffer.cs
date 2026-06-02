using System.Collections.Generic;

namespace UMMonopoly.Entities
{
    /// <summary>
    /// A proposed swap of tiles + cash between two players.
    /// Built by TradeModal, executed by Bank.ExecuteTrade.
    /// </summary>
    public class TradeOffer
    {
        public Player From;
        public Player To;
        public List<Tile> FromGives = new List<Tile>();
        public List<Tile> ToGives   = new List<Tile>();
        public int FromCash;   // cash the From player adds to the deal
        public int ToCash;     // cash the To player adds to the deal
    }
}
