using UnityEngine;
using UnityEngine.UI;

namespace Moonwing.Visuals
{
    [DefaultExecutionOrder(-10000)]
    public sealed class MoonwingOpening : MonoBehaviour
    {
        public Behaviour[] gameplay;
        public CanvasGroup overlay;
        public CanvasGroup narration;
        public CanvasGroup title;
        public CanvasGroup meter;
        public Button playButton;
        public GameObject[] endingPanels;
        public float narrationSeconds = 14f;
        public TMPro.TextMeshProUGUI narrationText;
        public CanvasGroup words;
        public Button skipButton;
        public MoonwingIntroMist introMist;
        static readonly string[] story = {
            "Welcome to Moonwing Forest.",
            "Here lives the Moonfairy, blessed with magic by the Moon Goddess herself.",
            "She guards a rare potion born of that magic—one coveted by the wealthy and powerful.",
            "I am her caretaker. And I have learned that when this forest grows quiet... we should be afraid."
        };
        bool[] enabledBefore;
        float elapsed;
        float reveal;
        float previousTimeScale;
        bool playing;

        void Awake()
        {
            previousTimeScale = Time.timeScale > 0 ? Time.timeScale : 1;
            Time.timeScale = 0;
            foreach (var panel in endingPanels) if (panel) panel.SetActive(false);
            enabledBefore = new bool[gameplay.Length];
            for (int i = 0; i < gameplay.Length; i++)
            {
                if (!gameplay[i]) continue;
                enabledBefore[i] = gameplay[i].enabled;
                gameplay[i].enabled = false;
            }
            overlay.alpha = 1;
            overlay.blocksRaycasts = true;
            narration.alpha = 0;
            title.alpha = 0;
            title.interactable = false;
            playButton.interactable = false;
            meter.alpha = 0;
            if (skipButton) skipButton.interactable = true;
        }
        void Update()
        {
            if (playing)
            {
                reveal += Time.unscaledDeltaTime / 0.85f;
                overlay.alpha = 1 - Mathf.SmoothStep(0, 1, reveal);
                meter.alpha = Mathf.SmoothStep(0, 1, reveal);
                if (reveal >= 1) gameObject.SetActive(false);
                return;
            }
            elapsed += Time.unscaledDeltaTime;
            narration.alpha = Mathf.Clamp01(elapsed / 0.9f) * Mathf.Clamp01((narrationSeconds - elapsed) / 2f);
            if (narrationText && elapsed < narrationSeconds)
            {
                float segment = narrationSeconds / story.Length;
                int index = Mathf.Min(story.Length - 1, (int)(elapsed / segment));
                narrationText.text = story[index];
                float local = elapsed - index * segment;
                if (words) words.alpha = Mathf.Clamp01(local / 0.5f) * Mathf.Clamp01((segment - local) / 0.6f);
            }
            if (elapsed >= narrationSeconds && introMist) introMist.Clear();
            title.alpha = Mathf.Clamp01((elapsed - narrationSeconds - 0.35f) / 1.15f);
            bool ready = elapsed >= narrationSeconds + 1.5f;
            title.interactable = ready;
            playButton.interactable = ready;
            narration.blocksRaycasts = elapsed < narrationSeconds;
            if (skipButton) skipButton.gameObject.SetActive(elapsed < narrationSeconds);
        }
        public void Skip()
        {
            if (playing) return;
            if (elapsed >= narrationSeconds) return;
            elapsed = narrationSeconds;
            narration.alpha = 0;
            narration.blocksRaycasts = false;
            title.alpha = 0;
            title.interactable = false;
            playButton.interactable = false;
            if (introMist) introMist.Clear();
            if (skipButton) skipButton.gameObject.SetActive(false);
        }
        public void Play()
        {
            if (playing || !playButton.interactable) return;
            playing = true;
            overlay.interactable = false;
            overlay.blocksRaycasts = false;
            Time.timeScale = previousTimeScale;
            if (Camera.main && Camera.main.TryGetComponent<CameraFollow>(out var camera)) camera.CapturePointer();
            for (int i = 0; i < gameplay.Length; i++)
                if (gameplay[i]) gameplay[i].enabled = enabledBefore[i];
        }
        void OnDestroy()
        {
            // Also leave editor Play mode/reloads with a usable time scale.
            if (!playing) Time.timeScale = previousTimeScale > 0 ? previousTimeScale : 1;
        }
    }
}
