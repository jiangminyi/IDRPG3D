using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    public static class IDRPG3DUnitVisualScale
    {
        public static void Apply(Transform target, float visualScale)
        {
            if (target == null)
            {
                return;
            }

            var safeScale = Mathf.Max(0.01f, visualScale);
            target.localScale = Vector3.one * safeScale;
        }
    }
}
