using Dreamteck.Splines;
using UnityEditor;
using UnityEngine;

namespace IDRPG3D.EditorTools
{
    public sealed class IDRPG3DRouteSurfaceSnapWindow : EditorWindow
    {
        private const float DefaultHeightOffset = 0.05f;
        private const float PointButtonSize = 0.08f;
        private const float PointButtonPickSize = 0.12f;

        private bool _enabled = true;
        private float _heightOffset = DefaultHeightOffset;
        private readonly IDRPG3DRouteSurfaceSnapState _snapState = new IDRPG3DRouteSurfaceSnapState();

        [MenuItem("IDRPG3D/Route/Surface Snap Mode", priority = 1)]
        public static void Open()
        {
            GetWindow<IDRPG3DRouteSurfaceSnapWindow>("Route Surface Snap");
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Dreamteck Route Surface Snap", EditorStyles.boldLabel);
            _enabled = EditorGUILayout.Toggle("Enable", _enabled);
            _heightOffset = EditorGUILayout.FloatField("Height Offset", _heightOffset);

            var spline = GetSelectedSpline();
            using (new EditorGUI.DisabledScope(spline == null))
            {
                var closed = spline != null && spline.isClosed;
                using (new EditorGUI.DisabledScope(!closed && !IDRPG3DRouteSurfaceSnapUtility.CanCloseLoop(spline)))
                {
                    var nextClosed = EditorGUILayout.Toggle("Closed Loop", closed);
                    if (spline != null && nextClosed != closed)
                    {
                        SetClosedLoop(spline, nextClosed);
                    }
                }

                if (spline != null && !closed && !IDRPG3DRouteSurfaceSnapUtility.CanCloseLoop(spline))
                {
                    EditorGUILayout.HelpBox("Closed Loop needs at least 3 control points.", MessageType.Info);
                }
            }

            using (new EditorGUI.DisabledScope(spline == null))
            {
                if (GUILayout.Button("Snap All Points On Selected Route"))
                {
                    SnapAllPoints(spline);
                }
            }

            if (spline == null)
            {
                EditorGUILayout.HelpBox("Select a GameObject with a Dreamteck SplineComputer.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox($"Selected route: {spline.name}. When Enable is on, moved or newly added points are snapped automatically. Green point buttons can still snap one point manually.", MessageType.None);
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (!_enabled)
            {
                return;
            }

            var spline = GetSelectedSpline();
            if (spline == null)
            {
                return;
            }

            SnapChangedPoints(spline);
            Handles.color = Color.green;
            for (var i = 0; i < spline.pointCount; i++)
            {
                var pointPosition = spline.GetPointPosition(i);
                var handleSize = HandleUtility.GetHandleSize(pointPosition);
                if (Handles.Button(pointPosition, Quaternion.identity, handleSize * PointButtonSize, handleSize * PointButtonPickSize, Handles.SphereHandleCap))
                {
                    SnapSinglePoint(spline, i);
                }
            }
        }

        private static SplineComputer GetSelectedSpline()
        {
            var selected = Selection.activeGameObject;
            return selected == null ? null : selected.GetComponent<SplineComputer>();
        }

        private void SnapSinglePoint(SplineComputer spline, int pointIndex)
        {
            Undo.RecordObject(spline, "Snap Route Point To Surface");
            if (IDRPG3DRouteSurfaceSnapUtility.SnapPointToSurface(spline, pointIndex, _heightOffset))
            {
                EditorUtility.SetDirty(spline);
                _snapState.Remember(spline);
                SceneView.RepaintAll();
                return;
            }

            Debug.LogWarning($"No terrain or collider surface found under route point {pointIndex} on {spline.name}.");
        }

        private void SnapAllPoints(SplineComputer spline)
        {
            Undo.RecordObject(spline, "Snap Route Points To Surface");
            var snappedCount = IDRPG3DRouteSurfaceSnapUtility.SnapAllPointsToSurface(spline, _heightOffset);
            EditorUtility.SetDirty(spline);
            _snapState.Remember(spline);
            SceneView.RepaintAll();
            Debug.Log($"Snapped {snappedCount}/{spline.pointCount} route points to surface on {spline.name}.");
        }

        private void SnapChangedPoints(SplineComputer spline)
        {
            var snappedCount = _snapState.SnapChangedPoints(spline, _heightOffset);
            if (snappedCount <= 0)
            {
                return;
            }

            EditorUtility.SetDirty(spline);
            SceneView.RepaintAll();
        }

        private void SetClosedLoop(SplineComputer spline, bool closed)
        {
            Undo.RecordObject(spline, closed ? "Close Route Loop" : "Break Route Loop");
            if (closed)
            {
                if (!IDRPG3DRouteSurfaceSnapUtility.CanCloseLoop(spline))
                {
                    Debug.LogWarning($"Route {spline.name} needs at least 3 points before it can be closed.");
                    return;
                }

                spline.Close();
            }
            else
            {
                spline.Break();
            }

            EditorUtility.SetDirty(spline);
            _snapState.Forget(spline);
            SceneView.RepaintAll();
        }
    }
}
