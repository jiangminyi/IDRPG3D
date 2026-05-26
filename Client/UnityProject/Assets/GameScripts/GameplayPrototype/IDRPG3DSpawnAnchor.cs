using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    public sealed class IDRPG3DSpawnAnchor : MonoBehaviour
    {
        [SerializeField] private string anchorId;

        public string AnchorId => anchorId;
        public Vector3 Position => transform.position;
        public Vector3 Forward => transform.forward;

        public void Configure(string id)
        {
            anchorId = id ?? string.Empty;
        }
    }
}
