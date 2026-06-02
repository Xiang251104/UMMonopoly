using UnityEngine;
using UMMonopoly.Data;
using UMMonopoly.Entities;

namespace UMMonopoly.Systems
{
    /// <summary>
    /// Centralised audio: plays a looping BGM track and routes EventBus events to one-shot SFX.
    /// Created and wired by SetupAudioManager (editor script).
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Volume")]
        [Range(0f, 1f)] public float sfxVolume = 0.7f;
        [Range(0f, 1f)] public float bgmVolume = 0.3f;
        [Tooltip("Boost factor applied to the dice loop relative to sfxVolume.")]
        [Range(0f, 2f)] public float diceVolumeMultiplier = 1.6f;

        [Header("Background Music")]
        public AudioClip backgroundMusic;

        [Header("SFX Clips")]
        public AudioClip diceRoll;
        public AudioClip buttonClick;
        public AudioClip propertyBought;
        public AudioClip moneyReceived;
        public AudioClip moneyPaid;
        public AudioClip cardDrawn;
        public AudioClip sentToJail;
        public AudioClip winFanfare;
        public AudioClip tokenHop;

        private AudioSource _sfxSource;
        private AudioSource _bgmSource;
        private AudioSource _diceSource;  // dedicated looping source — runs during dice animation

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.loop        = false;
            _sfxSource.volume      = sfxVolume;

            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.playOnAwake = false;
            _bgmSource.loop        = true;
            _bgmSource.volume      = bgmVolume;

            _diceSource = gameObject.AddComponent<AudioSource>();
            _diceSource.playOnAwake = false;
            _diceSource.loop        = true;
            _diceSource.volume      = Mathf.Clamp01(sfxVolume * diceVolumeMultiplier);
        }

        private void Start()
        {
            if (backgroundMusic != null)
            {
                _bgmSource.clip = backgroundMusic;
                _bgmSource.Play();
            }
        }

        private void OnEnable()
        {
            EventBus.OnDiceRollStarted += HandleDiceRollStarted;
            EventBus.OnDiceRollEnded   += HandleDiceRollEnded;
            EventBus.OnTilePurchased   += HandleTilePurchased;
            EventBus.OnMoneyChanged    += HandleMoneyChanged;
            EventBus.OnCardDrawn       += HandleCardDrawn;
            EventBus.OnSentToJail      += HandleSentToJail;
            EventBus.OnGameWon         += HandleGameWon;
        }

        private void OnDisable()
        {
            EventBus.OnDiceRollStarted -= HandleDiceRollStarted;
            EventBus.OnDiceRollEnded   -= HandleDiceRollEnded;
            EventBus.OnTilePurchased   -= HandleTilePurchased;
            EventBus.OnMoneyChanged    -= HandleMoneyChanged;
            EventBus.OnCardDrawn       -= HandleCardDrawn;
            EventBus.OnSentToJail      -= HandleSentToJail;
            EventBus.OnGameWon         -= HandleGameWon;
        }

        // ── Public API ─────────────────────────────────────────────────────────

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null || _sfxSource == null) return;
            // AudioSource.volume already scales the playback; passing sfxVolume here would double-scale.
            _sfxSource.PlayOneShot(clip);
        }

        /// <summary>Use this on Button.onClick to play the standard click sound.</summary>
        public void PlayButtonClick() => PlaySFX(buttonClick);

        /// <summary>Called once per single-tile hop by BoardView.</summary>
        public void PlayTokenHop() => PlaySFX(tokenHop);

        // ── Event handlers ─────────────────────────────────────────────────────

        private void HandleDiceRollStarted(Vector3 center)
        {
            if (diceRoll == null || _diceSource == null) return;
            _diceSource.clip   = diceRoll;
            _diceSource.volume = Mathf.Clamp01(sfxVolume * diceVolumeMultiplier);
            _diceSource.Play();
        }

        private void HandleDiceRollEnded()
        {
            if (_diceSource != null) _diceSource.Stop();
        }

        private void HandleTilePurchased(Player p, TileDataSO tile) => PlaySFX(propertyBought);
        private void HandleCardDrawn(Player p, Card c) => PlaySFX(cardDrawn);
        private void HandleSentToJail(Player p) => PlaySFX(sentToJail);
        private void HandleGameWon(Player winner) => PlaySFX(winFanfare);

        private void HandleMoneyChanged(Player p, int delta)
        {
            if (delta > 0)      PlaySFX(moneyReceived);
            else if (delta < 0) PlaySFX(moneyPaid);
        }
    }
}
