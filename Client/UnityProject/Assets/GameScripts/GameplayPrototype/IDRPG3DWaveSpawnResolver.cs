using System;
using System.Collections.Generic;
using UnityEngine;

namespace IDRPG3D.GameplayPrototype
{
    public static class IDRPG3DWaveSpawnResolver
    {
        public static IDRPG3DWaveSpawnPoint Resolve(
            IDRPG3DWaveDefinition wave,
            Vector3 routeAnchorPosition,
            Vector3 routeForward,
            IReadOnlyList<IDRPG3DSpawnAnchor> anchors)
        {
            if (wave.SpawnMode == IDRPG3DWaveSpawnMode.FixedAnchor)
            {
                return ResolveFixedAnchor(wave.SpawnAnchorId, anchors);
            }

            var forward = NormalizeForward(routeForward);
            return new IDRPG3DWaveSpawnPoint(
                true,
                routeAnchorPosition + forward * wave.SpawnDistanceAhead,
                forward);
        }

        private static IDRPG3DWaveSpawnPoint ResolveFixedAnchor(
            string anchorId,
            IReadOnlyList<IDRPG3DSpawnAnchor> anchors)
        {
            if (string.IsNullOrWhiteSpace(anchorId) || anchors == null)
            {
                return IDRPG3DWaveSpawnPoint.NotFound;
            }

            for (var i = 0; i < anchors.Count; i++)
            {
                var anchor = anchors[i];
                if (anchor == null)
                {
                    continue;
                }

                if (string.Equals(anchor.AnchorId, anchorId, StringComparison.OrdinalIgnoreCase))
                {
                    return new IDRPG3DWaveSpawnPoint(true, anchor.Position, NormalizeForward(anchor.Forward));
                }
            }

            return IDRPG3DWaveSpawnPoint.NotFound;
        }

        private static Vector3 NormalizeForward(Vector3 forward)
        {
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
            {
                return Vector3.forward;
            }

            return forward.normalized;
        }
    }

    public readonly struct IDRPG3DWaveSpawnPoint
    {
        public static readonly IDRPG3DWaveSpawnPoint NotFound = new IDRPG3DWaveSpawnPoint(false, Vector3.zero, Vector3.forward);

        public IDRPG3DWaveSpawnPoint(bool found, Vector3 position, Vector3 forward)
        {
            Found = found;
            Position = position;
            Forward = forward;
        }

        public bool Found { get; }
        public Vector3 Position { get; }
        public Vector3 Forward { get; }
    }
}
