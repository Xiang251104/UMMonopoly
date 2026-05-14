using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMMonopoly.Core;
using UMMonopoly.Entities;

namespace UMMonopoly.UI
{
    /// <summary>
    /// Manages 3D player tokens on the board.
    /// Tokens hop tile-by-tile around the board anchors when a player moves.
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        [Header("Board Setup")]
        [Tooltip("40 empty GameObjects placed around the board in tile order (0 = Main Gate).")]
        public Transform[] tileAnchors = new Transform[40];

        [Header("Token")]
        public GameObject playerTokenPrefab;
        public Color[] playerColors = { Color.red, Color.blue, Color.green, Color.yellow };

        [Header("Movement")]
        [Tooltip("How high the token bounces between tiles.")]
        public float hopHeight = 0.5f;
        [Tooltip("Time in seconds per tile hop.")]
        public float hopDuration = 0.18f;

        [Header("Landing Popup")]
        public TileLandingPopup landingPopup;

        public bool IsMoving { get; private set; }

        private readonly Dictionary<int, Transform> _tokens = new Dictionary<int, Transform>();
        private readonly Dictionary<int, int> _playerPositions = new Dictionary<int, int>();

        private void OnEnable()  => EventBus.OnPlayerMoved += HandleMoved;
        private void OnDisable() => EventBus.OnPlayerMoved -= HandleMoved;

        public void SpawnTokens(List<Player> players)
        {
            foreach (var p in players)
            {
                var go = Instantiate(playerTokenPrefab, GetAnchorPos(0), Quaternion.identity, transform);
                var r = go.GetComponentInChildren<Renderer>();
                if (r != null && p.Id < playerColors.Length)
                    r.material.color = playerColors[p.Id];

                _tokens[p.Id] = go.transform;
                _playerPositions[p.Id] = 0;

                // Tiny horizontal offset so tokens don't overlap exactly
                go.transform.position += new Vector3(p.Id * 0.2f, 0f, p.Id * 0.1f);
            }
        }

        private void HandleMoved(Player p, int destination)
        {
            if (!_tokens.TryGetValue(p.Id, out var token)) return;
            int from = _playerPositions.ContainsKey(p.Id) ? _playerPositions[p.Id] : 0;
            _playerPositions[p.Id] = destination;
            StartCoroutine(HopToDestination(token, p, from, destination));
        }

        private IEnumerator HopToDestination(Transform token, Player player, int from, int destination)
        {
            IsMoving = true;
            int boardSize = tileAnchors.Length;
            int current = from;

            while (current != destination)
            {
                int next = (current + 1) % boardSize;
                yield return StartCoroutine(HopOnce(token, GetAnchorPos(current), GetAnchorPos(next)));
                current = next;
            }

            IsMoving = false;

            // Show landing popup after movement finishes
            var gm = GameManager.Instance;
            if (gm != null && landingPopup != null)
            {
                var tile = gm.Board.GetTile(destination);
                landingPopup.Show(tile.Data);
            }
        }

        private IEnumerator HopOnce(Transform token, Vector3 from, Vector3 to)
        {
            float elapsed = 0f;
            while (elapsed < hopDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / hopDuration;
                // Arc: lerp XZ, add sine curve on Y for hop
                Vector3 pos = Vector3.Lerp(from, to, t);
                pos.y += Mathf.Sin(t * Mathf.PI) * hopHeight;
                token.position = pos;
                yield return null;
            }
            token.position = to;
        }

        private Vector3 GetAnchorPos(int tileIndex)
        {
            if (tileIndex < 0 || tileIndex >= tileAnchors.Length || tileAnchors[tileIndex] == null)
                return Vector3.zero;
            return tileAnchors[tileIndex].position;
        }
    }
}
