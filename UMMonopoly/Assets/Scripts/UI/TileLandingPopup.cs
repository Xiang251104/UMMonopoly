using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UMMonopoly.Core;
using UMMonopoly.Data;
using UMMonopoly.Entities;

namespace UMMonopoly.UI
{
    public class TileLandingPopup : MonoBehaviour
    {
        [Header("Panel")]
        public GameObject panelRoot;
        public CanvasGroup canvasGroup;
        public float fadeInSeconds = 0.25f;

        [Header("Location Photo")]
        public Image locationPhoto;
        public GameObject photoPlaceholder;

        [Header("Info — wire these in Inspector")]
        public TMP_Text tileNameLabel;
        public TMP_Text descriptionLabel;   // single text block for ALL tile info
        public TMP_Text priceLabel;         // hidden — kept for legacy wiring, unused
        public TMP_Text rentLabel;          // hidden — kept for legacy wiring, unused
        public TMP_Text ownerLabel;         // hidden — kept for legacy wiring, unused

        [Header("Buttons")]
        public Button buyButton;
        public Button upgradeButton;
        public Button closeButton;

        static readonly Color LightGray = new Color(0.820f, 0.835f, 0.859f, 1f);
        static readonly Color NavyPhoto = new Color(0.039f, 0.078f, 0.157f, 0.95f);

        static readonly string[] LevelNames =
            { "Base rent", "+ 1 Facility", "+ 2 Facilities", "+ 3 Facilities", "Postgrad Centre" };

        private TileDataSO _currentTile;

        private void Awake()
        {
            if (descriptionLabel != null)
            {
                // Content area height = card(490) - photo(196) - buttons(54) = 240px
                // Active VLG items after hiding rows: TileNameLabel(34) + Divider(1.5) + this label
                // Spacing(12) + Padding(16) = 28px fixed overhead → safe max ≈ 165px
                var le = descriptionLabel.GetComponent<LayoutElement>();
                if (le != null) { le.preferredHeight = 260f; le.minHeight = 120f; }

                var content = descriptionLabel.transform.parent;
                var vlg = content != null ? content.GetComponent<VerticalLayoutGroup>() : null;
                if (vlg != null) vlg.childControlHeight = true;

                // Belt-and-suspenders: also patch sizeDelta directly.
                // VLG with childControlHeight=false positions children by LayoutElement.preferredHeight
                // but never resizes their RectTransforms, so Divider keeps its 100 px default.
                var descRT = descriptionLabel.GetComponent<RectTransform>();
                if (descRT != null) descRT.sizeDelta = new Vector2(descRT.sizeDelta.x, 260f);

                if (content != null)
                {
                    for (int i = 0; i < content.childCount; i++)
                    {
                        if (content.GetChild(i).name == "Divider")
                        {
                            var drt = content.GetChild(i).GetComponent<RectTransform>();
                            if (drt != null) drt.sizeDelta = new Vector2(drt.sizeDelta.x, 1.5f);
                            break;
                        }
                    }
                }

                descriptionLabel.color        = LightGray;
                descriptionLabel.fontSize     = 16f;
                descriptionLabel.fontStyle    = FontStyles.Normal;
                descriptionLabel.overflowMode = TextOverflowModes.Overflow;
            }

            // priceLabel and rentLabel are both children of PriceRentRow — hide the whole row.
            // ownerLabel is a direct VLG child — hide it too.
            // All info is now consolidated inside descriptionLabel.
            if (priceLabel != null && priceLabel.transform.parent != null)
                priceLabel.transform.parent.gameObject.SetActive(false);
            if (ownerLabel != null)
                ownerLabel.gameObject.SetActive(false);

            if (closeButton   != null) closeButton.onClick.AddListener(Hide);
            if (buyButton     != null) buyButton.onClick.AddListener(OnBuyPressed);
            if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgradePressed);
            Hide();
        }

        // ── Public API ─────────────────────────────────────────────────────────

        public void Show(TileDataSO tile) => Show(tile, inspectOnly: false);

        public void Show(TileDataSO tile, bool inspectOnly)
        {
            if (tile == null) { Debug.LogError("[TileLandingPopup] null tile."); return; }

            _currentTile = tile;

            bool showForType = tile.type == TileType.Property
                            || tile.type == TileType.Station
                            || tile.type == TileType.Utility
                            || tile.type == TileType.Tax
                            || tile.type == TileType.Go
                            || tile.type == TileType.Jail
                            || tile.type == TileType.FreeParking;
            if (!showForType) return;

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    panelRoot.GetComponent<RectTransform>());
            }

            // ── Photo ──────────────────────────────────────────────────────────
            if (locationPhoto != null)
            {
                locationPhoto.gameObject.SetActive(true);
                if (tile.locationPhoto != null)
                {
                    locationPhoto.sprite = tile.locationPhoto;
                    locationPhoto.color  = Color.white;
                    if (photoPlaceholder != null) photoPlaceholder.SetActive(false);
                }
                else
                {
                    locationPhoto.sprite = null;
                    locationPhoto.color  = NavyPhoto;
                    if (photoPlaceholder != null) photoPlaceholder.SetActive(true);
                }
            }

            // ── Title ──────────────────────────────────────────────────────────
            if (tileNameLabel != null) tileNameLabel.text = tile.tileName ?? "Unknown";

            // ── Single info text block ─────────────────────────────────────────
            var gm = GameManager.Instance;
            if (gm == null) { Debug.LogError("[TileLandingPopup] GameManager null."); return; }

            Tile boardTile = null;
            try { boardTile = gm.Board.GetTile(tile.position); }
            catch (System.Exception e) { Debug.LogError($"[TileLandingPopup] GetTile threw: {e.Message}"); return; }
            if (boardTile == null) { Debug.LogError($"[TileLandingPopup] GetTile({tile.position}) = null"); return; }

            SetBuyButton(false, false);
            SetUpgradeButton(false, false);

            if (boardTile is PropertyTile pt)
            {
                SetLabel(descriptionLabel, BuildPropertyInfo(tile, pt, gm.Board));

                bool canBuy     = pt.Owner == null && gm.CurrentPlayer.Money >= tile.purchasePrice;
                bool isOwnedByCurrent = pt.Owner == gm.CurrentPlayer;
                bool canUpgrade = isOwnedByCurrent && pt.CanUpgrade(gm.Board);
                SetBuyButton(pt.Owner == null, canBuy);
                // Show the Upgrade button whenever the current player owns this tile;
                // disable (gray out) when CanUpgrade returns false (e.g. no full colour set).
                SetUpgradeButton(visible: isOwnedByCurrent, interactable: canUpgrade);
            }
            else if (boardTile is StationTile st)
            {
                SetLabel(descriptionLabel, BuildStationInfo(tile, st, gm.Board));
                bool canBuy = st.Owner == null && gm.CurrentPlayer.Money >= tile.purchasePrice;
                SetBuyButton(st.Owner == null, canBuy);
            }
            else if (boardTile is UtilityTile ut)
            {
                SetLabel(descriptionLabel, BuildUtilityInfo(tile, ut, gm.Board, gm.Bank.LastDiceTotal));
                bool canBuy = ut.Owner == null && gm.CurrentPlayer.Money >= tile.purchasePrice;
                SetBuyButton(ut.Owner == null, canBuy);
            }
            else if (boardTile is TaxTile)
            {
                SetLabel(descriptionLabel, $"Tax tile.\n\nYou must pay RM {tile.taxAmount} to the bank.");
            }
            else if (boardTile is GoTile)
            {
                SetLabel(descriptionLabel, $"Collect RM {tile.taxAmount} salary\nwhen you pass or land here.");
            }
            else if (boardTile is JailTile)
            {
                SetLabel(descriptionLabel, "Just visiting — no action required.");
            }
            else if (boardTile is FreeParkingTile)
            {
                SetLabel(descriptionLabel, "Free Parking.\nTake a break — nothing happens here.");
            }
            else
            {
                SetLabel(descriptionLabel, "");
            }

            // Inspect-only: hide Buy/Upgrade so the popup is purely informational.
            // (Player can still see all rent/owner/description data, but no actions.)
            if (inspectOnly)
            {
                SetBuyButton(false, false);
                SetUpgradeButton(false, false);
            }

            if (canvasGroup != null) StartCoroutine(FadeIn());
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0)) return;

            // Don't intercept clicks that land on UI (the popup itself, buttons, etc.)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            var cam = Camera.main;
            if (cam == null) return;

            var ray = cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 100f)) return;

            // Walk up the hit transform until we find the tile anchor (direct child of the Board).
            var boardRoot = GameObject.Find("Board");
            if (boardRoot == null) return;
            var t = hit.collider != null ? hit.collider.transform : null;
            while (t != null && t.parent != boardRoot.transform) t = t.parent;
            if (t == null) return;

            int position = t.GetSiblingIndex();
            var gm = GameManager.Instance;
            if (gm == null || gm.Board == null) return;

            var boardTile = gm.Board.GetTile(position);
            if (boardTile == null || boardTile.Data == null) return;

            // If the player is standing on this exact tile, treat the click like a landing
            // (so they can Buy/Upgrade their current tile). Otherwise pure inspect mode.
            bool atCurrentTile = position == gm.CurrentPlayer.BoardPosition;
            Show(boardTile.Data, inspectOnly: !atCurrentTile);
        }

        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            _currentTile = null;
        }

        // ── Button handlers ────────────────────────────────────────────────────

        private void OnBuyPressed()
        {
            if (_currentTile == null) return;
            if (GameManager.Instance.TryBuyCurrentTile()) Hide();
        }

        private void OnUpgradePressed()
        {
            if (_currentTile == null) return;
            var gm = GameManager.Instance;
            if (gm.Board.GetTile(_currentTile.position) is PropertyTile pt)
            {
                bool upgraded = gm.TryUpgrade(pt);
                if (upgraded)
                {
                    SetLabel(descriptionLabel, BuildPropertyInfo(_currentTile, pt, gm.Board));
                    SetUpgradeButton(visible: true, interactable: false);
                }
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static void SetLabel(TMP_Text label, string text)
        {
            if (label != null) label.text = text ?? "";
        }

        private void SetBuyButton(bool visible, bool interactable)
        {
            if (buyButton == null) return;
            buyButton.gameObject.SetActive(visible);
            buyButton.interactable = interactable;
        }

        private void SetUpgradeButton(bool visible, bool interactable)
        {
            if (upgradeButton == null) return;
            upgradeButton.gameObject.SetActive(visible);
            upgradeButton.interactable = interactable;
        }

        // ── Fade ───────────────────────────────────────────────────────────────

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

        // ── Info builders ──────────────────────────────────────────────────────

        private static string BuildPropertyInfo(TileDataSO data, PropertyTile pt, Board board)
        {
            var sb = new StringBuilder();

            if (pt.Owner != null)
            {
                sb.AppendLine($"Owner: {pt.Owner.Name}");
                if (pt.OwnsFullSet(board) && pt.UpgradeLevel == 0)
                    sb.AppendLine("(Full colour set — base rent doubled)");
                else if (pt.Owner == GameManager.Instance.CurrentPlayer && !pt.OwnsFullSet(board))
                    sb.AppendLine($"(Tip: own all {data.colorGroup} tiles for doubled base rent)");
            }

            // Price line
            sb.Append($"Price: RM {data.purchasePrice}");
            if (data.upgradeCost > 0) sb.Append($"   |   Upgrade: RM {data.upgradeCost}");
            sb.AppendLine();
            sb.AppendLine();

            // Rent table
            if (data.rentTable != null && data.rentTable.Length > 0)
            {
                for (int i = 0; i < data.rentTable.Length; i++)
                {
                    string name    = i < LevelNames.Length ? LevelNames[i] : $"Level {i}";
                    string current = (pt.UpgradeLevel == i) ? "  <<" : "";
                    sb.AppendLine($"  {name,-22}RM {data.rentTable[i]}{current}");
                }
            }
            else
            {
                sb.AppendLine("  Rent data not configured.");
            }

            if (!string.IsNullOrWhiteSpace(data.locationDescription))
            {
                sb.AppendLine();
                sb.Append(data.locationDescription);
            }

            return sb.ToString().TrimEnd();
        }

        private static string BuildStationInfo(TileDataSO data, StationTile st, Board board)
        {
            var sb = new StringBuilder();
            if (st.Owner != null) { sb.AppendLine($"Owner: {st.Owner.Name}"); sb.AppendLine(); }
            sb.AppendLine($"Price: RM {data.purchasePrice}");
            sb.AppendLine();

            if (data.rentTable != null && data.rentTable.Length > 0)
            {
                for (int i = 0; i < data.rentTable.Length; i++)
                    sb.AppendLine($"  Own {i + 1} station{(i > 0 ? "s" : "")}:   RM {data.rentTable[i]}");
            }
            else sb.AppendLine("  Rent data not configured.");

            return sb.ToString().TrimEnd();
        }

        private static string BuildUtilityInfo(TileDataSO data, UtilityTile ut, Board board, int diceTotal)
        {
            var sb = new StringBuilder();
            if (ut.Owner != null) { sb.AppendLine($"Owner: {ut.Owner.Name}"); sb.AppendLine(); }
            sb.AppendLine($"Price: RM {data.purchasePrice}");
            sb.AppendLine();
            sb.AppendLine("  Own 1 utility:    Dice roll x 4");
            sb.AppendLine("  Own 2 utilities:  Dice roll x 10");

            if (!string.IsNullOrWhiteSpace(data.locationDescription))
            {
                sb.AppendLine();
                sb.Append(data.locationDescription);
            }

            return sb.ToString().TrimEnd();
        }
    }
}
