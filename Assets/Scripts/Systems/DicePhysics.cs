using System;
using System.Collections;
using UnityEngine;

namespace UMMonopoly.Systems
{
    /// <summary>
    /// Attach to a 3D cube GameObject with a Rigidbody.
    /// Throws the die with random force + torque, waits for it to settle,
    /// then reads which face is pointing up and reports the value via callback.
    ///
    /// Face setup: the die cube must have child GameObjects named
    /// "Face1" through "Face6", each with its local +Y axis pointing
    /// outward from that face (i.e. Face1's up = the 1-side normal).
    /// A quick way to set this up in Unity: duplicate a cube 6 times as children,
    /// scale them flat, position each flush with a face, rotate so local Y faces out.
    /// </summary>
    public class DicePhysics : MonoBehaviour
    {
        [Header("References")]
        public Rigidbody rb;
        public Transform[] faces = new Transform[6]; // faces[0] = face value 1, etc.

        [Header("Throw Force")]
        public float minForce = 3f;
        public float maxForce = 6f;
        public float minTorque = 200f;
        public float maxTorque = 500f;

        [Header("Settle Detection")]
        [Tooltip("Velocity magnitude below which the die is considered settled.")]
        public float settleThreshold = 0.05f;
        [Tooltip("How many seconds the die must stay still before we read its value.")]
        public float settleHoldTime = 0.3f;
        [Tooltip("Max seconds to wait before force-reading.")]
        public float maxWaitTime = 5f;

        private Vector3 _restPosition;

        private void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody>();
            _restPosition = transform.position;
        }

        /// <summary>
        /// Throw the die and invoke callback with result (1–6) once settled.
        /// </summary>
        public void Roll(Action<int> onResult)
        {
            StartCoroutine(RollCoroutine(onResult));
        }

        private IEnumerator RollCoroutine(Action<int> onResult)
        {
            // Reset
            rb.isKinematic = false;
            transform.position = _restPosition;
            transform.rotation = UnityEngine.Random.rotation;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Throw
            rb.AddForce(new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(0.5f, 1f),
                UnityEngine.Random.Range(-1f, 1f)
            ).normalized * UnityEngine.Random.Range(minForce, maxForce), ForceMode.Impulse);

            rb.AddTorque(new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f),
                UnityEngine.Random.Range(-1f, 1f)
            ).normalized * UnityEngine.Random.Range(minTorque, maxTorque));

            // Wait for settle
            float holdTimer = 0f;
            float totalTimer = 0f;

            yield return new WaitForSeconds(0.5f); // give it time to actually start moving

            while (totalTimer < maxWaitTime)
            {
                totalTimer += Time.deltaTime;
                bool settled = rb.linearVelocity.magnitude < settleThreshold
                            && rb.angularVelocity.magnitude < settleThreshold;
                if (settled)
                {
                    holdTimer += Time.deltaTime;
                    if (holdTimer >= settleHoldTime) break;
                }
                else
                {
                    holdTimer = 0f;
                }
                yield return null;
            }

            rb.isKinematic = true;

            int result = ReadTopFace();
            onResult?.Invoke(result);
        }

        private int ReadTopFace()
        {
            // Find which face's local up (+Y) is most aligned with world up
            int best = 0;
            float bestDot = float.MinValue;

            for (int i = 0; i < faces.Length; i++)
            {
                if (faces[i] == null) continue;
                float dot = Vector3.Dot(faces[i].up, Vector3.up);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    best = i;
                }
            }

            return best + 1; // faces[0] = face value 1
        }
    }
}
