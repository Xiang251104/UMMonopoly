using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UMMonopoly.Core;

namespace UMMonopoly.UI
{
    /// <summary>
    /// Drives the two-page main menu:
    ///   Page 1 — title + Play + Quit (will hold the AI background image)
    ///   Page 2 — player name entry + Back + Start Game
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Pages")]
        public GameObject page1Panel;
        public GameObject page2Panel;

        [Header("Page 2 — Player Inputs")]
        public TMP_InputField[] playerNameInputs = new TMP_InputField[4];

        [Header("Scene to load on Start")]
        public string gameSceneName = "GameBoard";

        private void Start()
        {
            ShowPage1();
        }

        public void ShowPage1()
        {
            if (page1Panel != null) page1Panel.SetActive(true);
            if (page2Panel != null) page2Panel.SetActive(false);
        }

        public void ShowPage2()
        {
            if (page1Panel != null) page1Panel.SetActive(false);
            if (page2Panel != null) page2Panel.SetActive(true);
        }

        public void StartNewGame()
        {
            var names = new List<string>();
            for (int i = 0; i < playerNameInputs.Length; i++)
            {
                var input = playerNameInputs[i];
                if (input == null) continue;
                var t = input.text;
                if (!string.IsNullOrWhiteSpace(t)) names.Add(t.Trim());
            }
            if (names.Count < 2) names = new List<string> { "Player 1", "Player 2" };

            GameBootstrap.PendingPlayerNames = names;
            SceneManager.LoadScene(gameSceneName);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
