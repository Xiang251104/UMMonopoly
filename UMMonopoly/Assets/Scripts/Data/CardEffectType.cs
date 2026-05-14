namespace UMMonopoly.Data
{
    public enum CardEffectType
    {
        CollectMoney,        // gain X
        PayMoney,            // lose X
        MoveToTile,          // move to tile index
        MoveSteps,           // move +/- N tiles
        GoToJail,
        GetOutOfJailFree,
        CollectFromEachPlayer,
        PayEachPlayer,
        PayPerProperty       // pay X per owned property
    }
}
