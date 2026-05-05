using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UMMonopoly.Entities;

namespace UMMonopoly.UI
{
    public class PlayerCardUI : MonoBehaviour
    {
        public TMP_Text nameLabel;
        public TMP_Text moneyLabel;
        public TMP_Text propertyCountLabel;
        public Image colorTag;
        public GameObject bankruptOverlay;

        public Color[] playerColors = { Color.red, Color.blue, Color.green, Color.yellow };

        private Player _player;

        public void Bind(Player p)
        {
            _player = p;
            if (colorTag != null && p.Id < playerColors.Length) colorTag.color = playerColors[p.Id];
            Refresh();
        }

        public void Refresh()
        {
            if (_player == null) return;
            if (nameLabel != null) nameLabel.text = _player.Name;
            if (moneyLabel != null) moneyLabel.text = $"RM {_player.Money}";
            if (propertyCountLabel != null) propertyCountLabel.text = $"{_player.OwnedProperties.Count} props";
        }

        public void MarkBankrupt()
        {
            if (bankruptOverlay != null) bankruptOverlay.SetActive(true);
        }
    }
}
