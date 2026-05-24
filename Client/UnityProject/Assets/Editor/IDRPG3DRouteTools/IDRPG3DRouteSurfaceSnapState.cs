using System.Collections.Generic;
using Dreamteck.Splines;
using UnityEngine;

namespace IDRPG3D.EditorTools
{
    public sealed class IDRPG3DRouteSurfaceSnapState
    {
        private readonly Dictionary<int, Vector3[]> _lastPositionsBySpline = new Dictionary<int, Vector3[]>();

        public int SnapChangedPoints(SplineComputer spline, float heightOffset)
        {
            if (spline == null)
            {
                return 0;
            }

            var key = spline.GetInstanceID();
            var currentPositions = CapturePositions(spline);
            if (!_lastPositionsBySpline.TryGetValue(key, out var lastPositions))
            {
                Remember(spline);
                return 0;
            }

            if (lastPositions.Length != currentPositions.Length)
            {
                var snappedCount = IDRPG3DRouteSurfaceSnapUtility.SnapAllPointsToSurface(spline, heightOffset);
                Remember(spline);
                return snappedCount;
            }

            var changedCount = 0;
            for (var i = 0; i < currentPositions.Length; i++)
            {
                if (!HasPositionChanged(lastPositions[i], currentPositions[i]))
                {
                    continue;
                }

                if (IDRPG3DRouteSurfaceSnapUtility.SnapPointToSurface(spline, i, heightOffset))
                {
                    changedCount++;
                }
            }

            Remember(spline);
            return changedCount;
        }

        public void Remember(SplineComputer spline)
        {
            if (spline == null)
            {
                return;
            }

            _lastPositionsBySpline[spline.GetInstanceID()] = CapturePositions(spline);
        }

        public void Forget(SplineComputer spline)
        {
            if (spline == null)
            {
                return;
            }

            _lastPositionsBySpline.Remove(spline.GetInstanceID());
        }

        private static Vector3[] CapturePositions(SplineComputer spline)
        {
            var positions = new Vector3[spline.pointCount];
            for (var i = 0; i < positions.Length; i++)
            {
                positions[i] = spline.GetPointPosition(i);
            }

            return positions;
        }

        private static bool HasPositionChanged(Vector3 before, Vector3 after)
        {
            return !Mathf.Approximately(before.x, after.x)
                   || !Mathf.Approximately(before.y, after.y)
                   || !Mathf.Approximately(before.z, after.z);
        }
    }
}
