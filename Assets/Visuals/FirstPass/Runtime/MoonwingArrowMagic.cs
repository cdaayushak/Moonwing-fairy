using UnityEngine;

namespace Moonwing.Visuals
{
    public sealed class MoonwingArrowMagic : MonoBehaviour
    {
        public MoonwingFadingWake wakePrefab;
        public Renderer silverShaft;
        public Transform energy;
        MoonwingFadingWake wake;
        Vector3 energyScale;
        float age;
        void Awake()
        {
            energyScale = energy ? energy.localScale : Vector3.one;
            if (energy) energy.localScale = energyScale * 0.35f;
            if (!wakePrefab) return;
            wake = Instantiate(wakePrefab, transform.position, transform.rotation);
            wake.name = "Moonlight wake (fading pixie dust)";
            if (wake.ribbon) wake.ribbon.Clear();
            if (wake.dust) { wake.dust.Play(); wake.dust.Emit(12); }
        }
        void LateUpdate()
        {
            age += Time.deltaTime;
            float ignition = Mathf.Clamp01(age / 0.16f);
            if (energy) energy.localScale = energyScale * Mathf.Lerp(0.35f, 1, ignition);
            if (silverShaft) silverShaft.enabled = true; // Enchantment surrounds a recognizable physical arrow.
            if (wake) wake.transform.SetPositionAndRotation(transform.position, transform.rotation);
        }
        void OnDestroy()
        {
            if (wake) wake.Release();
        }
    }
}
