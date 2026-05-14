using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UMMonopoly.Core;
using UMMonopoly.Entities;

namespace UMMonopoly.UI
{
    public class PropertyPopup : MonoBehaviour
    {
        public GameObject panelRoot;
        public TMP_Text nameLabel;
        public TMP_Text priceLabel;
        public TMP_Text rentLabel;
        public TMP_Text ownerLabel;
        public Button buyButton;
        public Button upgradeButton;
        public Button closeButton;

        private PropertyTile _shown;

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
            if (buyButton != null) buyButton.onClick.AddListener(OnBuy);
            if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgrade);
            Hide();
        }

        public void Show(PropertyTile tile)
        {
            _shown = tile;
            if (panelRoot != null) panelRoot.SetActive(true);
            if (nameLabel != null) nameLabel.text = tile.Name;
            if (priceLabel != null) priceLabel.text = $"Price: RM {tile.Data.purchasePrice}";
            if (rentLabel != null)
            {
                int rent = tile.CurrentRent(GameManager.Instance.Board);
                rentLabel.text = $"Rent: RM {rent} (lvl {tile.UpgradeLevel})";
            }
            if (ownerLabel != null)
                ownerLabel.text = tile.Owner == null ? "Unowned" : $"Owner: {tile.Owner.Name}";

            var gm = GameManager.Instance;
            if (buyButton != null)
                buyButton.interactable = tile.Owner == null && gm.CurrentPlayer.Money >= tile.Data.purchasePrice;
            if (upgradeButton != null)
                upgradeButton.interactable = tile.Owner == gm.CurrentPlayer && tile.CanUpgrade(gm.Board);
        }

        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            _shown = null;
        }

        private void OnBuy()
        {
            if (_shown == null) return;
            GameManager.Instance.Bank.BuyProperty(GameManager.Instance.CurrentPlayer, _shown);
            Show(_shown);
        }

        private void OnUpgrade()
        {
            if (_shown == null) return;
            GameManager.Instance.TryUpgrade(_shown);
            Show(_shown);
        }
    }
}
