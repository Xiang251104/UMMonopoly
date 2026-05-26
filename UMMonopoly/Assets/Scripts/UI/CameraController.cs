using UnityEngine;
using UMMonopoly.Core;
using UMMonopoly.Entities;

namespace UMMonopoly.UI
{
    /// <summary>
    /// Camera state machine for UMMonopoly.
    ///   Overview       → full board view (game start / Tab key)
    ///   TurnFocus      → zooms to current player's tile at turn start
    ///   MovementFollow → smoothly follows token as it hops
    ///   LandingZoom    → close-up on landing tile for a few seconds
    ///
    /// Manual controls (always active):
    ///   Right-drag → orbit   |   Scroll → zoom   |   Tab → Overview
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("References")]
        public BoardView boardView;

        [Header("Distances per state")]
        public float overviewDistance  = 18f;
        public float followDistance    =  9f;
        public float landingDistance   =  5f;

        [Header("Timing")]
        public float landingZoomDuration = 2.5f;   // seconds to stay in close-up
        public float pivotSmooth         = 6f;      // how fast pivot slides
        public float distanceSmooth      = 4f;      // how fast zoom changes

        [Header("Angle")]
        public float startPitch  = 62f;
        public float startYaw    =  0f;
        public float minPitch    = 28f;
        public float maxPitch    = 88f;

        [Header("Manual orbit")]
        public float orbitSpeed  = 180f;
        public float zoomSpeed   = 15f;
        public float minDistance =  2f;
        public float maxDistance = 22f;

        // ── internal state ───────────────────────────────────────────────────
        private enum CamState { Overview, TurnFocus, MovementFollow, LandingZoom }
        private CamState _state = CamState.Overview;

        private float   _pitch;
        private float   _yaw;
        private float   _distance;        // current (smoothed)
        private float   _targetDistance;
        private Vector3 _pivot;           // current (smoothed)
        private Vector3 _targetPivot;

        private bool    _prevMoving;
        private float   _landingTimer;
        private int     _currentPlayerIdx;

        // ── lifecycle ────────────────────────────────────────────────────────
        private void Awake()
        {
            _pitch          = startPitch;
            _yaw            = startYaw;
            _distance       = overviewDistance;
            _targetDistance = overviewDistance;
            _pivot = _targetPivot = Vector3.zero;
        }

        private void OnEnable()
        {
            EventBus.OnTurnStarted  += HandleTurnStarted;
            EventBus.OnPlayerMoved  += HandlePlayerMoved;
        }

        private void OnDisable()
        {
            EventBus.OnTurnStarted  -= HandleTurnStarted;
            EventBus.OnPlayerMoved  -= HandlePlayerMoved;
        }

        // ── event handlers ───────────────────────────────────────────────────
        private void HandleTurnStarted(int idx)
        {
            _currentPlayerIdx = idx;
            _state            = CamState.TurnFocus;
            _pitch            = startPitch;
            _yaw              = startYaw;
            _targetDistance   = followDistance;

            if (GameManager.Instance != null)
                SetPivotToTile(GameManager.Instance.Players[idx].BoardPosition);
        }

        private void HandlePlayerMoved(Player p, int destination)
        {
            if (p.Id != _currentPlayerIdx) return;
            _state          = CamState.MovementFollow;
            _targetDistance = followDistance;
            // pivot will track token live in Update
        }

        // ── Update ───────────────────────────────────────────────────────────
        private void Update()
        {
            HandleManualInput();
            UpdateStateMachine();
            ApplyCamera();
        }

        private void HandleManualInput()
        {
            // Tab → Overview
            if (Input.GetKeyDown(KeyCode.Tab))
                GoToOverview();

            // Right-mouse drag → orbit
            if (Input.GetMouseButton(1))
            {
                _yaw   += Input.GetAxis("Mouse X") * orbitSpeed * Time.deltaTime;
                _pitch -= Input.GetAxis("Mouse Y") * orbitSpeed * Time.deltaTime;
                _pitch  = Mathf.Clamp(_pitch, minPitch, maxPitch);
            }

            // Scroll → zoom target distance
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                _targetDistance -= scroll * zoomSpeed;
                _targetDistance  = Mathf.Clamp(_targetDistance, minDistance, maxDistance);
            }
        }

        private void UpdateStateMachine()
        {
            bool isMoving = boardView != null && boardView.IsMoving;

            switch (_state)
            {
                case CamState.Overview:
                    // nothing auto; manual controls only
                    break;

                case CamState.TurnFocus:
                    // once movement begins, switch to follow
                    if (isMoving)
                    {
                        _state          = CamState.MovementFollow;
                        _targetDistance = followDistance;
                    }
                    break;

                case CamState.MovementFollow:
                    // track the moving token live
                    if (boardView != null)
                    {
                        var tokenTf = boardView.GetPlayerToken(_currentPlayerIdx);
                        if (tokenTf != null) _targetPivot = tokenTf.position;
                    }

                    // detect when movement stops → LandingZoom
                    if (_prevMoving && !isMoving)
                    {
                        _state          = CamState.LandingZoom;
                        _targetDistance = landingDistance;
                        _landingTimer   = landingZoomDuration;
                        // snap pivot to anchor so it's exact
                        if (GameManager.Instance != null)
                            SetPivotToTile(GameManager.Instance.CurrentPlayer.BoardPosition);
                    }
                    break;

                case CamState.LandingZoom:
                    _landingTimer -= Time.deltaTime;
                    if (_landingTimer <= 0f)
                    {
                        // pull back to follow distance at landing tile
                        _state          = CamState.TurnFocus;
                        _targetDistance = followDistance;
                    }
                    break;
            }

            _prevMoving = isMoving;
        }

        private void ApplyCamera()
        {
            // Smooth pivot and distance
            _pivot    = Vector3.Lerp(_pivot,    _targetPivot,    pivotSmooth    * Time.deltaTime);
            _distance = Mathf.Lerp(_distance,   _targetDistance, distanceSmooth * Time.deltaTime);

            // Spherical → world position
            Vector3 offset = Quaternion.Euler(_pitch, _yaw, 0) * Vector3.back * _distance;
            transform.position = _pivot + offset;
            transform.LookAt(_pivot + Vector3.up * 0.3f);
        }

        // ── helpers ──────────────────────────────────────────────────────────
        private void GoToOverview()
        {
            _state          = CamState.Overview;
            _targetPivot    = Vector3.zero;
            _targetDistance = overviewDistance;
            _pitch          = startPitch;
            _yaw            = startYaw;
        }

        private void SetPivotToTile(int tileIndex)
        {
            if (boardView == null) return;
            if (tileIndex < 0 || tileIndex >= boardView.tileAnchors.Length) return;
            var anchor = boardView.tileAnchors[tileIndex];
            if (anchor != null) _targetPivot = anchor.position;
        }
    }
}
