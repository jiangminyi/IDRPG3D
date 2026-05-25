using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    public sealed class IDRPG3DTerrainVisualGrounder : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float rayStartHeight = 3f;
        [SerializeField] private float rayDistance = 8f;
        [SerializeField] private float footOffset;
        [SerializeField] private float maxVisualOffset = 2f;
        [SerializeField] private bool lockLocalXZ = true;

        private Vector3 initialVisualLocalPosition;
        private float currentOffset;
        private bool hasInitialVisualLocalPosition;

        public void Configure(Transform targetVisualRoot, LayerMask targetGroundMask, float targetFootOffset)
        {
            visualRoot = targetVisualRoot;
            groundMask = targetGroundMask;
            footOffset = targetFootOffset;
            CaptureInitialVisualPosition();
        }

        private void LateUpdate()
        {
            if (visualRoot == null)
            {
                return;
            }

            if (!hasInitialVisualLocalPosition)
            {
                CaptureInitialVisualPosition();
            }

            var origin = transform.position + Vector3.up * rayStartHeight;
            if (!Physics.Raycast(origin, Vector3.down, out var hit, rayDistance, groundMask, QueryTriggerInteraction.Ignore))
            {
                RestoreVisualLocalXZ();
                return;
            }

            var groundOffset = hit.point.y + footOffset - transform.position.y;
            var wantedOffset = initialVisualLocalPosition.y + Mathf.Clamp(groundOffset, -maxVisualOffset, maxVisualOffset);
            if (Mathf.Abs(wantedOffset - currentOffset) >= 0.001f)
            {
                currentOffset = wantedOffset;
            }

            var localPosition = visualRoot.localPosition;
            if (lockLocalXZ)
            {
                localPosition.x = initialVisualLocalPosition.x;
                localPosition.z = initialVisualLocalPosition.z;
            }
            localPosition.y = currentOffset;
            visualRoot.localPosition = localPosition;
        }

        private void CaptureInitialVisualPosition()
        {
            if (visualRoot == null)
            {
                hasInitialVisualLocalPosition = false;
                return;
            }

            initialVisualLocalPosition = visualRoot.localPosition;
            currentOffset = initialVisualLocalPosition.y;
            hasInitialVisualLocalPosition = true;
        }

        private void RestoreVisualLocalXZ()
        {
            if (!lockLocalXZ || visualRoot == null || !hasInitialVisualLocalPosition)
            {
                return;
            }

            var localPosition = visualRoot.localPosition;
            localPosition.x = initialVisualLocalPosition.x;
            localPosition.z = initialVisualLocalPosition.z;
            visualRoot.localPosition = localPosition;
        }
    }
}
