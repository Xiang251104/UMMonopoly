using UMMonopoly.Data;

namespace UMMonopoly.Entities
{
    public class CardTile : Tile
    {
        public CardTile(TileDataSO data) : base(data) { }

        public override void OnPlayerLanded(Player player, GameContext ctx)
        {
            var deck = Data.deckType == CardDeckType.Akademik ? ctx.AkademikDeck : ctx.KampusDeck;
            var card = deck.Draw();
            EventBus.RaiseCardDrawn(player, card);
            CardEffectResolver.Apply(card, player, ctx);
        }
    }
}
