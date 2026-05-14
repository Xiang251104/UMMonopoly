using System.Collections.Generic;
using UnityEngine;
using UMMonopoly.UI;

namespace UMMonopoly.Core
{
    /// <summary>
    /// Carries player setup info from the Main Menu scene to the Game scene,
    /// then kicks off the game once the GameBoard scene has loaded.
    /// Place this on a GameObject in the GameBoard scene.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        public static List<string> PendingPlayerNames;

        public GameManager gameManager;
        public HUDController hud;
        public BoardView boardView;

        private void Start()
        {
            var names = PendingPlayerNames ?? new List<string> { "Player 1", "Player 2" };

            if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
            if (hud == null) hud = FindAnyObjectByType<HUDController>();
            if (boardView == null) boardView = FindAnyObjectByType<BoardView>();

            gameManager.StartGame(names);
            if (hud != null) hud.BuildPlayerCards(gameManager.Players);
            if (boardView != null) boardView.SpawnTokens(gameManager.Players);
        }
    }
}
