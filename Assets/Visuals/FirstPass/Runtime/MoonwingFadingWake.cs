using UnityEngine;

namespace Moonwing.Visuals
{
    // Detached world-space wake: stopping emission leaves existing dust alive.
    public sealed class MoonwingFadingWake : MonoBehaviour
    {
        public ParticleSystem dust;
        public TrailRenderer ribbon;
        bool fading;
        float elapsed;
        public void Release()
        {
            if (fading) return;
            fading = true;
            if (dust) dust.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            if (ribbon) ribbon.emitting = false;
        }
        void Update()
        {
            if (!fading) return;
            elapsed += Time.unscaledDeltaTime;
            // Continue cleanup even when victory/capture freezes gameplay time.
            if (ribbon) ribbon.widthMultiplier = Mathf.Max(0, 1 - elapsed / 0.6f);
            if (elapsed >= 1.6f) Destroy(gameObject);
        }
    }
}
