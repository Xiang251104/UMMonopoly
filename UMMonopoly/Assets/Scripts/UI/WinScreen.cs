using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UMMonopoly.Entities;

namespace UMMonopoly.UI
{
    /// <summary>
    /// Full-screen win celebration shown when one player remains after others go bankrupt.
    /// Triggered by HUDController.HandleGameWon → Show(winner).
    /// Play Again button reloads the current scene.
    /// </summary>
    public class WinScreen : MonoBehaviour
    {
        [Header("Panel")]
        public GameObject panelRoot;
        public CanvasGroup canvasGroup;
        public RectTransform card;

        [Header("Labels")]
        public Image colorTag;
        public TMP_Text winnerNameLabel;
        public TMP_Text moneyLabel;
        public TMP_Text propertyCountLabel;

        [Header("Buttons")]
        public Button playAgainButton;

        [Header("Animation")]
        public float fadeInDuration = 0.5f;
        public float startScale = 0.8f;

        public Color[] playerColors =
        {
            Color.red,
            Color.blue,
            Color.green,
            new Color(1f, 0.92156863f, 0.015686275f),
        };

        private void OnEnable()
        {
            if (playAgainButton != null) playAgainButton.onClick.AddListener(OnPlayAgainPressed);
            Hide();
        }

        private void OnDisable()
        {
            if (playAgainButton != null) playAgainButton.onClick.RemoveListener(OnPlayAgainPressed);
        }

        public void Show(Player winner)
        {
            if (winner == null || panelRoot == null) return;

            panelRoot.SetActive(true);

            if (winnerNameLabel != null) winnerNameLabel.text = winner.Name;
            if (moneyLabel      != null) moneyLabel.text      = $"RM {winner.Money}";
            if (propertyCountLabel != null)
            {
                int n = winner.TotalOwnedCount;
                propertyCountLabel.text = n == 1 ? "1 Property" : $"{n} Properties";
            }
            if (colorTag != null && winner.Id >= 0 && winner.Id < playerColors.Length)
                colorTag.color = playerColors[winner.Id];

            StartCoroutine(AnimateIn());
        }

        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (card != null) card.localScale = Vector3.one;
        }

        private void OnPlayAgainPressed()
        {
            // Reload the current scene from scratch — easiest way to reset all state.
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private IEnumerator AnimateIn()
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;

            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / fadeInDuration));
                if (canvasGroup != null) canvasGroup.alpha = t;
                if (card != null)        card.localScale  = Vector3.one * Mathf.Lerp(startScale, 1f, t);
                yield return null;
            }
            if (canvasGroup != null) canvasGroup.alpha = 1f;
            if (card != null)        card.localScale  = Vector3.one;
        }
    }
}
