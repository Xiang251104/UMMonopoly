using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UMMonopoly.Core;
using UMMonopoly.Entities;

namespace UMMonopoly.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Wire in Inspector")]
        public Transform playerPanelRoot;
        public PlayerCardUI playerCardPrefab;
        public TMP_Text turnLabel;
        public TMP_Text diceLabel;
        public Button rollButton;
        public Button endTurnButton;
        public Button buyButton;
        [Tooltip("A TMP_Text UI element used to show brief toast messages (rent paid, jail, etc.)")]
        public TMP_Text notificationLabel;
        [Tooltip("Dark-navy pill Image that sits behind the notification label — auto-created if left empty.")]
        public Image notificationBackground;
        [Tooltip("Reference to TileLandingPopup — auto-shown after token stops on a tile")]
        public TileLandingPopup tileLandingPopup;
        [Tooltip("Reference to TurnController — used to wait for dice/token animation to finish")]
        public TurnController turnController;

        private readonly Dictionary<int, PlayerCardUI> _cards = new Dictionary<int, PlayerCardUI>();
        private Coroutine _notifCoroutine;

        private void Start()
        {
            // Auto-create the notification background pill if not wired in Inspector
            if (notificationLabel != null && notificationBackground == null)
                notificationBackground = BuildNotificationPill();
        }

        private void OnEnable()
        {
            EventBus.OnTurnStarted    += HandleTurnStarted;
            EventBus.OnTurnEnded      += HandleTurnEnded;
            EventBus.OnDiceRolled     += HandleDiceRolled;
            EventBus.OnMoneyChanged   += HandleMoneyChanged;
            EventBus.OnPlayerBankrupt += HandleBankrupt;
            EventBus.OnGameWon        += HandleGameWon;
            EventBus.OnPlayerMoved    += HandlePlayerMoved;
            EventBus.OnRentPaid       += HandleRentPaid;
            EventBus.OnSentToJail     += HandleSentToJail;
            EventBus.OnPropertyBought += HandlePropertyBought;
            EventBus.OnTileResolved   += HandleTileResolved;
        }

        private void OnDisable()
        {
            EventBus.OnTurnStarted    -= HandleTurnStarted;
            EventBus.OnTurnEnded      -= HandleTurnEnded;
            EventBus.OnDiceRolled     -= HandleDiceRolled;
            EventBus.OnMoneyChanged   -= HandleMoneyChanged;
            EventBus.OnPlayerBankrupt -= HandleBankrupt;
            EventBus.OnGameWon        -= HandleGameWon;
            EventBus.OnPlayerMoved    -= HandlePlayerMoved;
            EventBus.OnRentPaid       -= HandleRentPaid;
            EventBus.OnSentToJail     -= HandleSentToJail;
            EventBus.OnPropertyBought -= HandlePropertyBought;
            EventBus.OnTileResolved   -= HandleTileResolved;
        }

        public void BuildPlayerCards(List<Player> players)
        {
            foreach (Transform t in playerPanelRoot) Destroy(t.gameObject);
            _cards.Clear();
            foreach (var p in players)
            {
                var card = Instantiate(playerCardPrefab, playerPanelRoot);
                card.Bind(p);
                _cards[p.Id] = card;
            }
        }

        private void HandleTurnStarted(int idx)
        {
            var p = GameManager.Instance.Players[idx];
            if (turnLabel != null) turnLabel.text = $"{p.Name}'s Turn";
            if (diceLabel != null) diceLabel.text = "Rolled: --";   // clear previous roll
            if (rollButton != null) rollButton.interactable = true;
            if (endTurnButton != null) endTurnButton.interactable = false;
            RefreshBuyButton();

            // Pulse-highlight the active player card; dim all others
            foreach (var kvp in _cards)
                kvp.Value.SetActivePlayer(kvp.Key == idx);
        }

        private void HandleDiceRolled(Player p, int total)
        {
            if (diceLabel != null) diceLabel.text = $"Rolled: {total}";
            if (rollButton != null) rollButton.interactable = false;
            if (endTurnButton != null) endTurnButton.interactable = true;
            RefreshBuyButton();
        }

        private void HandleMoneyChanged(Player p, int delta)
        {
            if (_cards.TryGetValue(p.Id, out var c)) c.Refresh();
        }

        private void HandleBankrupt(Player p)
        {
            if (_cards.TryGetValue(p.Id, out var c)) c.MarkBankrupt();
        }

        private void HandleGameWon(Player winner)
        {
            if (turnLabel != null) turnLabel.text = winner == null ? "Game Over" : $"{winner.Name} wins!";
            if (rollButton != null) rollButton.interactable = false;
            if (endTurnButton != null) endTurnButton.interactable = false;
            if (buyButton != null) buyButton.interactable = false;
        }

        private void HandlePlayerMoved(Player p, int pos)
        {
            RefreshBuyButton();
        }

        private void HandleRentPaid(Player from, Player to, int amount)
        {
            if (_cards.TryGetValue(from.Id, out var c)) c.Refresh();
            ShowNotification($"{from.Name} paid RM{amount} rent to {to.Name}");
        }

        private void HandleSentToJail(Player p)
        {
            if (_cards.TryGetValue(p.Id, out var c)) c.Refresh();
            ShowNotification($"{p.Name} was sent to Jail!");
        }

        private void HandlePropertyBought(Player p, PropertyTile pt)
        {
            RefreshBuyButton();
            ShowNotification($"{p.Name} bought {pt.Data.tileName} for RM{pt.Data.purchasePrice}");
        }

        private void HandleTurnEnded(int idx)
        {
            // Close the landing popup when the player ends their turn
            if (tileLandingPopup != null) tileLandingPopup.Hide();
        }

        // Called the instant DecisionPhase is set — refresh buy button then auto-show landing popup
        private void HandleTileResolved()
        {
            RefreshBuyButton();
            if (tileLandingPopup != null)
                StartCoroutine(ShowLandingPopupAfterAnimation());
        }

        private IEnumerator ShowLandingPopupAfterAnimation()
        {
            // Wait for dice physics + token hop animation to finish before showing popup
            if (turnController != null)
                yield return new WaitUntil(() => !turnController.IsAnimating);
            else
                yield return new WaitForSeconds(1.2f);

            var gm = GameManager.Instance;
            if (gm == null || gm.Board == null) yield break;

            var tile = gm.Board.GetTile(gm.CurrentPlayer.BoardPosition);
            tileLandingPopup.Show(tile.Data);
        }

        private void ShowNotification(string message)
        {
            if (notificationLabel == null) return;
            if (_notifCoroutine != null) StopCoroutine(_notifCoroutine);
            _notifCoroutine = StartCoroutine(NotificationRoutine(message));
        }

        private IEnumerator NotificationRoutine(string message)
        {
            notificationLabel.text = message;
            notificationLabel.gameObject.SetActive(true);
            if (notificationBackground != null) notificationBackground.gameObject.SetActive(true);
            yield return new WaitForSeconds(3.5f);
            notificationLabel.gameObject.SetActive(false);
            if (notificationBackground != null) notificationBackground.gameObject.SetActive(false);
        }

        private Image BuildNotificationPill()
        {
            var bgGO  = new GameObject("NotificationBg", typeof(RectTransform));
            bgGO.transform.SetParent(notificationLabel.transform.parent, false);

            // Insert immediately before the label so it renders behind it
            bgGO.transform.SetSiblingIndex(notificationLabel.transform.GetSiblingIndex());

            var bgRect  = bgGO.GetComponent<RectTransform>();
            var lblRect = notificationLabel.rectTransform;

            bgRect.anchorMin        = lblRect.anchorMin;
            bgRect.anchorMax        = lblRect.anchorMax;
            bgRect.pivot            = lblRect.pivot;
            bgRect.anchoredPosition = lblRect.anchoredPosition;
            // Pill is slightly wider/taller than the text label
            bgRect.sizeDelta = lblRect.sizeDelta + new Vector2(48f, 24f);

            var img = bgGO.AddComponent<Image>();
            img.color = new Color(0.039f, 0.078f, 0.157f, 0.85f); // dark navy 85%
            bgGO.SetActive(false);
            return img;
        }

        private void RefreshBuyButton()
        {
            if (buyButton == null || GameManager.Instance == null) return;
            var gm = GameManager.Instance;

            if (gm.CurrentState != GameState.DecisionPhase || gm.Board == null)
            {
                buyButton.interactable = false;
                return;
            }

            var tile = gm.Board.GetTile(gm.CurrentPlayer.BoardPosition);
            bool canBuy = false;
            if (tile is PropertyTile pt)
                canBuy = pt.Owner == null && gm.CurrentPlayer.Money >= pt.Data.purchasePrice;
            else if (tile is StationTile st)
                canBuy = st.Owner == null && gm.CurrentPlayer.Money >= st.Data.purchasePrice;
            else if (tile is UtilityTile ut)
                canBuy = ut.Owner == null && gm.CurrentPlayer.Money >= ut.Data.purchasePrice;

            buyButton.interactable = canBuy;
        }

        public void OnBuyPressed()
        {
            if (GameManager.Instance == null) return;
            // Direct purchase — do NOT reopen the popup.
            // If the popup was open the player already saw the tile info; just buy it.
            if (tileLandingPopup != null) tileLandingPopup.Hide();
            GameManager.Instance.TryBuyCurrentTile();
            RefreshBuyButton();
            foreach (var c in _cards.Values) c.Refresh();
        }
    }
}
