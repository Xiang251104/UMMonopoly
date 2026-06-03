using System;
using System.Collections;
using UnityEngine;

namespace UMMonopoly.Systems
{
    /// <summary>
    /// Drop-in replacement for DicePhysics.
    /// Coroutine-driven tumble: die rises, spins chaotically, then snaps to the face
    /// matching the rolled number.  No Rigidbody needed.
    ///
    /// Face setup (same as DicePhysics):
    ///   Add six child GameObjects named Face1–Face6.
    ///   Rotate each child so its local +Y axis points outward from that face of the cube.
    ///   Assign them to faces[0]–faces[5] in the Inspector (faces[0] = value 1, etc.).
    /// </summary>
    public class DiceAnimator : MonoBehaviour
    {
        [Header("Face References")]
        [Tooltip("Six children with local +Y pointing out from each face. faces[0]=value 1 … faces[5]=value 6.")]
        public Transform[] faces = new Transform[6];

        [Header("Timing (seconds)")]
        public float riseTime   = 0.20f;
        public float tumbleTime = 1.10f;  // 1.0–1.5 s of chaotic spin
        public float landTime   = 0.35f;  // smooth slerp onto correct face
        public float dropTime   = 0.18f;

        [Header("Motion")]
        [Tooltip("World-units the die lifts during the tumble phase.")]
        public float riseHeight = 0.5f;
        [Tooltip("Approximate degrees per second during the chaotic spin.")]
        public float spinSpeed  = 900f;

        private Vector3 _restLocalPos;

        private void Awake()
        {
            _restLocalPos = transform.localPosition;
        }

        // ── Public API (same signature as DicePhysics) ─────────────────────────

        /// <summary>
        /// Picks a random result (1–6), plays the full tumble animation, then calls
        /// onResult with the value so TurnController can proceed.
        /// </summary>
        public void Roll(Action<int> onResult)
        {
            int result = UnityEngine.Random.Range(1, 7);
            StartCoroutine(AnimateRoll(result, onResult));
        }

        // ── Animation coroutine ────────────────────────────────────────────────

        private IEnumerator AnimateRoll(int result, Action<int> onResult)
        {
            Vector3 riseLocalPos = _restLocalPos + Vector3.up * riseHeight;

            // 1 ── Lift off
            yield return MoveLocal(_restLocalPos, riseLocalPos, riseTime, ease: true);

            // 2 ── Chaotic tumble: axis slowly drifts to keep motion unpredictable
            Vector3 axis    = UnityEngine.Random.onUnitSphere;
            float   elapsed = 0f;
            while (elapsed < tumbleTime)
            {
                float dt = Time.deltaTime;
                elapsed += dt;
                // Blend to a new random axis a few times per second → feels genuinely random
                axis = Vector3.Slerp(axis, UnityEngine.Random.onUnitSphere, dt * 4f).normalized;
                transform.Rotate(axis, spinSpeed * dt, Space.World);
                yield return null;
            }

            // 3 ── Gracefully snap to the correct face
            Quaternion targetRot = FaceUpRotation(result - 1);
            yield return RotateTo(targetRot, landTime);

            // 4 ── Settle back down
            yield return MoveLocal(riseLocalPos, _restLocalPos, dropTime, ease: true);

            onResult?.Invoke(result);
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the world rotation that places faces[faceIndex]'s outward normal
        /// pointing straight up.
        /// </summary>
        private Quaternion FaceUpRotation(int faceIndex)
        {
            if (faceIndex < 0 || faceIndex >= faces.Length || faces[faceIndex] == null)
                return transform.rotation;   // no face data — leave wherever it stopped

            // faces[i].localRotation * Vector3.up = face's outward normal in die-local space.
            // We want the die's world rotation to bring that local direction to world-up.
            Vector3 faceNormalLocal = faces[faceIndex].localRotation * Vector3.up;
            return Quaternion.FromToRotation(faceNormalLocal, Vector3.up);
        }

        private IEnumerator MoveLocal(Vector3 from, Vector3 to, float duration, bool ease)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                if (ease) t = Mathf.SmoothStep(0f, 1f, t);
                transform.localPosition = Vector3.LerpUnclamped(from, to, t);
                yield return null;
            }
            transform.localPosition = to;
        }

        private IEnumerator RotateTo(Quaternion target, float duration)
        {
            Quaternion start   = transform.rotation;
            float      elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                transform.rotation = Quaternion.SlerpUnclamped(start, target, t);
                yield return null;
            }
            transform.rotation = target;
        }
    }
}
