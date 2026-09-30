using UnityEngine;

namespace Moonwing.Visuals
{
    // A separate observer on the projectile root; ArrowDamage remains authoritative.
    // It uses the same EnemyHealth lookup as ArrowDamage so scenery never produces a hit cue.
    public sealed class MoonwingImpactFeedback : MonoBehaviour
    {
        public ParticleSystem impactPrefab;
        bool reacted;
        void OnCollisionEnter(Collision collision)
        {
            if (reacted) return;
            EnemyHealth enemy = collision.gameObject.GetComponent<EnemyHealth>();
            if (!enemy) return;
            reacted = true;
            Vector3 point = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
            if (impactPrefab)
            {
                ParticleSystem effect = Instantiate(impactPrefab, point, Quaternion.identity);
                effect.Play();
            }
            var reaction = enemy.GetComponentInChildren<MoonwingShadowReaction>();
            if (reaction) reaction.React();
        }
    }
}
