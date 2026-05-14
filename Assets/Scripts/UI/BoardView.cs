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
            EventBus.OnSentToJail += HandleSentToJail;
        }

        private void OnDisable()
        {
            EventBus.OnPlayerMoved -= HandleMoved;
            EventBus.OnSentToJail -= HandleSentToJail;
        }

        public void SpawnTokens(List<Player> players)
        {
            foreach (var token in _tokens.Values)
            {
                if (token != null) Destroy(token.gameObject);
            }
            _tokens.Clear();

            foreach (var p in players)
            {
                var t = Instantiate(playerTokenPrefab, transform).transform;
                if (p.Id < playerColors.Length)
                {
                    var spriteRenderer = t.GetComponentInChildren<SpriteRenderer>();
                    if (spriteRenderer != null) spriteRenderer.color = playerColors[p.Id];

                    var meshRenderer = t.GetComponentInChildren<MeshRenderer>();
                    if (meshRenderer != null) meshRenderer.sharedMaterial = CreateTokenMaterial(playerColors[p.Id]);
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

        private void HandleSentToJail(Player p)
        {
            HandleMoved(p, p.BoardPosition);
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

        private static Material CreateTokenMaterial(Color color)
        {
            var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color");
            var material = new Material(shader);
            material.color = color;
            return material;
        }
    }
}
