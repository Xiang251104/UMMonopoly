using System.Collections.Generic;
using UnityEngine;
using UMMonopoly.Core;
using UMMonopoly.Entities;

namespace UMMonopoly.UI
{
    /// <summary>
    /// Lays out 40 tiles around the screen edge and moves player tokens between them.
    /// Wire up an array of 40 Transform anchors in the Inspector (one per tile, in order).
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        public Transform[] tileAnchors = new Transform[40];
        public GameObject playerTokenPrefab;
        public Color[] playerColors = { Color.red, Color.blue, Color.green, Color.yellow };

        private readonly Dictionary<int, Transform> _tokens = new Dictionary<int, Transform>();

        private void OnEnable()
        {
            EventBus.OnPlayerMoved += HandleMoved;
        }

        private void OnDisable()
        {
            EventBus.OnPlayerMoved -= HandleMoved;
        }

        public void SpawnTokens(List<Player> players)
        {
            foreach (var p in players)
            {
                var t = Instantiate(playerTokenPrefab, transform).transform;
                if (p.Id < playerColors.Length)
                {
                    var sr = t.GetComponentInChildren<SpriteRenderer>();
                    if (sr != null) sr.color = playerColors[p.Id];
                }
                _tokens[p.Id] = t;
                MoveTokenTo(t, p.BoardPosition);
            }
        }

        private void HandleMoved(Player p, int pos)
        {
            if (!_tokens.TryGetValue(p.Id, out var t)) return;
            MoveTokenTo(t, pos);
        }

        private void MoveTokenTo(Transform token, int pos)
        {
            if (pos < 0 || pos >= tileAnchors.Length || tileAnchors[pos] == null) return;
            // Tiny offset by player id so tokens don't overlap on the same tile.
            int idOffset = 0;
            foreach (var kv in _tokens)
            {
                if (kv.Value == token) { idOffset = kv.Key; break; }
            }
            token.position = tileAnchors[pos].position + new Vector3(idOffset * 0.15f, idOffset * 0.15f, 0f);
        }
    }
}
