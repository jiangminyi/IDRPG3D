using Dreamteck.Splines;
using UnityEngine;

namespace IDRPG3D.LocalTest
{
    public sealed class IDRPG3DRouteFollowerDriver : MonoBehaviour
    {
        private static readonly int HorizontalSpeedHash = Animator.StringToHash("HorizontalSpeed");
        private static readonly int VerticalSpeedHash = Animator.StringToHash("VerticalSpeed");
        private static readonly int MoveDirectionXHash = Animator.StringToHash("MoveDirectionX");
        private static readonly int MoveDirectionYHash = Animator.StringToHash("MoveDirectionY");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        private static readonly int MoveSpeedModifierHash = Animator.StringToHash("MoveSpeedModifier");

        [SerializeField] private SplineComputer route;
        [SerializeField] private Transform actor;
        [SerializeField] private float moveSpeed = 2.4f;
        [SerializeField] private float animationDampTime = 0.1f;
        [SerializeField] private bool loop = true;
        [SerializeField] private bool startOnAwake = true;

        private SplineFollower follower;
        private Animator animator;

        public void Configure(SplineComputer targetRoute, Transform targetActor, float speed)
        {
            route = targetRoute;
            actor = targetActor;
            moveSpeed = speed;
            SetupFollower();
        }

        private void Awake()
        {
            if (startOnAwake)
            {
                SetupFollower();
            }
        }

        private void Update()
        {
            UpdateAnimator();
        }

        private void SetupFollower()
        {
            if (route == null || actor == null)
            {
                return;
            }

            follower = actor.GetComponent<SplineFollower>();
            if (follower == null)
            {
                follower = actor.gameObject.AddComponent<SplineFollower>();
            }

            follower.spline = route;
            follower.followMode = SplineFollower.FollowMode.Uniform;
            follower.wrapMode = loop ? SplineFollower.Wrap.Loop : SplineFollower.Wrap.Default;
            follower.followSpeed = moveSpeed;
            follower.autoStartPosition = false;
            follower.applyDirectionRotation = true;
            follower.follow = true;
            follower.RebuildImmediate();
            follower.SetPercent(0.0);

            animator = actor.GetComponentInChildren<Animator>();
            UpdateAnimator();

            Debug.Log($"[IDRPG3D Route] {actor.name} is following {route.name} at {moveSpeed:0.##} m/s.");
        }

        private void UpdateAnimator()
        {
            if (animator == null)
            {
                return;
            }

            var normalizedSpeed = follower != null && follower.follow ? 1f : 0f;
            animator.SetBool(IsGroundedHash, true);
            animator.SetFloat(MoveSpeedModifierHash, 1f);
            animator.SetFloat(HorizontalSpeedHash, 0f, animationDampTime, Time.deltaTime);
            animator.SetFloat(VerticalSpeedHash, normalizedSpeed, animationDampTime, Time.deltaTime);
            animator.SetFloat(MoveDirectionXHash, 0f, animationDampTime, Time.deltaTime);
            animator.SetFloat(MoveDirectionYHash, normalizedSpeed, animationDampTime, Time.deltaTime);
        }
    }
}
