using System.Collections;
using UnityEngine;
using UMMonopoly.Systems;
using UMMonopoly.UI;

namespace UMMonopoly.Core
{
    /// <summary>
    /// Bridges the Roll button → physical dice → token movement → game logic.
    /// Attach to a manager GameObject in the GameBoard scene.
    /// Wire diceA and diceB to the two 3D die GameObjects.
    /// Wire boardView to the BoardView script.
    /// </summary>
    public class TurnController : MonoBehaviour
    {
        [Header("References")]
        public DicePhysics diceA;
        public DicePhysics diceB;
        public BoardView boardView;

        [Header("Pacing")]
        [Tooltip("Seconds to wait after dice settle before moving the token.")]
        public float pauseAfterRoll = 0.4f;

        public bool IsAnimating { get; private set; }

        public void OnRollButtonPressed()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            if (gm.CurrentState != GameState.TurnStart && gm.CurrentState != GameState.RollPhase) return;
            if (IsAnimating) return;
            StartCoroutine(PhysicsRollCoroutine());
        }

        public void OnEndTurnPressed()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            if (gm.CurrentState != GameState.DecisionPhase) return;
            if (IsAnimating) return;
            gm.SetState(GameState.EndTurnPhase);
            gm.EndTurn();
        }

        private IEnumerator PhysicsRollCoroutine()
        {
            IsAnimating = true;
            var gm = GameManager.Instance;
            gm.SetState(GameState.RollPhase);

            int resultA = 0, resultB = 0;
            bool doneA = false, doneB = false;

            if (diceA != null) diceA.Roll(v => { resultA = v; doneA = true; });
            else { resultA = Random.Range(1, 7); doneA = true; }

            if (diceB != null) diceB.Roll(v => { resultB = v; doneB = true; });
            else { resultB = Random.Range(1, 7); doneB = true; }

            // Wait for both dice to settle
            yield return new WaitUntil(() => doneA && doneB);
            yield return new WaitForSeconds(pauseAfterRoll);

            // Override the GameManager's internal DiceRoller with the physical result
            gm.Dice.OverrideResult(resultA, resultB);
            int total = gm.RollAndMove();

            // Wait for token to finish hopping
            if (boardView != null)
                yield return new WaitUntil(() => !boardView.IsMoving);

            IsAnimating = false;
        }
    }
}
