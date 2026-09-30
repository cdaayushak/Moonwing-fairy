using UnityEngine;

namespace Moonwing.Visuals
{
    // Only attach to presentation children. Never drives a gameplay root.
    public sealed class MoonwingVisualMotion : MonoBehaviour
    {
        public Transform leftWing;
        public Transform rightWing;
        public float floatHeight = 0.025f;
        public float floatSpeed = 1.4f;
        public float wingAngle = 11f;
        Vector3 origin;
        Quaternion leftRest, rightRest;
        void Awake()
        {
            origin = transform.localPosition;
            if (leftWing) leftRest = leftWing.localRotation;
            if (rightWing) rightRest = rightWing.localRotation;
        }
        void LateUpdate()
        {
            float phase = Time.time * floatSpeed;
            transform.localPosition = origin + Vector3.up * (Mathf.Sin(phase) * floatHeight);
            float flutter = Mathf.Sin(phase * 2.3f) * wingAngle;
            if (leftWing) leftWing.localRotation = leftRest * Quaternion.Euler(0, flutter, 0);
            if (rightWing) rightWing.localRotation = rightRest * Quaternion.Euler(0, -flutter, 0);
        }
    }
}
