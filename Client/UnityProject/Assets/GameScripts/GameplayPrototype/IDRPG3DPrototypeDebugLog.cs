using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    public static class IDRPG3DPrototypeDebugLog
    {
        public static bool CombatEnabled { get; set; }
        public static bool AnimationEventEnabled { get; set; }

        public static void Combat(string message)
        {
            if (CombatEnabled)
            {
                Debug.Log(message);
            }
        }

        public static void AnimationEvent(string message)
        {
            if (AnimationEventEnabled)
            {
                Debug.Log(message);
            }
        }
    }
}
