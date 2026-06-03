using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UMMonopoly.Core;
using UMMonopoly.Entities;

namespace UMMonopoly.UI
{
    /// <summary>
    /// Trade modal with two modes:
    ///   Compose — initiator picks tiles + cash to offer/request and clicks Send.
    ///   Review  — recipient sees summary and picks Accept / Reject / Counter.
    /// Counter swaps initiator+recipient and pre-fills the Compose UI with the previous selection.
    /// </summary>
    public class TradeModal : MonoBehaviour
    {
        [Header("Panel")]
        public GameObject panelRoot;
        public CanvasGroup canvasGroup;

        [Header("Mode toggles (parent containers)")]
        public GameObject composeUI;
        public GameObject reviewUI;

        [Header("Compose — header / labels")]
        public TMP_Text titleLabel;
        public TMP_Text leftHeaderLabel;
        public TMP_Text rightHeaderLabel;

        [Header("Compose — tile lists (scroll content)")]
        public RectTransform leftListContent;
        public RectTransform rightListContent;

        [Header("Compose — cash inputs")]
        public TMP_InputField leftCashInput;
        public TMP_InputField rightCashInput;

        [Header("Compose — buttons")]
        public Button cancelButton;
        public Button resetButton;
        public Button sendButton;

        [Header("Review — labels")]
        public TMP_Text reviewTitleLabel;
        public TMP_Text offerSummaryLabel;
        public TMP_Text requestSummaryLabel;

        [Header("Review — buttons")]
        public Button rejectButton;
        public Button counterButton;
        public Button acceptButton;

        [Header("Shared")]
        public TMP_Text resultLabel;

        static readonly Color RowUnselected = new Color(0.12f, 0.16f, 0.26f, 0.95f);
        static readonly Color RowSelected   = new Color(0.95f, 0.79f, 0.30f, 0.85f);

        private enum Mode { Compose, Review }

        private Player _from;
        private Player _to;
        private TradeOffer _pendingOffer;

        private class TileRow
        {
            public Tile Tile;
            public Image Bg;
            public TMP_Text Label;
            public bool Selected;
        }

        private readonly List<TileRow> _leftRows  = new List<TileRow>();
        private readonly List<TileRow> _rightRows = new List<TileRow>();

        private void Awake()
        {
            if (cancelButton  != null) cancelButton.onClick.AddListener(Hide);
            if (resetButton   != null) resetButton.onClick.AddListener(OnReset);
            if (sendButton    != null) sendButton.onClick.AddListener(OnSend);
            if (rejectButton  != null) rejectButton.onClick.AddListener(OnReject);
            if (counterButton != null) counterButton.onClick.AddListener(OnCounter);
            if (acceptButton  != null) acceptButton.onClick.AddListener(OnAccept);
            Hide();
        }

        // ── Public API ─────────────────────────────────────────────────────────

        public void Show(Player from, Player to)
        {
            if (from == null || to == null) return;
            _from = from;
            _to = to;
            _pendingOffer = null;

            if (panelRoot   != null) panelRoot.SetActive(true);
            if (canvasGroup != null) canvasGroup.alpha = 1f;
            if (resultLabel != null) resultLabel.text = "";

            BuildCompose(prefill: null);
            SetMode(Mode.Compose);
        }

        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            ClearList(leftListContent,  _leftRows);
            ClearList(rightListContent, _rightRows);
            _pendingOffer = null;
        }

        // ── Mode switching ─────────────────────────────────────────────────────

        private void SetMode(Mode mode)
        {
            if (composeUI != null) composeUI.SetActive(mode == Mode.Compose);
            if (reviewUI  != null) reviewUI.SetActive(mode == Mode.Review);
            if (resultLabel != null) resultLabel.text = "";
        }

        // ── Compose ────────────────────────────────────────────────────────────

        private void BuildCompose(TradeOffer prefill)
        {
            if (titleLabel != null)       titleLabel.text       = $"TRADE: {_from.Name}  ↔  {_to.Name}";
            if (leftHeaderLabel != null)  leftHeaderLabel.text  = $"{_from.Name} offers:";
            if (rightHeaderLabel != null) rightHeaderLabel.text = $"{_to.Name} gives:";

            PopulateList(leftListContent,  _leftRows,  CollectOwned(_from));
            PopulateList(rightListContent, _rightRows, CollectOwned(_to));

            if (leftCashInput  != null) leftCashInput.text  = prefill != null ? prefill.FromCash.ToString() : "0";
            if (rightCashInput != null) rightCashInput.text = prefill != null ? prefill.ToCash.ToString()   : "0";

            if (prefill != null)
            {
                foreach (var row in _leftRows)  if (prefill.FromGives.Contains(row.Tile)) SelectRow(row);
                foreach (var row in _rightRows) if (prefill.ToGives.Contains(row.Tile))   SelectRow(row);
            }
        }

        private void OnReset()
        {
            foreach (var r in _leftRows)  ResetRow(r);
            foreach (var r in _rightRows) ResetRow(r);
            if (leftCashInput  != null) leftCashInput.text  = "0";
            if (rightCashInput != null) rightCashInput.text = "0";
            if (resultLabel    != null) resultLabel.text    = "";
        }

        private void OnSend()
        {
            if (_from == null || _to == null) return;
            var offer = BuildOfferFromCompose();
            bool nothing = offer.FromGives.Count == 0 && offer.ToGives.Count == 0
                        && offer.FromCash == 0 && offer.ToCash == 0;
            if (nothing)
            {
                if (resultLabel != null) resultLabel.text = "Nothing to trade.";
                return;
            }
            _pendingOffer = offer;
            ShowReview(offer);
        }

        private TradeOffer BuildOfferFromCompose()
        {
            var offer = new TradeOffer
            {
                From = _from,
                To = _to,
                FromCash = ParseInt(leftCashInput),
                ToCash   = ParseInt(rightCashInput)
            };
            foreach (var r in _leftRows)  if (r.Selected) offer.FromGives.Add(r.Tile);
            foreach (var r in _rightRows) if (r.Selected) offer.ToGives.Add(r.Tile);
            return offer;
        }

        // ── Review ─────────────────────────────────────────────────────────────

        private void ShowReview(TradeOffer offer)
        {
            SetMode(Mode.Review);
            if (reviewTitleLabel    != null) reviewTitleLabel.text    = $"{offer.To.Name}: Review Offer";
            if (offerSummaryLabel   != null) offerSummaryLabel.text   = BuildSummary($"{offer.From.Name} offers you:", offer.FromGives, offer.FromCash);
            if (requestSummaryLabel != null) requestSummaryLabel.text = BuildSummary("In exchange for:", offer.ToGives, offer.ToCash);
        }

        private static string BuildSummary(string heading, List<Tile> tiles, int cash)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<b>{heading}</b>");
            sb.AppendLine();
            if (tiles.Count == 0 && cash == 0)
            {
                sb.AppendLine("  <i>(nothing)</i>");
            }
            else
            {
                foreach (var t in tiles)
                    sb.AppendLine($"   •  {t.Data.tileName}   (RM {t.Data.purchasePrice})");
                if (cash > 0)
                    sb.AppendLine($"   •  RM {cash} cash");
            }
            return sb.ToString().TrimEnd();
        }

        private void OnAccept()
        {
            if (_pendingOffer == null) return;
            var gm = GameManager.Instance;
            if (gm == null) return;
            bool ok = gm.Bank.ExecuteTrade(_pendingOffer);
            if (ok)
            {
                _pendingOffer = null;
                Hide();
            }
            else
            {
                if (resultLabel != null) resultLabel.text = "Trade failed — insufficient cash?";
            }
        }

        private void OnReject()
        {
            _pendingOffer = null;
            Hide();
        }

        private void OnCounter()
        {
            if (_pendingOffer == null) return;

            // Swap initiator + recipient and mirror the offer so the new initiator
            // sees the OLD recipient's side of the deal pre-selected.
            var p = _pendingOffer;
            var counterPrefill = new TradeOffer
            {
                From      = p.To,
                To        = p.From,
                FromGives = new List<Tile>(p.ToGives),
                ToGives   = new List<Tile>(p.FromGives),
                FromCash  = p.ToCash,
                ToCash    = p.FromCash
            };
            _pendingOffer = null;

            _from = counterPrefill.From;
            _to   = counterPrefill.To;
            BuildCompose(counterPrefill);
            SetMode(Mode.Compose);
        }

        // ── Row helpers ────────────────────────────────────────────────────────

        private static List<Tile> CollectOwned(Player p)
        {
            var list = new List<Tile>();
            foreach (var t in p.OwnedProperties) list.Add(t);
            foreach (var t in p.OwnedStations)   list.Add(t);
            foreach (var t in p.OwnedUtilities)  list.Add(t);
            return list;
        }

        private void PopulateList(RectTransform parent, List<TileRow> rows, List<Tile> tiles)
        {
            ClearList(parent, rows);
            if (parent == null) return;
            foreach (var tile in tiles) rows.Add(BuildRow(parent, tile));
        }

        private void ClearList(RectTransform parent, List<TileRow> rows)
        {
            rows.Clear();
            if (parent == null) return;
            for (int i = parent.childCount - 1; i >= 0; i--)
                Destroy(parent.GetChild(i).gameObject);
        }

        private TileRow BuildRow(RectTransform parent, Tile tile)
        {
            var rowGO = new GameObject(tile.Data.tileName, typeof(RectTransform));
            rowGO.transform.SetParent(parent, false);

            var bg = rowGO.AddComponent<Image>();
            bg.color = RowUnselected;

            var btn = rowGO.AddComponent<Button>();
            // Button.colors acts as a tint multiplier; keep near-white so the Image's color shows true.
            var cb = btn.colors;
            cb.normalColor      = Color.white;
            cb.highlightedColor = new Color(1.10f, 1.10f, 1.15f, 1f);
            cb.pressedColor     = new Color(0.85f, 0.85f, 0.85f, 1f);
            cb.selectedColor    = Color.white;
            cb.disabledColor    = Color.white;
            btn.colors = cb;

            var le = rowGO.AddComponent<LayoutElement>();
            le.minHeight       = 32f;
            le.preferredHeight = 32f;

            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(rowGO.transform, false);
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(10f, 0f);
            labelRect.offsetMax = new Vector2(-10f, 0f);
            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text          = $"{tile.Data.tileName}   (RM {tile.Data.purchasePrice})";
            tmp.fontSize      = 15f;
            tmp.color         = Color.white;
            tmp.alignment     = TextAlignmentOptions.MidlineLeft;
            tmp.raycastTarget = false;

            var row = new TileRow { Tile = tile, Bg = bg, Label = tmp, Selected = false };
            btn.onClick.AddListener(() =>
            {
                row.Selected = !row.Selected;
                if (row.Selected) SelectRow(row); else ResetRow(row);
            });
            return row;
        }

        private static void SelectRow(TileRow r)
        {
            r.Selected = true;
            if (r.Bg    != null) r.Bg.color    = RowSelected;
            if (r.Label != null) r.Label.color = Color.black;
        }

        private static void ResetRow(TileRow r)
        {
            r.Selected = false;
            if (r.Bg    != null) r.Bg.color    = RowUnselected;
            if (r.Label != null) r.Label.color = Color.white;
        }

        private static int ParseInt(TMP_InputField field)
        {
            if (field == null) return 0;
            if (int.TryParse(field.text, out int v)) return Mathf.Max(0, v);
            return 0;
        }
    }
}
