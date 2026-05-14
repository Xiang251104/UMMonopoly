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

        private Tile _shownTile;
        private PropertyTile _shownProperty;

        private void OnEnable()
        {
            EventBus.OnTileResolved += HandleTileResolved;
        }

        private void OnDisable()
        {
            EventBus.OnTileResolved -= HandleTileResolved;
        }

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Hide);
            if (buyButton != null) buyButton.onClick.AddListener(OnBuy);
            if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgrade);
            Hide();
        }

        public void Show(Tile tile)
        {
            _shownTile = tile;
            _shownProperty = tile as PropertyTile;
            if (panelRoot != null) panelRoot.SetActive(true);
            if (nameLabel != null) nameLabel.text = tile.Name;
            if (priceLabel != null) priceLabel.text = BuildPriceText(tile);
            if (rentLabel != null)
            {
                rentLabel.text = BuildRentText(tile);
            }
            if (ownerLabel != null)
                ownerLabel.text = BuildOwnerText(tile);

            var gm = GameManager.Instance;
            if (buyButton != null)
                buyButton.interactable = IsCurrentBuyableTile(tile, gm);
            if (upgradeButton != null)
                upgradeButton.interactable = _shownProperty != null && _shownProperty.Owner == gm.CurrentPlayer && _shownProperty.CanUpgrade(gm.Board);
        }

        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            _shownTile = null;
            _shownProperty = null;
        }

        private void OnBuy()
        {
            if (_shownTile == null || !IsCurrentBuyableTile(_shownTile, GameManager.Instance)) return;
            GameManager.Instance.TryBuyCurrentTile();
            Show(_shownTile);
        }

        private void OnUpgrade()
        {
            if (_shownProperty == null) return;
            GameManager.Instance.TryUpgrade(_shownProperty);
            Show(_shownProperty);
        }

        private void HandleTileResolved(Player player, Tile tile)
        {
            if (player == GameManager.Instance.CurrentPlayer)
            {
                Show(tile);
            }
        }

        private static bool IsCurrentBuyableTile(Tile tile, GameManager gm)
        {
            if (gm == null || tile == null || gm.Board == null || gm.CurrentPlayer.BoardPosition != tile.Position) return false;
            if (tile is PropertyTile p) return p.Owner == null && gm.CurrentPlayer.Money >= p.Data.purchasePrice;
            if (tile is StationTile s) return s.Owner == null && gm.CurrentPlayer.Money >= s.Data.purchasePrice;
            if (tile is UtilityTile u) return u.Owner == null && gm.CurrentPlayer.Money >= u.Data.purchasePrice;
            return false;
        }

        private static string BuildPriceText(Tile tile)
        {
            if (tile is PropertyTile || tile is StationTile || tile is UtilityTile)
            {
                return $"Price: RM {tile.Data.purchasePrice}";
            }
            if (tile is TaxTile)
            {
                return $"Tax: RM {tile.Data.taxAmount}";
            }
            if (tile is CardTile)
            {
                return $"{tile.Data.deckType} deck";
            }
            return "No purchase";
        }

        private static string BuildRentText(Tile tile)
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.Board == null) return string.Empty;
            if (tile is PropertyTile property)
            {
                return $"Rent: RM {property.CurrentRent(gm.Board)} (lvl {property.UpgradeLevel})";
            }
            if (tile is StationTile station)
            {
                return $"Rent: RM {station.CurrentRent(gm.Board)}";
            }
            if (tile is UtilityTile utility)
            {
                return $"Rent: {utility.CurrentRent(gm.Board, gm.Bank.LastDiceTotal)} after dice";
            }
            return string.Empty;
        }

        private static string BuildOwnerText(Tile tile)
        {
            if (tile is PropertyTile p) return p.Owner == null ? "Unowned" : $"Owner: {p.Owner.Name}";
            if (tile is StationTile s) return s.Owner == null ? "Unowned" : $"Owner: {s.Owner.Name}";
            if (tile is UtilityTile u) return u.Owner == null ? "Unowned" : $"Owner: {u.Owner.Name}";
            return tile.Name;
        }
    }
}
