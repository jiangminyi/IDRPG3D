using UnityEngine;
using UnityEngine.AI;

namespace IDRPG3D.GameplayPrototype
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class IDRPG3DNavMoveAgent : MonoBehaviour
    {
        [SerializeField] private float destinationUpdateInterval = 0.18f;
        [SerializeField] private float destinationDelta = 0.25f;
        [SerializeField] private float rotationSpeed = 720f;

        private NavMeshAgent agent;
        private IDRPG3DAnimatorBridge animatorBridge;
        private Vector3 lastDestination;
        private float nextDestinationUpdateTime;

        public NavMeshAgent Agent => agent;
        public float Radius => agent != null ? agent.radius : 0.35f;
        public float CurrentSpeed => agent != null ? agent.velocity.magnitude : 0f;
        public bool IsOnNavMesh => agent != null && agent.enabled && agent.isOnNavMesh;

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            animatorBridge?.SetMoveSpeed(CurrentSpeed);
        }

        public void Initialize()
        {
            agent = GetComponent<NavMeshAgent>();
            animatorBridge = GetComponent<IDRPG3DAnimatorBridge>();
            if (animatorBridge == null)
            {
                animatorBridge = gameObject.AddComponent<IDRPG3DAnimatorBridge>();
            }
            animatorBridge.Initialize();

            agent.updateRotation = true;
            agent.stoppingDistance = 0.08f;
            lastDestination = transform.position;
        }

        public void SetMoveStats(float speed, float acceleration, float angularSpeed)
        {
            if (agent == null)
            {
                Initialize();
            }

            agent.speed = Mathf.Max(0.1f, speed);
            agent.acceleration = Mathf.Max(0.1f, acceleration);
            agent.angularSpeed = Mathf.Max(1f, angularSpeed);
        }

        public bool MoveTo(Vector3 destination, float stoppingDistance)
        {
            if (!IsOnNavMesh)
            {
                return false;
            }

            if (Time.time < nextDestinationUpdateTime
                && (destination - lastDestination).sqrMagnitude < destinationDelta * destinationDelta)
            {
                return true;
            }

            if (!NavMesh.SamplePosition(destination, out var hit, 1.5f, agent.areaMask))
            {
                return false;
            }

            agent.isStopped = false;
            agent.stoppingDistance = Mathf.Max(0.01f, stoppingDistance);
            lastDestination = hit.position;
            nextDestinationUpdateTime = Time.time + destinationUpdateInterval;
            return agent.SetDestination(hit.position);
        }

        public void Stop()
        {
            if (!IsOnNavMesh)
            {
                return;
            }

            agent.ResetPath();
            agent.isStopped = true;
            animatorBridge?.SetMoveSpeed(0f);
        }

        public void FacePosition(Vector3 worldPosition)
        {
            var direction = worldPosition - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            var targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
