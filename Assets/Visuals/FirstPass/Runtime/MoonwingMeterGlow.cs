using UnityEngine;
using UnityEngine.UI;

namespace Moonwing.Visuals
{
    // Reads the existing slider; never writes its value or touches FairyMagic.
    public sealed class MoonwingMeterGlow : MonoBehaviour
    {
        public Slider meter;
        public Image fill;
        public Image halo;
        public Color fullColor = new Color(0.65f, 0.88f, 1f);
        public Color lowColor = new Color(0.62f, 0.42f, 0.86f);
        void Update()
        {
            if (!meter || !fill) return;
            float remaining = meter.normalizedValue;
            float pulse = 0.91f + Mathf.Sin(Time.unscaledTime * (remaining < 0.22f ? 2.5f : 1.1f)) * 0.09f;
            Color color = Color.Lerp(lowColor, fullColor, Mathf.SmoothStep(0, 1, remaining / 0.5f));
            fill.color = color;
            if (halo) halo.color = new Color(color.r, color.g, color.b, 0.25f * pulse * Mathf.Lerp(0.6f, 1, remaining));
        }
    }
}
