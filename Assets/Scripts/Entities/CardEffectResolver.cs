using UMMonopoly.Data;

namespace UMMonopoly.Entities
{
    public static class CardEffectResolver
    {
        public static void Apply(Card card, Player player, GameContext ctx)
        {
            switch (card.Effect)
            {
                case CardEffectType.CollectMoney:
                    ctx.Bank.Award(player, card.Amount);
                    break;

                case CardEffectType.PayMoney:
                    ctx.Bank.CollectTax(player, card.Amount);
                    break;

                case CardEffectType.MoveToTile:
                    player.TeleportTo(card.Amount, ctx.Config.boardSize, ctx.Config.salaryOnGo);
                    EventBus.RaisePlayerMoved(player, player.BoardPosition);
                    ctx.Board.GetTile(player.BoardPosition).OnPlayerLanded(player, ctx);
                    break;

                case CardEffectType.MoveSteps:
                    player.Move(card.Amount, ctx.Config.boardSize, awardSalary: true, ctx.Config.salaryOnGo);
                    EventBus.RaisePlayerMoved(player, player.BoardPosition);
                    ctx.Board.GetTile(player.BoardPosition).OnPlayerLanded(player, ctx);
                    break;

                case CardEffectType.GoToJail:
                    player.InJail = true;
                    player.JailTurnsRemaining = ctx.Config.maxJailTurns;
                    player.TeleportTo(ctx.Config.jailTilePosition, ctx.Config.boardSize, 0);
                    EventBus.RaisePlayerMoved(player, player.BoardPosition);
                    EventBus.RaiseSentToJail(player);
                    break;

                case CardEffectType.GetOutOfJailFree:
                    player.GetOutOfJailCards++;
                    break;

                case CardEffectType.CollectFromEachPlayer:
                    foreach (var other in ctx.Players)
                    {
                        if (other == player || other.IsBankrupt) continue;
                        ctx.Bank.PayRent(other, player, card.Amount);
                    }
                    break;

                case CardEffectType.PayEachPlayer:
                    foreach (var other in ctx.Players)
                    {
                        if (other == player || other.IsBankrupt) continue;
                        ctx.Bank.PayRent(player, other, card.Amount);
                    }
                    break;

                case CardEffectType.PayPerProperty:
                    int total = player.OwnedAssetCount() * card.Amount;
                    ctx.Bank.CollectTax(player, total);
                    break;
            }
        }
    }
}
