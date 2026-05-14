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
        public Button saveButton;
        public Button loadButton;

        private readonly Dictionary<int, PlayerCardUI> _cards = new Dictionary<int, PlayerCardUI>();

        private void OnEnable()
        {
            EventBus.OnTurnStarted += HandleTurnStarted;
            EventBus.OnDiceRolled += HandleDiceRolled;
            EventBus.OnMoneyChanged += HandleMoneyChanged;
            EventBus.OnPlayerBankrupt += HandleBankrupt;
            EventBus.OnGameWon += HandleGameWon;
        }

        private void OnDisable()
        {
            EventBus.OnTurnStarted -= HandleTurnStarted;
            EventBus.OnDiceRolled -= HandleDiceRolled;
            EventBus.OnMoneyChanged -= HandleMoneyChanged;
            EventBus.OnPlayerBankrupt -= HandleBankrupt;
            EventBus.OnGameWon -= HandleGameWon;
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
            if (rollButton != null) rollButton.interactable = true;
            if (endTurnButton != null) endTurnButton.interactable = false;
            RefreshBuyButton();
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

        private void RefreshBuyButton()
        {
            if (buyButton == null || GameManager.Instance == null) return;
            var gm = GameManager.Instance;
            var tile = gm.Board.GetTile(gm.CurrentPlayer.BoardPosition);
            bool canBuy = (tile is PropertyTile p && p.Owner == null && gm.CurrentPlayer.Money >= p.Data.purchasePrice)
                || (tile is StationTile s && s.Owner == null && gm.CurrentPlayer.Money >= s.Data.purchasePrice)
                || (tile is UtilityTile u && u.Owner == null && gm.CurrentPlayer.Money >= u.Data.purchasePrice);
            buyButton.interactable = canBuy;
        }

        public void OnBuyPressed()
        {
            GameManager.Instance.TryBuyCurrentTile();
            RefreshBuyButton();
            foreach (var c in _cards.Values) c.Refresh();
        }

        public void OnSavePressed()
        {
            GameManager.Instance.SaveCurrentGame();
        }

        public void OnLoadPressed()
        {
            if (!GameManager.Instance.LoadSavedGame()) return;
            BuildPlayerCards(GameManager.Instance.Players);
            RefreshBuyButton();
        }
    }
}
