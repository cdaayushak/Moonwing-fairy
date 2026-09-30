using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Moonwing.Visuals
{
    /// <summary>Presentation only: queues already-selected lines without driving conversations.</summary>
    public sealed class MoonwingDialogueBubble : MonoBehaviour
    {
        struct Line { public Transform speaker; public string text; public float queuedAt; }
        static MoonwingDialogueBubble instance;
        readonly Queue<Line> pending = new Queue<Line>();
        CanvasGroup group;
        RectTransform panel;
        TextMeshProUGUI label;
        Transform speaker;
        Camera view;
        float elapsed, duration;
        RectTransform canvasRect;
        Collider speakerBody;
        bool snapPosition;
        Sprite mistSprite, moteSprite;
        Texture2D mistTexture, moteTexture;
        readonly Image[] motes = new Image[4];
        Color accent;

        public static void Show(Transform speaker, string text)
        {
            if (!speaker || string.IsNullOrEmpty(text)) return;
            if (!instance)
                instance = new GameObject("Moonwing Dialogue Bubbles").AddComponent<MoonwingDialogueBubble>();
            // Bound presentation backlog when multiple existing conversations fire together.
            if (instance.pending.Count >= 4) instance.pending.Dequeue();
            instance.pending.Enqueue(new Line { speaker = speaker, text = text, queuedAt = Time.time });
        }

        void Awake()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5;
            canvasRect = (RectTransform)canvas.transform;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1440, 900);
            scaler.matchWidthOrHeight = 0.5f;
            // No raycaster: dialogue cannot intercept movement, shooting or menu input.
            panel = new GameObject("Speaking NPC", typeof(RectTransform), typeof(CanvasGroup), typeof(Image)).GetComponent<RectTransform>();
            panel.SetParent(transform, false);
            panel.sizeDelta = new Vector2(364, 78);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0);
            group = panel.GetComponent<CanvasGroup>();
            group.alpha = 0;
            group.blocksRaycasts = false;
            group.interactable = false;
            var background = panel.GetComponent<Image>();
            var style = Resources.Load<MoonwingDialogueStyle>("MoonwingDialogueStyle");
            background.enabled = false;
            background.raycastTarget = false;
            BuildMist();

            label = new GameObject("Moonlit Words", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            label.rectTransform.SetParent(panel, false);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(22, 16);
            label.rectTransform.offsetMax = new Vector2(-22, -16);
            label.font = style && style.font ? style.font : TMP_Settings.defaultFontAsset;
            label.fontSharedMaterial = label.font.material;
            label.fontStyle = FontStyles.Normal;
            label.fontSize = 26;
            label.enableAutoSizing = false;
            label.lineSpacing = 3;
            label.characterSpacing = 0.3f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(0.97f, 0.95f, 0.90f);
            label.richText = false;
            label.raycastTarget = false;

        }

        public static Sprite CreateWordMist(out Texture2D mistTexture)
        {
            // One small reusable texture per dialogue canvas; no lights, particles or custom shader.
            mistTexture = new Texture2D(128, 64, TextureFormat.RGBA32, false);
            mistTexture.name = "NPC Moon Mist";
            mistTexture.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < 64; y++) for (int x = 0; x < 128; x++)
            {
                float u = x / 127f * 2 - 1, v = y / 63f * 2 - 1;
                float bend = 0.09f * Mathf.Sin(u * 7) + 0.06f * Mathf.Sin(u * 13 + 1);
                float radius = Mathf.Sqrt(u * u + Mathf.Pow((v + bend) / (0.83f + 0.10f * Mathf.Sin(u * 9)), 2));
                float core = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.55f, 1, radius));
                float edge = Mathf.SmoothStep(0, 1, Mathf.Min(1 - Mathf.Abs(u), 1 - Mathf.Abs(v)) / 0.16f);
                Color c = Color.Lerp(new Color(0.025f, 0.033f, 0.085f), new Color(0.18f, 0.15f, 0.28f), 1 - core);
                c.a = core * edge * 0.675f; // 27% more apparent than the approved first mist pass.
                mistTexture.SetPixel(x, y, c);
            }
            mistTexture.Apply(false, true);
            return Sprite.Create(mistTexture, new Rect(0, 0, 128, 64), Vector2.one * 0.5f);
        }

        void BuildMist()
        {
            mistSprite = CreateWordMist(out mistTexture);
            var mist = NewImage("Enchanted Word Mist", mistSprite);
            mist.rectTransform.anchorMin = Vector2.zero; mist.rectTransform.anchorMax = Vector2.one;
            mist.rectTransform.offsetMin = new Vector2(-48, -25); mist.rectTransform.offsetMax = new Vector2(48, 25);

            moteTexture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            moteTexture.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++)
            {
                float u = Mathf.Abs(x / 15f * 2 - 1), v = Mathf.Abs(y / 15f * 2 - 1);
                float a = Mathf.Pow(Mathf.Clamp01(1 - Mathf.Sqrt(u * u + v * v)), 2);
                a += Mathf.Pow(Mathf.Clamp01(1 - Mathf.Min(u, v) * 7), 2) * Mathf.Clamp01(1 - Mathf.Max(u, v)) * 0.45f;
                moteTexture.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(a)));
            }
            moteTexture.Apply(false, true);
            moteSprite = Sprite.Create(moteTexture, new Rect(0, 0, 16, 16), Vector2.one * 0.5f);
            for (int i = 0; i < motes.Length; i++)
            {
                motes[i] = NewImage("Quiet Mote " + i, moteSprite);
                motes[i].rectTransform.anchorMin = motes[i].rectTransform.anchorMax = new Vector2(i % 2 == 0 ? 0.08f : 0.92f, i < 2 ? 0.22f : 0.80f);
                motes[i].rectTransform.sizeDelta = Vector2.one * (i % 2 == 0 ? 5 : 7);
            }
        }

        Image NewImage(string name, Sprite sprite)
        {
            var image = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            image.rectTransform.SetParent(panel, false);
            image.sprite = sprite; image.raycastTarget = false;
            return image;
        }

        void OnDestroy()
        {
            if (mistSprite) Destroy(mistSprite);
            if (moteSprite) Destroy(moteSprite);
            if (mistTexture) Destroy(mistTexture);
            if (moteTexture) Destroy(moteTexture);
        }

        void LateUpdate()
        {
            if (!view) view = Camera.main;
            // Existing opening/endings pause time. Keep dialogue out of those overlays.
            if (Time.timeScale <= 0 || !view) { group.alpha = 0; return; }
            elapsed += Time.deltaTime;
            if (!speaker || elapsed >= duration)
            {
                speaker = null;
                while (pending.Count > 0)
                {
                    var next = pending.Dequeue();
                    if (!next.speaker || Time.time - next.queuedAt > 12) continue;
                    speaker = next.speaker;
                    speakerBody = speaker.GetComponent<Collider>();
                    snapPosition = true;
                    accent = speaker.GetComponent<CaretakerInteraction>() ? new Color(0.94f, 0.84f, 0.65f) : new Color(0.70f, 0.85f, 1f);
                    label.text = next.text;
                    panel.sizeDelta = new Vector2(364, Mathf.Max(label.GetPreferredValues(next.text, 320, 0).y + 32, 76));
                    duration = Mathf.Clamp(1.4f + next.text.Length * 0.045f, 3.2f, 4.5f);
                    elapsed = 0;
                    break;
                }
            }
            if (!speaker) { group.alpha = 0; return; }
            var position = view.WorldToViewportPoint(speaker.position);
            float distance = Vector3.Distance(view.transform.position, speaker.position);
            if (position.z <= 0 || position.x < 0 || position.x > 1 || position.y < 0 || position.y > 1)
            { group.alpha = 0; return; }
            Vector3 head = speaker.position + Vector3.up;
            if (speakerBody) head.y = speakerBody.bounds.max.y;
            var bounds = canvasRect.rect;
            Vector3 headViewport = view.WorldToViewportPoint(head);
            Vector2 target = bounds.min + Vector2.Scale(bounds.size, new Vector2(headViewport.x, headViewport.y));
            target.y += 8;
            float marginX = panel.sizeDelta.x * 0.5f + 51;
            float minX = bounds.xMin + marginX, maxX = bounds.xMax - marginX;
            float minY = bounds.yMin + 28, maxY = bounds.yMax - panel.sizeDelta.y - 28;
            target.x = Mathf.Clamp(target.x, minX, Mathf.Max(minX, maxX));
            target.y = Mathf.Clamp(target.y, minY, Mathf.Max(minY, maxY));
            var follow = snapPosition ? target : Vector2.Lerp(panel.anchoredPosition, target, 1 - Mathf.Exp(-18 * Time.deltaTime));
            follow.x = Mathf.Clamp(follow.x, minX, Mathf.Max(minX, maxX));
            follow.y = Mathf.Clamp(follow.y, minY, Mathf.Max(minY, maxY));
            panel.anchoredPosition = follow;
            snapPosition = false;
            for (int i = 0; i < motes.Length; i++)
            {
                float t = Time.time * 0.65f + i * 2.1f;
                motes[i].rectTransform.anchoredPosition = new Vector2(Mathf.Sin(t) * 4, Mathf.Cos(t * 0.8f) * 3);
                Color tint = accent; tint.a = 1.27f * (0.30f + 0.20f * (0.5f + 0.5f * Mathf.Sin(t)));
                motes[i].color = tint;
            }
            float fade = Mathf.SmoothStep(0, 1, elapsed / 0.25f) * Mathf.Clamp01((duration - elapsed) / 0.55f);
            group.alpha = fade * (1 - Mathf.InverseLerp(22, 28, distance));
        }
    }
}
