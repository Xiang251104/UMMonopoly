using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UMMonopoly.Entities;

namespace UMMonopoly.UI
{
    public class EndGameController : MonoBehaviour
    {
        public GameObject panelRoot;
        public TMP_Text winnerLabel;
        public string mainMenuSceneName = "MainMenu";

        private void OnEnable()
        {
            EventBus.OnGameWon += Show;
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void OnDisable()
        {
            EventBus.OnGameWon -= Show;
        }

        private void Show(Player winner)
        {
            if (panelRoot != null) panelRoot.SetActive(true);
            if (winnerLabel != null)
                winnerLabel.text = winner == null ? "Draw" : $"Congratulations, {winner.Name}!\nYou graduated.";
        }

        public void OnReturnToMenu() => SceneManager.LoadScene(mainMenuSceneName);
    }
}
