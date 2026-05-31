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
        [Tooltip("Visible only when the active player is in jail AND owns ≥1 Get-Out-of-Jail card.")]
        public Button useJailCardButton;
        [Tooltip("Visible only when the active player is in jail. Interactable only if they can afford the fine.")]
        public Button payJailFineButton;
        [Tooltip("Jail fine cost — matches GameConfigSO.jailFine.")]
        public int jailFineAmount = 50;
        [Tooltip("A TMP_Text UI element used to show brief toast messages (rent paid, jail, etc.)")]
        public TMP_Text notificationLabel;
        [Tooltip("Dark-navy pill Image that sits behind the notification label.")]
        public Image notificationBackground;
        [Tooltip("CanvasGroup on the toast root — enables fade-in/out animation when wired.")]
        public CanvasGroup notificationCanvasGroup;
        [Tooltip("Reference to TileLandingPopup — auto-shown after token stops on a tile")]
        public TileLandingPopup tileLandingPopup;
        [Tooltip("Reference to TurnController — used to wait for dice/token animation to finish")]
        public TurnController turnController;

        private readonly Dictionary<int, PlayerCardUI> _cards = new Dictionary<int, PlayerCardUI>();
        private Coroutine _notifCoroutine;

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
            EventBus.OnTilePurchased  += HandleTilePurchased;
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
            EventBus.OnTilePurchased  -= HandleTilePurchased;
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
            RefreshJailButtons();

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
            RefreshJailButtons();
        }

        private void HandleMoneyChanged(Player p, int delta)
        {
            if (_cards.TryGetValue(p.Id, out var c)) c.Refresh();
            RefreshJailButtons();
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
            // Notification handled by HandleTilePurchased (fires for property + station + utility)
            RefreshBuyButton();
        }

        private void HandleTilePurchased(Player p, UMMonopoly.Data.TileDataSO tile)
        {
            RefreshBuyButton();
            ShowNotification($"{p.Name} bought {tile.tileName} for RM{tile.purchasePrice}");
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
            RefreshJailButtons();
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

            // Pick the root that gets shown/hidden — prefer the canvasGroup parent if wired
            GameObject root = notificationCanvasGroup != null
                ? notificationCanvasGroup.gameObject
                : notificationLabel.gameObject;
            root.SetActive(true);
            if (notificationBackground != null && notificationCanvasGroup == null)
                notificationBackground.gameObject.SetActive(true);

            const float fadeIn  = 0.25f;
            const float hold    = 3.0f;
            const float fadeOut = 0.5f;

            // Fade in
            if (notificationCanvasGroup != null)
            {
                notificationCanvasGroup.alpha = 0f;
                float t = 0f;
                while (t < fadeIn)
                {
                    t += Time.unscaledDeltaTime;
                    notificationCanvasGroup.alpha = Mathf.Clamp01(t / fadeIn);
                    yield return null;
                }
                notificationCanvasGroup.alpha = 1f;
            }

            yield return new WaitForSeconds(hold);

            // Fade out
            if (notificationCanvasGroup != null)
            {
                float t = 0f;
                while (t < fadeOut)
                {
                    t += Time.unscaledDeltaTime;
                    notificationCanvasGroup.alpha = 1f - Mathf.Clamp01(t / fadeOut);
                    yield return null;
                }
                notificationCanvasGroup.alpha = 0f;
            }

            root.SetActive(false);
            if (notificationBackground != null && notificationCanvasGroup == null)
                notificationBackground.gameObject.SetActive(false);
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

        public void OnUseJailCardPressed()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.AttemptJailExit(payFine: false);
            RefreshJailButtons();
            foreach (var c in _cards.Values) c.Refresh();
        }

        public void OnPayJailFinePressed()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.AttemptJailExit(payFine: true);
            RefreshJailButtons();
            foreach (var c in _cards.Values) c.Refresh();
        }

        private void RefreshJailButtons()
        {
            if (GameManager.Instance == null) return;
            var cp = GameManager.Instance.CurrentPlayer;
            bool inJail = cp != null && cp.InJail;

            if (useJailCardButton != null)
            {
                bool show = inJail && cp.GetOutOfJailCards > 0;
                useJailCardButton.gameObject.SetActive(show);
                useJailCardButton.interactable = show;
            }
            if (payJailFineButton != null)
            {
                payJailFineButton.gameObject.SetActive(inJail);
                payJailFineButton.interactable = inJail && cp.Money >= jailFineAmount;
            }
        }
    }
}
