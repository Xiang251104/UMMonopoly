using TMPro;
using UnityEngine;
using UMMonopoly.Core;
using UMMonopoly.Entities;

namespace UMMonopoly.UI
{
    public class DiceUI : MonoBehaviour
    {
        public TMP_Text die1Label;
        public TMP_Text die2Label;

        private void OnEnable()
        {
            EventBus.OnDiceRolled += HandleRolled;
        }

        private void OnDisable()
        {
            EventBus.OnDiceRolled -= HandleRolled;
        }

        private void HandleRolled(Player p, int total)
        {
            var dice = GameManager.Instance.Dice;
            if (dice.LastRoll.Length >= 1 && die1Label != null) die1Label.text = dice.LastRoll[0].ToString();
            if (dice.LastRoll.Length >= 2 && die2Label != null) die2Label.text = dice.LastRoll[1].ToString();
        }
    }
}
