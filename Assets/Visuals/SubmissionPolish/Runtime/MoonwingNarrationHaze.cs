using UnityEngine;
using UnityEngine.UI;

namespace Moonwing.Visuals
{
    // Inherits the existing narration CanvasGroup fade; independent of the ground mist.
    public sealed class MoonwingNarrationHaze : MonoBehaviour
    {
        public Image haze;
        public Image[] stars;
        Texture2D texture;
        Sprite sprite;
        void Awake()
        {
            sprite = MoonwingDialogueBubble.CreateWordMist(out texture);
            haze.sprite = sprite;
        }
        void Update()
        {
            for (int i = 0; i < stars.Length; i++)
            {
                float t = Time.unscaledTime * 0.55f + i * 1.7f;
                Color color = new Color(0.85f, 0.84f, 1f, 0.22f + 0.18f * (0.5f + 0.5f * Mathf.Sin(t)));
                stars[i].color = color;
                stars[i].rectTransform.anchoredPosition = new Vector2(Mathf.Sin(t) * 3, Mathf.Cos(t * 0.7f) * 3);
            }
        }
        void OnDestroy()
        {
            if (sprite) Destroy(sprite);
            if (texture) Destroy(texture);
        }
    }
}
