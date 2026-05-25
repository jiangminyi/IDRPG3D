using Animancer;
using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    public sealed class IDRPG3DAnimatorBridge : MonoBehaviour
    {
        [SerializeField] private AnimationClip idleClip;
        [SerializeField] private AnimationClip walkClip;
        [SerializeField] private AnimationClip runClip;
        [SerializeField] private AnimationClip attackClip;
        [SerializeField] private AnimationClip deathClip;
        [SerializeField] private float walkSpeed = 1.2f;
        [SerializeField] private float runSpeed = 3.5f;
        [SerializeField] private float fadeDuration = 0.12f;

        private AnimancerComponent animancer;
        private Animator animator;
        private AnimationClip currentClip;
        private float lastMoveSpeed;
        private bool isDead;
        private bool isActionLocked;
        private float actionUnlockTime;

        public void ConfigureClips(
            AnimationClip idle,
            AnimationClip walk,
            AnimationClip run,
            AnimationClip attack,
            AnimationClip death)
        {
            idleClip = idle != null ? idle : idleClip;
            walkClip = walk != null ? walk : walkClip;
            runClip = run != null ? run : runClip;
            attackClip = attack != null ? attack : attackClip;
            deathClip = death != null ? death : deathClip;
            PlayLocomotionForSpeed(lastMoveSpeed, force: true);
        }

        public void Initialize()
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                return;
            }

            animancer = animator.GetComponent<AnimancerComponent>();
            if (animancer == null)
            {
                animancer = animator.gameObject.AddComponent<AnimancerComponent>();
            }

            if (animator.GetComponent<IDRPG3DAnimationEventReceiver>() == null)
            {
                animator.gameObject.AddComponent<IDRPG3DAnimationEventReceiver>();
            }

            animator.applyRootMotion = false;
            animator.updateMode = AnimatorUpdateMode.Normal;
            animancer.Animator = animator;
            if (animator.runtimeAnimatorController != null)
            {
                animator.runtimeAnimatorController = null;
            }

            PlayLocomotionForSpeed(lastMoveSpeed, force: currentClip == null);
        }

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            if (isActionLocked && Time.time >= actionUnlockTime)
            {
                isActionLocked = false;
                PlayLocomotionForSpeed(lastMoveSpeed, force: true);
            }
        }

        public void SetMoveSpeed(float speed)
        {
            lastMoveSpeed = speed;
            if (isDead || isActionLocked)
            {
                return;
            }

            PlayLocomotionForSpeed(speed, force: false);
        }

        public void PlayMeleeAttack()
        {
            if (isDead || attackClip == null)
            {
                return;
            }

            PlayClip(attackClip, restart: true);
            var duration = attackClip.length > 0f ? attackClip.length : 0.6f;
            isActionLocked = true;
            actionUnlockTime = Time.time + Mathf.Min(duration, 1.1f);
        }

        public void SetDead(bool dead)
        {
            isDead = dead;
            isActionLocked = false;
            if (dead)
            {
                lastMoveSpeed = 0f;
                PlayClip(deathClip != null ? deathClip : idleClip, restart: true);
            }
            else
            {
                PlayLocomotionForSpeed(lastMoveSpeed, force: true);
            }
        }

        private void PlayLocomotionForSpeed(float speed, bool force)
        {
            var target = idleClip;
            if (speed >= Mathf.Max(walkSpeed, runSpeed * 0.55f) && runClip != null)
            {
                target = runClip;
            }
            else if (speed > 0.05f && walkClip != null)
            {
                target = walkClip;
            }

            PlayClip(target, restart: force);
        }

        private void PlayClip(AnimationClip clip, bool restart)
        {
            if (clip == null)
            {
                return;
            }

            if (animancer == null)
            {
                Initialize();
                if (animancer == null)
                {
                    return;
                }
            }

            if (!restart && currentClip == clip)
            {
                return;
            }

            currentClip = clip;
            var state = animancer.Play(clip, fadeDuration);
            if (restart)
            {
                state.Time = 0f;
            }
        }
    }
}
