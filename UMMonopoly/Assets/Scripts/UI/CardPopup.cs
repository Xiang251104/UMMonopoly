using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UMMonopoly.Entities;

namespace UMMonopoly.UI
{
    /// <summary>
    /// Animated card draw popup.
    /// Slide-up from bottom + scale 0.5→1 + fade 0→1 on show, reverse on hide.
    /// Shows the card sprite full-bleed (text is baked into the artwork).
    /// Falls back to a text-only display if a card has no image assigned.
    /// </summary>
    public class CardPopup : MonoBehaviour
    {
        [Header("Panel")]
        public GameObject panelRoot;
        public CanvasGroup canvasGroup;

        [Header("Card visual")]
        public RectTransform cardRect;
        public Image cardImage;

        [Header("Fallback (used only if card has no image)")]
        public GameObject fallbackPanel;
        public TMP_Text fallbackDeckLabel;
        public TMP_Text fallbackDescriptionLabel;

        [Header("Buttons")]
        public Button closeButton;
        public Button backdropButton;

        [Header("Animation")]
        public float fadeInDuration  = 0.40f;
        public float fadeOutDuration = 0.25f;
        [Tooltip("Pixels the card slides up from during entry animation.")]
        public float slideDistance   = 400f;
        public float startScale      = 0.5f;

        private Vector2   _restAnchoredPos;
        private Coroutine _activeAnim;

        private void Awake()
        {
            if (cardRect != null) _restAnchoredPos = cardRect.anchoredPosition;
        }

        private void OnEnable()
        {
            EventBus.OnCardDrawn += HandleCardDrawn;
            if (closeButton    != null) closeButton.onClick.AddListener(Hide);
            if (backdropButton != null) backdropButton.onClick.AddListener(Hide);
            HideImmediate();
        }

        private void OnDisable()
        {
            EventBus.OnCardDrawn -= HandleCardDrawn;
            if (closeButton    != null) closeButton.onClick.RemoveListener(Hide);
            if (backdropButton != null) backdropButton.onClick.RemoveListener(Hide);
        }

        // ── Public API ─────────────────────────────────────────────────────────

        private void HandleCardDrawn(Player p, Card c)
        {
            if (c == null) return;

            bool hasImage = c.Image != null;

            // Image-first display
            if (cardImage != null)
            {
                cardImage.sprite  = c.Image;
                cardImage.enabled = hasImage;
            }

            // Fallback text only when no sprite
            if (fallbackPanel != null) fallbackPanel.SetActive(!hasImage);
            if (!hasImage)
            {
                if (fallbackDeckLabel        != null) fallbackDeckLabel.text        = $"{p.Name} draws a {c.Data.deck} Card";
                if (fallbackDescriptionLabel != null) fallbackDescriptionLabel.text = c.Description;
            }

            ShowAnimated();
        }

        public void Hide()
        {
            if (panelRoot == null || !panelRoot.activeSelf) return;
            if (_activeAnim != null) StopCoroutine(_activeAnim);
            _activeAnim = StartCoroutine(HideRoutine());
        }

        // ── Animation ──────────────────────────────────────────────────────────

        private void ShowAnimated()
        {
            if (panelRoot == null) return;
            panelRoot.SetActive(true);

            if (_activeAnim != null) StopCoroutine(_activeAnim);
            _activeAnim = StartCoroutine(ShowRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable   = true;
            }

            Vector2 startPos = _restAnchoredPos + new Vector2(0f, -slideDistance);
            Vector2 endPos   = _restAnchoredPos;
            Vector3 startScl = Vector3.one * startScale;
            Vector3 endScl   = Vector3.one;

            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / fadeInDuration));
                if (canvasGroup != null) canvasGroup.alpha = t;
                if (cardRect    != null)
                {
                    cardRect.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, t);
                    cardRect.localScale       = Vector3.LerpUnclamped(startScl, endScl, t);
                }
                yield return null;
            }

            if (canvasGroup != null) canvasGroup.alpha = 1f;
            if (cardRect    != null)
            {
                cardRect.anchoredPosition = endPos;
                cardRect.localScale       = endScl;
            }

            _activeAnim = null;
        }

        private IEnumerator HideRoutine()
        {
            Vector2 startPos = cardRect != null ? cardRect.anchoredPosition : Vector2.zero;
            Vector2 endPos   = _restAnchoredPos + new Vector2(0f, -slideDistance);
            Vector3 startScl = cardRect != null ? cardRect.localScale : Vector3.one;
            Vector3 endScl   = Vector3.one * startScale;

            float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;

            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / fadeOutDuration));
                if (canvasGroup != null) canvasGroup.alpha = Mathf.LerpUnclamped(startAlpha, 0f, t);
                if (cardRect    != null)
                {
                    cardRect.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, t);
                    cardRect.localScale       = Vector3.LerpUnclamped(startScl, endScl, t);
                }
                yield return null;
            }

            HideImmediate();
            _activeAnim = null;
        }

        private void HideImmediate()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha          = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable   = false;
            }
            if (panelRoot != null) panelRoot.SetActive(false);
            if (cardRect  != null)
            {
                cardRect.anchoredPosition = _restAnchoredPos;
                cardRect.localScale       = Vector3.one;
            }
        }
    }
}
