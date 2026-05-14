using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UMMonopoly.Core;
using UMMonopoly.Data;
using UMMonopoly.Entities;

namespace UMMonopoly.UI
{
    /// <summary>
    /// Popup shown when a player's token lands on a tile.
    /// Displays the tile's location photo (e.g. real photo of FSKTM),
    /// the tile name, description, price, and buy/upgrade options.
    /// </summary>
    public class TileLandingPopup : MonoBehaviour
    {
        [Header("Panel")]
        public GameObject panelRoot;
        public CanvasGroup canvasGroup;
        public float fadeInSeconds = 0.25f;

        [Header("Location Photo")]
        [Tooltip("The large image of the UM location (e.g. photo of FSKTM building).")]
        public Image locationPhoto;
        public GameObject photoPlaceholder; // shown when no photo assigned

        [Header("Info")]
        public TMP_Text tileNameLabel;
        public TMP_Text descriptionLabel;
        public TMP_Text priceLabel;
        public TMP_Text rentLabel;
        public TMP_Text ownerLabel;

        [Header("Buttons")]
        public Button buyButton;
        public Button upgradeButton;
        public Button closeButton;

        private TileDataSO _currentTile;

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
            if (buyButton != null) buyButton.onClick.AddListener(OnBuyPressed);
            if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgradePressed);
            Hide();
        }

        public void Show(TileDataSO tile)
        {
            _currentTile = tile;

            // Only show popup for tiles that have meaningful content
            bool showForType = tile.type == TileType.Property
                            || tile.type == TileType.Station
                            || tile.type == TileType.Utility
                            || tile.type == TileType.Tax
                            || tile.type == TileType.Go;

            if (!showForType) return;

            if (panelRoot != null) panelRoot.SetActive(true);

            // Location photo
            if (tile.locationPhoto != null)
            {
                if (locationPhoto != null)
                {
                    locationPhoto.sprite = tile.locationPhoto;
                    locationPhoto.gameObject.SetActive(true);
                }
                if (photoPlaceholder != null) photoPlaceholder.SetActive(false);
            }
            else
            {
                if (locationPhoto != null) locationPhoto.gameObject.SetActive(false);
                if (photoPlaceholder != null) photoPlaceholder.SetActive(true);
            }

            // Text
            if (tileNameLabel != null) tileNameLabel.text = tile.tileName;
            if (descriptionLabel != null) descriptionLabel.text = tile.locationDescription;

            var gm = GameManager.Instance;
            if (gm == null) return;

            var boardTile = gm.Board.GetTile(tile.position);
            Player owner = null;
            int currentRent = 0;

            if (boardTile is PropertyTile pt)
            {
                owner = pt.Owner;
                currentRent = pt.CurrentRent(gm.Board);
                if (priceLabel != null) priceLabel.text = owner == null
                    ? $"Price: RM {tile.purchasePrice}"
                    : $"Upgrade: RM {tile.upgradeCost}";
                if (rentLabel != null) rentLabel.text = $"Rent: RM {currentRent} (Level {pt.UpgradeLevel})";

                bool isCurrentPlayer = owner == gm.CurrentPlayer;
                if (buyButton != null)
                    buyButton.gameObject.SetActive(owner == null);
                if (upgradeButton != null)
                    upgradeButton.gameObject.SetActive(isCurrentPlayer && pt.CanUpgrade(gm.Board));
                if (buyButton != null)
                    buyButton.interactable = owner == null && gm.CurrentPlayer.Money >= tile.purchasePrice;
            }
            else if (boardTile is StationTile st)
            {
                owner = st.Owner;
                if (priceLabel != null) priceLabel.text = owner == null ? $"Price: RM {tile.purchasePrice}" : "";
                if (rentLabel != null)
                {
                    var rent = st.CurrentRent(gm.Board);
                    rentLabel.text = $"Rent: RM {rent}";
                }
                if (buyButton != null) buyButton.gameObject.SetActive(owner == null);
                if (upgradeButton != null) upgradeButton.gameObject.SetActive(false);
                if (buyButton != null)
                    buyButton.interactable = owner == null && gm.CurrentPlayer.Money >= tile.purchasePrice;
            }
            else if (boardTile is UtilityTile ut)
            {
                owner = ut.Owner;
                if (priceLabel != null) priceLabel.text = owner == null ? $"Price: RM {tile.purchasePrice}" : "";
                if (rentLabel != null) rentLabel.text = "Rent: dice × 4 (or × 10 if both owned)";
                if (buyButton != null) buyButton.gameObject.SetActive(owner == null);
                if (upgradeButton != null) upgradeButton.gameObject.SetActive(false);
                if (buyButton != null)
                    buyButton.interactable = owner == null && gm.CurrentPlayer.Money >= tile.purchasePrice;
            }
            else if (boardTile is TaxTile)
            {
                if (priceLabel != null) priceLabel.text = $"Fine: RM {tile.taxAmount}";
                if (rentLabel != null) rentLabel.text = "";
                if (buyButton != null) buyButton.gameObject.SetActive(false);
                if (upgradeButton != null) upgradeButton.gameObject.SetActive(false);
                owner = null;
            }
            else
            {
                if (priceLabel != null) priceLabel.text = "";
                if (rentLabel != null) rentLabel.text = "";
                if (buyButton != null) buyButton.gameObject.SetActive(false);
                if (upgradeButton != null) upgradeButton.gameObject.SetActive(false);
            }

            if (ownerLabel != null)
                ownerLabel.text = owner == null ? "Unowned" : $"Owner: {owner.Name}";

            if (canvasGroup != null) StartCoroutine(FadeIn());
        }

        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            _currentTile = null;
        }

        private void OnBuyPressed()
        {
            if (_currentTile == null) return;
            GameManager.Instance.TryBuyCurrentTile();
            Show(_currentTile); // refresh
        }

        private void OnUpgradePressed()
        {
            if (_currentTile == null) return;
            var gm = GameManager.Instance;
            if (gm.Board.GetTile(_currentTile.position) is PropertyTile pt)
                gm.TryUpgrade(pt);
            Show(_currentTile); // refresh
        }

        private IEnumerator FadeIn()
        {
            canvasGroup.alpha = 0f;
            float elapsed = 0f;
            while (elapsed < fadeInSeconds)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInSeconds);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }
    }
}
