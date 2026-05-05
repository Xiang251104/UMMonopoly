using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UMMonopoly.Entities;

namespace UMMonopoly.UI
{
    public class CardPopup : MonoBehaviour
    {
        public GameObject panelRoot;
        public TMP_Text deckLabel;
        public TMP_Text descriptionLabel;
        public Button closeButton;

        private void OnEnable()
        {
            EventBus.OnCardDrawn += Show;
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
            Hide();
        }

        private void OnDisable()
        {
            EventBus.OnCardDrawn -= Show;
            if (closeButton != null) closeButton.onClick.RemoveListener(Hide);
        }

        private void Show(Player p, Card c)
        {
            if (panelRoot != null) panelRoot.SetActive(true);
            if (deckLabel != null) deckLabel.text = c.Data.deck.ToString() + " Card";
            if (descriptionLabel != null) descriptionLabel.text = c.Description;
        }

        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }
    }
}
