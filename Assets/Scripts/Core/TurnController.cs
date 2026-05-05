using System.Collections;
using UnityEngine;
using UMMonopoly.Entities;

namespace UMMonopoly.Core
{
    /// <summary>
    /// Drives the user-facing turn flow: waits for the Roll button, animates dice/movement,
    /// then hands control back to the player for buy/upgrade/end-turn decisions.
    /// </summary>
    public class TurnController : MonoBehaviour
    {
        [Header("Optional animation pacing")]
        public float diceAnimSeconds = 0.6f;
        public float perStepSeconds = 0.15f;

        public bool IsAnimating { get; private set; }

        public void OnRollButtonPressed()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            if (gm.CurrentState != GameState.TurnStart && gm.CurrentState != GameState.RollPhase) return;
            if (IsAnimating) return;
            StartCoroutine(RollAndAnimate());
        }

        public void OnEndTurnPressed()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            if (gm.CurrentState != GameState.DecisionPhase) return;
            gm.SetState(GameState.EndTurnPhase);
            gm.EndTurn();
        }

        private IEnumerator RollAndAnimate()
        {
            IsAnimating = true;
            var gm = GameManager.Instance;
            gm.SetState(GameState.RollPhase);

            int startPos = gm.CurrentPlayer.BoardPosition;
            int total = gm.RollAndMove();

            yield return new WaitForSeconds(diceAnimSeconds);

            // Movement animation cue (UI listens to OnPlayerMoved already; pacing only)
            int steps = total;
            yield return new WaitForSeconds(perStepSeconds * Mathf.Max(1, steps));

            IsAnimating = false;
        }
    }
}
