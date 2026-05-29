using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    public sealed class IDRPG3DAnimationEventReceiver : MonoBehaviour
    {
        [SerializeField] private bool logFootsteps = false;

        public void FootL()
        {
            if (logFootsteps)
            {
                IDRPG3DPrototypeDebugLog.AnimationEvent($"[IDRPG3D AnimationEvent] {name} FootL.");
            }
        }

        public void FootR()
        {
            if (logFootsteps)
            {
                IDRPG3DPrototypeDebugLog.AnimationEvent($"[IDRPG3D AnimationEvent] {name} FootR.");
            }
        }

        public void Footstep()
        {
            if (logFootsteps)
            {
                IDRPG3DPrototypeDebugLog.AnimationEvent($"[IDRPG3D AnimationEvent] {name} Footstep.");
            }
        }

        public void Footstep(int foot)
        {
            if (logFootsteps)
            {
                IDRPG3DPrototypeDebugLog.AnimationEvent($"[IDRPG3D AnimationEvent] {name} Footstep {foot}.");
            }
        }
    }
}
