using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UMMonopoly.Core;

namespace UMMonopoly.UI
{
    public class MainMenuController : MonoBehaviour
    {
        public TMP_InputField[] playerNameInputs = new TMP_InputField[4];
        public string gameSceneName = "GameBoard";

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
