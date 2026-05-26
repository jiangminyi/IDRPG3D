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
        [SerializeField] private float minPlaybackSpeed = 0.35f;
        [SerializeField] private float maxPlaybackSpeed = 2.5f;

        private AnimancerComponent animancer;
        private Animator animator;
        private AnimationClip currentClip;
        private float lastMoveSpeed;
        private float currentPlaybackSpeed = 1f;
        private bool isDead;
        private bool isActionLocked;
        private float actionUnlockTime;

        public float CurrentPlaybackSpeed => currentPlaybackSpeed;

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
            SetMoveSpeed(speed, CalculateDefaultMovePlaybackSpeed(speed));
        }

        public void SetMoveSpeed(float speed, float playbackSpeed)
        {
            lastMoveSpeed = speed;
            if (isDead || isActionLocked)
            {
                return;
            }

            PlayLocomotionForSpeed(speed, force: false, playbackSpeed: playbackSpeed);
        }

        public void PlayMeleeAttack()
        {
            PlayMeleeAttack(1f);
        }

        public void PlayMeleeAttack(float playbackSpeed)
        {
            if (isDead || attackClip == null)
            {
                return;
            }

            var clampedPlaybackSpeed = ClampPlaybackSpeed(playbackSpeed);
            PlayClip(attackClip, restart: true, playbackSpeed: clampedPlaybackSpeed);
            var duration = attackClip.length > 0f ? attackClip.length : 0.6f;
            isActionLocked = true;
            actionUnlockTime = Time.time + Mathf.Min(duration / clampedPlaybackSpeed, 1.1f);
        }

        public void SetDead(bool dead)
        {
            isDead = dead;
            isActionLocked = false;
            if (dead)
            {
                lastMoveSpeed = 0f;
                PlayClip(deathClip != null ? deathClip : idleClip, restart: true, playbackSpeed: 1f);
            }
            else
            {
                PlayLocomotionForSpeed(lastMoveSpeed, force: true, playbackSpeed: CalculateDefaultMovePlaybackSpeed(lastMoveSpeed));
            }
        }

        private void PlayLocomotionForSpeed(float speed, bool force)
        {
            PlayLocomotionForSpeed(speed, force, CalculateDefaultMovePlaybackSpeed(speed));
        }

        private void PlayLocomotionForSpeed(float speed, bool force, float playbackSpeed)
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

            PlayClip(target, restart: force, playbackSpeed: target == idleClip ? 1f : playbackSpeed);
        }

        private void PlayClip(AnimationClip clip, bool restart)
        {
            PlayClip(clip, restart, 1f);
        }

        private void PlayClip(AnimationClip clip, bool restart, float playbackSpeed)
        {
            currentPlaybackSpeed = ClampPlaybackSpeed(playbackSpeed);
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
            state.Speed = currentPlaybackSpeed;
            if (restart)
            {
                state.Time = 0f;
            }
        }

        private float CalculateDefaultMovePlaybackSpeed(float speed)
        {
            if (speed <= 0.05f)
            {
                return 1f;
            }

            var referenceSpeed = speed >= Mathf.Max(walkSpeed, runSpeed * 0.55f) ? runSpeed : walkSpeed;
            return referenceSpeed > 0.01f ? speed / referenceSpeed : 1f;
        }

        private float ClampPlaybackSpeed(float playbackSpeed)
        {
            return Mathf.Clamp(playbackSpeed, minPlaybackSpeed, maxPlaybackSpeed);
        }
    }
}
