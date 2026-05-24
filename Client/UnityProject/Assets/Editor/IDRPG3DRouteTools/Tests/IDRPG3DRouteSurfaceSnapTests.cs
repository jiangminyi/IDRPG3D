using Dreamteck.Splines;
using NUnit.Framework;
using UnityEngine;

namespace IDRPG3D.EditorTools.Tests
{
    public sealed class IDRPG3DRouteSurfaceSnapTests
    {
        private GameObject _splineObject;
        private Terrain _terrain;

        [TearDown]
        public void TearDown()
        {
            if (_splineObject != null)
            {
                Object.DestroyImmediate(_splineObject);
            }

            if (_terrain != null)
            {
                Object.DestroyImmediate(_terrain.gameObject);
            }
        }

        [Test]
        public void SnapPointToTerrainKeepsHorizontalPositionAndAppliesTerrainHeightOffset()
        {
            _terrain = CreateFlatTerrain(3f, new Vector3(10000f, 0f, 10000f));
            var spline = CreateSplineComputer(new Vector3(10012f, 20f, 10014f));

            var snapped = IDRPG3DRouteSurfaceSnapUtility.SnapPointToSurface(spline, 0, 0.25f);

            Assert.IsTrue(snapped);
            var point = spline.GetPointPosition(0);
            Assert.AreEqual(10012f, point.x, 0.001f);
            Assert.AreEqual(3.25f, point.y, 0.001f);
            Assert.AreEqual(10014f, point.z, 0.001f);
        }

        [Test]
        public void SnapPointToTerrainReturnsFalseWhenPointIndexIsInvalid()
        {
            _terrain = CreateFlatTerrain(0f, new Vector3(10000f, 0f, 10000f));
            var spline = CreateSplineComputer(new Vector3(10012f, 20f, 10014f));

            var snapped = IDRPG3DRouteSurfaceSnapUtility.SnapPointToSurface(spline, 99, 0f);

            Assert.IsFalse(snapped);
        }

        [Test]
        public void SnapChangedPointsSnapsMovedPointAutomatically()
        {
            _terrain = CreateFlatTerrain(4f, new Vector3(10000f, 0f, 10000f));
            var spline = CreateSplineComputer(new Vector3(10012f, 20f, 10014f));
            var state = new IDRPG3DRouteSurfaceSnapState();
            state.Remember(spline);

            spline.SetPointPosition(1, new Vector3(10018f, 30f, 10018f));

            var snappedCount = state.SnapChangedPoints(spline, 0.1f);

            Assert.AreEqual(1, snappedCount);
            var point = spline.GetPointPosition(1);
            Assert.AreEqual(10018f, point.x, 0.001f);
            Assert.AreEqual(4.1f, point.y, 0.001f);
            Assert.AreEqual(10018f, point.z, 0.001f);
        }

        [Test]
        public void SnapChangedPointsStartsTrackingWithoutMovingExistingRoute()
        {
            _terrain = CreateFlatTerrain(4f, new Vector3(10000f, 0f, 10000f));
            var spline = CreateSplineComputer(new Vector3(10012f, 20f, 10014f));
            var state = new IDRPG3DRouteSurfaceSnapState();

            var snappedCount = state.SnapChangedPoints(spline, 0.1f);

            Assert.AreEqual(0, snappedCount);
            Assert.AreEqual(20f, spline.GetPointPosition(0).y, 0.001f);
        }

        [Test]
        public void SplineComputerCloseAndBreakToggleLoopState()
        {
            var spline = CreateSplineComputer(new Vector3(10012f, 20f, 10014f));

            spline.Close();
            Assert.IsTrue(spline.isClosed);

            spline.Break();
            Assert.IsFalse(spline.isClosed);
        }

        [Test]
        public void CanCloseLoopRequiresAtLeastThreePoints()
        {
            _splineObject = new GameObject("Route_Test");
            var spline = _splineObject.AddComponent<SplineComputer>();

            Assert.IsFalse(IDRPG3DRouteSurfaceSnapUtility.CanCloseLoop(spline));

            spline.SetPoints(new[]
            {
                new SplinePoint(new Vector3(10012f, 20f, 10014f)),
                new SplinePoint(new Vector3(10012f, 20f, 10018f)),
                new SplinePoint(new Vector3(10012f, 20f, 10022f))
            });

            Assert.IsTrue(IDRPG3DRouteSurfaceSnapUtility.CanCloseLoop(spline));
        }

        private Terrain CreateFlatTerrain(float height, Vector3 position)
        {
            var terrainData = new TerrainData
            {
                heightmapResolution = 33,
                size = new Vector3(50f, 20f, 50f)
            };

            var normalizedHeight = height / terrainData.size.y;
            var heights = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
            for (var z = 0; z < terrainData.heightmapResolution; z++)
            {
                for (var x = 0; x < terrainData.heightmapResolution; x++)
                {
                    heights[z, x] = normalizedHeight;
                }
            }

            terrainData.SetHeights(0, 0, heights);

            var terrainObject = Terrain.CreateTerrainGameObject(terrainData);
            terrainObject.transform.position = position;
            return terrainObject.GetComponent<Terrain>();
        }

        private SplineComputer CreateSplineComputer(Vector3 pointPosition)
        {
            _splineObject = new GameObject("Route_Test");
            var spline = _splineObject.AddComponent<SplineComputer>();
            spline.type = Spline.Type.CatmullRom;
            spline.SetPoints(new[]
            {
                new SplinePoint(pointPosition),
                new SplinePoint(pointPosition + Vector3.forward * 5f),
                new SplinePoint(pointPosition + Vector3.forward * 10f)
            });
            return spline;
        }
    }
}
