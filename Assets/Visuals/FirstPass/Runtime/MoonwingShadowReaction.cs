using UnityEngine;

namespace Moonwing.Visuals
{
    public sealed class MoonwingShadowReaction : MonoBehaviour
    {
        public ParticleSystem accents;
        public Renderer sigil;
        MaterialPropertyBlock block;
        float remaining;
        void Awake() { block = new MaterialPropertyBlock(); }
        public void React()
        {
            remaining = 0.24f;
            if (accents) accents.Emit(7);
        }
        void LateUpdate()
        {
            if (!sigil || remaining <= 0) return;
            remaining = Mathf.Max(0, remaining - Time.unscaledDeltaTime);
            block.SetFloat("_Emission", Mathf.Lerp(0.6f, 3f, remaining / 0.24f));
            sigil.SetPropertyBlock(block);
        }
    }
}
