using UnityEngine;
using UnityEngine.UI;
namespace Moonwing.Visuals
{
    public sealed class MoonwingIntroMist : MonoBehaviour
    {
        public Graphic mist;
        public float clearDuration = 1.5f;
        Material instance;
        float cleared;
        bool clearing;
        public bool IsCleared => cleared >= 1;
        void Awake() { if (mist) { instance = new Material(mist.material); mist.material = instance; } }
        public void Clear() { clearing = true; }
        void Update()
        {
            if (!instance || !mist) return;
            if (clearing) cleared = Mathf.Min(1, cleared + Time.unscaledDeltaTime / clearDuration);
            instance.SetFloat("_Clock", Time.unscaledTime);
            instance.SetFloat("_Clear", Mathf.SmoothStep(0, 1, cleared));
            if (IsCleared) { mist.enabled = false; enabled = false; }
        }
        void OnDestroy() { if (instance) Destroy(instance); }
    }
}
