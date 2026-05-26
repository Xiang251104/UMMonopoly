using System.Collections;
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
        [Tooltip("A TMP_Text label that reads 'IN JAIL' — shown/hidden based on player jail status.")]
        public TMP_Text jailLabel;
        [Tooltip("Full-rect Image behind the card — pulsed gold when this player is active.")]
        public Image glowBorder;

        // UM-branded player colours
        public Color[] playerColors =
        {
            new Color(0.651f, 0.098f, 0.180f),  // Player 1 – UM Red    #A6192E
            new Color(0.949f, 0.788f, 0.298f),  // Player 2 – UM Gold   #F2C94C
            new Color(0.106f, 0.310f, 0.545f),  // Player 3 – UM Blue   #1B4F8B
            new Color(0.102f, 0.510f, 0.282f),  // Player 4 – UM Green  #1A8248
        };

        private Player _player;
        private Coroutine _pulseCoroutine;

        static readonly Color GoldColor = new Color(0.949f, 0.788f, 0.298f); // #F2C94C

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
            if (jailLabel != null) jailLabel.gameObject.SetActive(_player.InJail);
        }

        public void MarkBankrupt()
        {
            if (bankruptOverlay != null) bankruptOverlay.SetActive(true);
        }

        public void SetActivePlayer(bool isActive)
        {
            if (_pulseCoroutine != null) { StopCoroutine(_pulseCoroutine); _pulseCoroutine = null; }

            if (!isActive)
            {
                // Inactive: fully transparent — card shows pure dark navy, no gold fill
                if (glowBorder != null)
                    glowBorder.color = new Color(GoldColor.r, GoldColor.g, GoldColor.b, 0f);
                transform.localScale = Vector3.one;
                return;
            }

            _pulseCoroutine = StartCoroutine(PulseGlow());
        }

        private IEnumerator PulseGlow()
        {
            while (true)
            {
                // ~1.4 Hz sine pulse: transparent → strong gold glow on active player's card
                float t = (Mathf.Sin(Time.time * 1.4f * Mathf.PI) + 1f) * 0.5f;

                if (glowBorder != null)
                    glowBorder.color = new Color(GoldColor.r, GoldColor.g, GoldColor.b,
                                                  Mathf.Lerp(0.0f, 0.80f, t));

                // Very subtle scale pulse (1.0 → 1.022)
                transform.localScale = Vector3.one * Mathf.Lerp(1.000f, 1.022f, t);

                yield return null;
            }
        }
    }
}
