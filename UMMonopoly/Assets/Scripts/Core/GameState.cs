namespace UMMonopoly.Core
{
    public enum GameState
    {
        MainMenu,
        Setup,
        TurnStart,
        RollPhase,
        MovePhase,
        ResolveTilePhase,
        DecisionPhase,    // buy property, upgrade, end turn
        EndTurnPhase,
        GameOver
    }
}
