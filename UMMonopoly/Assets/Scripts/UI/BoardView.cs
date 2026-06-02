using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UMMonopoly.Core;
using UMMonopoly.Data;
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
        [Tooltip("Y offset above anchor so tokens sit on top of tile cubes.")]
        public float tokenHeightOffset = 0.35f;
        [Tooltip("How high the token bounces between tiles.")]
        public float hopHeight = 0.5f;
        [Tooltip("Time in seconds per tile hop.")]
        public float hopDuration = 0.18f;

        [Header("Landing Popup")]
        public TileLandingPopup landingPopup;

        [Header("Token Highlight")]
        [Tooltip("Constant emission strength for non-active player tokens.")]
        public float inactiveEmission = 0.5f;
        [Tooltip("Active player emission pulses between these two multipliers.")]
        public float activeEmissionMin = 0.8f;
        public float activeEmissionMax = 1.4f;
        [Tooltip("Seconds for one full pulse cycle.")]
        public float pulsePeriod = 1.5f;
        [Header("Token Material (glossy plastic look)")]
        [Range(0f, 1f)] public float tokenMetallic   = 0.2f;
        [Range(0f, 1f)] public float tokenSmoothness = 0.7f;

        public bool IsMoving { get; private set; }

        public Transform GetPlayerToken(int playerId) =>
            _tokens.TryGetValue(playerId, out var t) ? t : null;

        private readonly Dictionary<int, Transform>  _tokens = new Dictionary<int, Transform>();
        private readonly Dictionary<int, int>        _playerPositions = new Dictionary<int, int>();
        private readonly Dictionary<int, Material>   _mainMats = new Dictionary<int, Material>();
        private readonly Dictionary<int, Material>   _haloMats = new Dictionary<int, Material>();
        private readonly Dictionary<int, GameObject> _haloObjects = new Dictionary<int, GameObject>();
        // Ownership markers keyed by tile position (0..39). Each tile holds a list of
        // cubes — index 0 is the base ownership cube; subsequent indexes are upgrade "houses".
        private readonly Dictionary<int, List<GameObject>> _ownerMarkers = new Dictionary<int, List<GameObject>>();
        private int _activePlayerId = -1;

        const float HaloMinAlpha = 0.35f;
        const float HaloMaxAlpha = 0.65f;

        private void OnEnable()
        {
            EventBus.OnPlayerMoved       += HandleMoved;
            EventBus.OnTurnStarted       += HandleTurnStarted;
            EventBus.OnTilePurchased     += HandleTilePurchased;
            EventBus.OnPropertyUpgraded  += HandlePropertyUpgraded;
            EventBus.OnSentToJail        += HandleSentToJail;
            EventBus.OnTradeCompleted    += HandleTradeCompleted;
        }

        private void OnDisable()
        {
            EventBus.OnPlayerMoved       -= HandleMoved;
            EventBus.OnTurnStarted       -= HandleTurnStarted;
            EventBus.OnTilePurchased     -= HandleTilePurchased;
            EventBus.OnPropertyUpgraded  -= HandlePropertyUpgraded;
            EventBus.OnSentToJail        -= HandleSentToJail;
            EventBus.OnTradeCompleted    -= HandleTradeCompleted;
        }

        /// <summary>
        /// Recolors every ownership marker on tiles transferred via a trade.
        /// Marker count (= 1 + UpgradeLevel) is unchanged; only the color flips to the new owner.
        /// </summary>
        private void HandleTradeCompleted(TradeOffer offer)
        {
            if (offer == null) return;
            // After the trade, FromGives belong to To and ToGives belong to From.
            foreach (var t in offer.FromGives) RecolorMarkersFor(t, offer.To);
            foreach (var t in offer.ToGives)   RecolorMarkersFor(t, offer.From);
        }

        private void RecolorMarkersFor(Tile tile, Player newOwner)
        {
            if (tile == null || tile.Data == null || newOwner == null) return;
            int pos = tile.Data.position;
            if (pos < 0 || pos >= tileAnchors.Length) return;
            if (!_ownerMarkers.TryGetValue(pos, out var markers) || markers == null) return;

            Color color = newOwner.Id < playerColors.Length ? playerColors[newOwner.Id] : Color.white;
            foreach (var m in markers)
                if (m != null) ApplyMarkerColor(m, color);
        }

        /// <summary>
        /// Snaps the token instantly to the jail tile (no hop animation) when a player
        /// is sent to jail by GoToJailTile or a card. Player.BoardPosition is already updated
        /// at this point by the backend.
        /// </summary>
        private void HandleSentToJail(Player player)
        {
            if (player == null) return;
            if (!_tokens.TryGetValue(player.Id, out var token) || token == null) return;
            token.position = GetAnchorPos(player.BoardPosition);
            _playerPositions[player.Id] = player.BoardPosition;
        }

        private void Update()
        {
            // Pulse only the active player's emission + halo alpha.
            if (_activePlayerId < 0) return;
            if (_activePlayerId >= playerColors.Length) return;

            float sin01 = (Mathf.Sin(Time.time * 2f * Mathf.PI / pulsePeriod) + 1f) * 0.5f;
            Color baseColor = playerColors[_activePlayerId];

            // Token emission
            if (_mainMats.TryGetValue(_activePlayerId, out var mat) && mat != null)
            {
                float multiplier = Mathf.Lerp(activeEmissionMin, activeEmissionMax, sin01);
                mat.SetColor("_EmissionColor", baseColor * multiplier);
            }

            // Halo alpha (only the active player has a visible halo)
            if (_haloMats.TryGetValue(_activePlayerId, out var haloMat) && haloMat != null)
            {
                float a = Mathf.Lerp(HaloMinAlpha, HaloMaxAlpha, sin01);
                haloMat.SetColor("_BaseColor", new Color(baseColor.r, baseColor.g, baseColor.b, a));
            }
        }

        public void SpawnTokens(List<Player> players)
        {
            var litShader   = Shader.Find("Universal Render Pipeline/Lit");
            var unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (litShader == null)   Debug.LogError("[BoardView] URP/Lit shader not found.");
            if (unlitShader == null) Debug.LogError("[BoardView] URP/Unlit shader not found.");

            foreach (var p in players)
            {
                var go = Instantiate(playerTokenPrefab, GetAnchorPos(0), Quaternion.identity, transform);
                Color playerColor = p.Id < playerColors.Length ? playerColors[p.Id] : Color.white;

                // Find the TokenMesh (parent of pawn parts) and Halo children.
                var meshTf = go.transform.Find("TokenMesh");
                var haloTf = go.transform.Find("Halo");

                // Collect all renderers under TokenMesh (Base + Neck + Head share one material).
                Renderer[] meshRenderers;
                if (meshTf != null)
                {
                    meshRenderers = meshTf.GetComponentsInChildren<Renderer>();
                }
                else
                {
                    var single = go.GetComponentInChildren<Renderer>();
                    meshRenderers = single != null ? new[] { single } : new Renderer[0];
                }
                var haloRenderer = haloTf != null ? haloTf.GetComponent<Renderer>() : null;

                // Main token material — glossy plastic feel, shared across all pawn parts.
                if (meshRenderers.Length > 0 && litShader != null)
                {
                    var mainMat = new Material(litShader);
                    mainMat.SetColor("_BaseColor", playerColor);
                    mainMat.SetFloat("_Metallic",   tokenMetallic);
                    mainMat.SetFloat("_Smoothness", tokenSmoothness);
                    mainMat.EnableKeyword("_EMISSION");
                    mainMat.SetColor("_EmissionColor", playerColor * inactiveEmission);
                    mainMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                    foreach (var r in meshRenderers)
                    {
                        if (r != null) r.sharedMaterial = mainMat;
                    }
                    _mainMats[p.Id] = mainMat;
                }

                // Halo material — fresh URP/Unlit with transparent setup so we don't depend on prefab.
                if (haloRenderer != null && unlitShader != null)
                {
                    var haloMat = new Material(unlitShader);
                    haloMat.SetColor("_BaseColor",
                        new Color(playerColor.r, playerColor.g, playerColor.b, HaloMinAlpha));
                    haloMat.SetFloat("_Surface", 1f);
                    haloMat.SetFloat("_Blend",   0f);
                    haloMat.SetFloat("_ZWrite",  0f);
                    haloMat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                    haloMat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                    haloMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    haloMat.DisableKeyword("_ALPHATEST_ON");
                    haloMat.renderQueue = (int)RenderQueue.Transparent;
                    haloRenderer.sharedMaterial = haloMat;
                    _haloMats[p.Id]    = haloMat;
                    _haloObjects[p.Id] = haloTf.gameObject;
                    // Show halo only if this player is already the active one
                    // (handles event-order races where OnTurnStarted fired before SpawnTokens).
                    haloTf.gameObject.SetActive(p.Id == _activePlayerId);
                }

                _tokens[p.Id] = go.transform;
                _playerPositions[p.Id] = 0;

                // Tiny horizontal offset so tokens don't overlap exactly
                go.transform.position += new Vector3(p.Id * 0.2f, 0f, p.Id * 0.1f);
            }
        }

        /// <summary>
        /// Adds the base ownership cube to the corner of a newly bought tile.
        /// If the player already owns it (e.g., bankruptcy transfer in future), recolors all cubes.
        /// </summary>
        private void HandleTilePurchased(Player p, TileDataSO tile)
        {
            if (tile == null) return;
            if (tile.position < 0 || tile.position >= tileAnchors.Length) return;
            var anchor = tileAnchors[tile.position];
            if (anchor == null) return;

            Color color = p.Id < playerColors.Length ? playerColors[p.Id] : Color.white;

            if (!_ownerMarkers.TryGetValue(tile.position, out var markers) || markers == null)
            {
                markers = new List<GameObject>();
                _ownerMarkers[tile.position] = markers;
            }

            if (markers.Count == 0)
            {
                AddMarkerCube(anchor, color, 0, markers);
            }
            else
            {
                // Already has cubes — recolor them all (covers future ownership transfers)
                foreach (var m in markers)
                    if (m != null) ApplyMarkerColor(m, color);
            }
        }

        /// <summary>
        /// Each upgrade adds another "house" cube next to the existing ones on the tile.
        /// </summary>
        private void HandlePropertyUpgraded(Player p, PropertyTile pt)
        {
            if (pt == null) return;
            int pos = pt.Data.position;
            if (pos < 0 || pos >= tileAnchors.Length) return;
            var anchor = tileAnchors[pos];
            if (anchor == null) return;

            Color color = p.Id < playerColors.Length ? playerColors[p.Id] : Color.white;

            if (!_ownerMarkers.TryGetValue(pos, out var markers) || markers == null)
            {
                markers = new List<GameObject>();
                _ownerMarkers[pos] = markers;
            }
            AddMarkerCube(anchor, color, markers.Count, markers);
        }

        /// <summary>
        /// Spawns a single ownership/house cube at the given index along the tile corner.
        /// </summary>
        private void AddMarkerCube(Transform anchor, Color color, int index, List<GameObject> markers)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = $"OwnerMarker_{index}";
            marker.transform.SetParent(anchor, false);
            // Stack cubes along the tile's local Z axis: indexes 0..4 fan out from +Z to -Z
            float zOffset = 0.30f - index * 0.18f;
            marker.transform.localPosition = new Vector3(0.30f, 0.16f, zOffset);
            marker.transform.localScale    = new Vector3(0.16f, 0.12f, 0.16f);
            var col = marker.GetComponent<Collider>();
            if (col != null) Destroy(col);
            ApplyMarkerColor(marker, color);
            markers.Add(marker);
        }

        private void ApplyMarkerColor(GameObject marker, Color color)
        {
            var litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) return;
            var mat = new Material(litShader);
            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Metallic",   0.2f);
            mat.SetFloat("_Smoothness", 0.7f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 0.6f);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            marker.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private void HandleTurnStarted(int playerId)
        {
            // Reset previously-active player: steady inactive emission + hide halo.
            if (_activePlayerId >= 0 && _activePlayerId != playerId)
            {
                if (_mainMats.TryGetValue(_activePlayerId, out var prevMat)
                    && _activePlayerId < playerColors.Length)
                {
                    prevMat.SetColor("_EmissionColor", playerColors[_activePlayerId] * inactiveEmission);
                }
                if (_haloObjects.TryGetValue(_activePlayerId, out var prevHalo) && prevHalo != null)
                {
                    prevHalo.SetActive(false);
                }
            }

            _activePlayerId = playerId;

            // Show new active player's halo (Update will pulse it).
            if (_haloObjects.TryGetValue(_activePlayerId, out var newHalo) && newHalo != null)
            {
                newHalo.SetActive(true);
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
                // If the player was sent to jail mid-flight (e.g., they landed on GoToJail),
                // abort the hop and snap to wherever player.BoardPosition is now (jail tile).
                if (player.InJail)
                {
                    token.position = GetAnchorPos(player.BoardPosition);
                    _playerPositions[player.Id] = player.BoardPosition;
                    IsMoving = false;
                    yield break;
                }
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
            if (UMMonopoly.Systems.AudioManager.Instance != null)
                UMMonopoly.Systems.AudioManager.Instance.PlayTokenHop();

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
            return tileAnchors[tileIndex].position + Vector3.up * tokenHeightOffset;
        }
    }
}
