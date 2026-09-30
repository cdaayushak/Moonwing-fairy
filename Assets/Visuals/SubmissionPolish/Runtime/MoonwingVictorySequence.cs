using UnityEngine;
using UnityEngine.UI;
namespace Moonwing.Visuals
{
    // Activated only by the existing spawner victoryPanel reference, after its settling delay.
    public sealed class MoonwingVictorySequence : MonoBehaviour
    {
        public CanvasGroup confirmation;
        public CanvasGroup poem;
        public Image backdrop;
        float elapsed;
        bool started;
        public bool Completed => elapsed >= 4.8f;
        void OnEnable()
        {
            if (started) return;
            started = true;
            confirmation.alpha = 0; poem.alpha = 0;
        }
        void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            confirmation.alpha = Mathf.Clamp01(elapsed / 0.65f) * Mathf.Clamp01((3.5f - elapsed) / 0.65f);
            poem.alpha = Mathf.Clamp01((elapsed - 3.6f) / 1.2f);
            if (backdrop)
            {
                var color = backdrop.color;
                color.a = Mathf.Lerp(0.38f, 0.89f, poem.alpha);
                backdrop.color = color;
            }
        }
    }
}
