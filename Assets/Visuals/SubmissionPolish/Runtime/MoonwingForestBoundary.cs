using UnityEngine;

namespace Moonwing.Visuals
{
    // Transform-driven movement needs a positional boundary: physics walls would be bypassed.
    public static class MoonwingForestBoundary
    {
        public const float Radius = 38f;
        public static Vector3 Clamp(Vector3 position, float inset = 0)
        {
            Vector2 horizontal = Vector2.ClampMagnitude(new Vector2(position.x, position.z), Radius - inset);
            return new Vector3(horizontal.x, position.y, horizontal.y);
        }
        public static bool Contains(Vector3 position, float inset = 0) =>
            new Vector2(position.x, position.z).sqrMagnitude <= (Radius - inset) * (Radius - inset);
    }
}
