using Dreamteck.Splines;
using UnityEngine;

namespace IDRPG3D.EditorTools
{
    public static class IDRPG3DRouteSurfaceSnapUtility
    {
        private const float RaycastHeight = 1000f;
        private const float RaycastDistance = RaycastHeight * 2f;

        public static bool SnapPointToSurface(SplineComputer spline, int pointIndex, float heightOffset)
        {
            if (spline == null || pointIndex < 0 || pointIndex >= spline.pointCount)
            {
                return false;
            }

            var currentPosition = spline.GetPointPosition(pointIndex);
            if (!TryGetSurfacePosition(currentPosition, heightOffset, out var surfacePosition, out var surfaceNormal))
            {
                return false;
            }

            spline.SetPointPosition(pointIndex, surfacePosition);
            spline.SetPointNormal(pointIndex, surfaceNormal);
            return true;
        }

        public static int SnapAllPointsToSurface(SplineComputer spline, float heightOffset)
        {
            if (spline == null)
            {
                return 0;
            }

            var snappedCount = 0;
            for (var i = 0; i < spline.pointCount; i++)
            {
                if (SnapPointToSurface(spline, i, heightOffset))
                {
                    snappedCount++;
                }
            }

            return snappedCount;
        }

        public static bool CanCloseLoop(SplineComputer spline)
        {
            return spline != null && spline.pointCount >= 3;
        }

        private static bool TryGetSurfacePosition(Vector3 currentPosition, float heightOffset, out Vector3 surfacePosition, out Vector3 surfaceNormal)
        {
            if (TryGetTerrainSurface(currentPosition, heightOffset, out surfacePosition, out surfaceNormal))
            {
                return true;
            }

            return TryGetRaycastSurface(currentPosition, heightOffset, out surfacePosition, out surfaceNormal);
        }

        private static bool TryGetTerrainSurface(Vector3 currentPosition, float heightOffset, out Vector3 surfacePosition, out Vector3 surfaceNormal)
        {
            foreach (var terrain in Terrain.activeTerrains)
            {
                if (terrain == null || terrain.terrainData == null || !ContainsHorizontalPosition(terrain, currentPosition))
                {
                    continue;
                }

                var terrainPosition = terrain.transform.position;
                var terrainSize = terrain.terrainData.size;
                var normalizedX = Mathf.InverseLerp(terrainPosition.x, terrainPosition.x + terrainSize.x, currentPosition.x);
                var normalizedZ = Mathf.InverseLerp(terrainPosition.z, terrainPosition.z + terrainSize.z, currentPosition.z);
                var height = terrain.SampleHeight(currentPosition) + terrainPosition.y + heightOffset;

                surfacePosition = new Vector3(currentPosition.x, height, currentPosition.z);
                surfaceNormal = terrain.terrainData.GetInterpolatedNormal(normalizedX, normalizedZ);
                return true;
            }

            surfacePosition = currentPosition;
            surfaceNormal = Vector3.up;
            return false;
        }

        private static bool ContainsHorizontalPosition(Terrain terrain, Vector3 position)
        {
            var terrainPosition = terrain.transform.position;
            var terrainSize = terrain.terrainData.size;
            return position.x >= terrainPosition.x
                   && position.x <= terrainPosition.x + terrainSize.x
                   && position.z >= terrainPosition.z
                   && position.z <= terrainPosition.z + terrainSize.z;
        }

        private static bool TryGetRaycastSurface(Vector3 currentPosition, float heightOffset, out Vector3 surfacePosition, out Vector3 surfaceNormal)
        {
            var origin = new Vector3(currentPosition.x, currentPosition.y + RaycastHeight, currentPosition.z);
            if (Physics.Raycast(origin, Vector3.down, out var hit, RaycastDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                surfacePosition = hit.point + hit.normal * heightOffset;
                surfaceNormal = hit.normal;
                return true;
            }

            surfacePosition = currentPosition;
            surfaceNormal = Vector3.up;
            return false;
        }
    }
}
