using Dreamteck.Splines;
using UnityEditor;
using UnityEngine;

namespace IDRPG3D.EditorTools
{
    public sealed class IDRPG3DRouteChineseGuideWindow : EditorWindow
    {
        private Vector2 _scroll;
        private string _search = string.Empty;
        private int _sectionIndex;

        [MenuItem("IDRPG3D/Route/路线编辑器中文辅助", priority = 2)]
        public static void Open()
        {
            GetWindow<IDRPG3DRouteChineseGuideWindow>("路线编辑器中文辅助");
        }

        private void OnGUI()
        {
            DrawRouteSummary();
            EditorGUILayout.Space();
            DrawSectionFilter();
            EditorGUILayout.Space();
            DrawSearch();
            EditorGUILayout.Space();
            DrawGuide();
        }

        private void DrawRouteSummary()
        {
            EditorGUILayout.LabelField("当前路线", EditorStyles.boldLabel);
            var spline = GetSelectedSpline();
            if (spline == null)
            {
                EditorGUILayout.HelpBox("请选择一个带有 Dreamteck SplineComputer 的路线物体。原 Dreamteck Inspector 继续负责实际编辑，本窗口用于中文说明和常用路线操作。", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("对象", spline.name);
            EditorGUILayout.LabelField("曲线类型", $"{spline.type} - {TranslateSplineType(spline.type)}");
            EditorGUILayout.LabelField("控制点数量", spline.pointCount.ToString());
            EditorGUILayout.LabelField("是否闭合", spline.isClosed ? "是，首尾相接" : "否，开放路线");
            EditorGUILayout.LabelField("采样数量", spline.sampleCount.ToString());

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(!spline.isClosed && !IDRPG3DRouteSurfaceSnapUtility.CanCloseLoop(spline)))
                {
                    if (GUILayout.Button(spline.isClosed ? "断开循环" : "闭合循环"))
                    {
                        ToggleClosed(spline);
                    }
                }

                if (GUILayout.Button("打开自动贴地窗口"))
                {
                    IDRPG3DRouteSurfaceSnapWindow.Open();
                }
            }

            if (!spline.isClosed && !IDRPG3DRouteSurfaceSnapUtility.CanCloseLoop(spline))
            {
                EditorGUILayout.HelpBox("闭合循环至少需要 3 个控制点。", MessageType.Info);
            }
        }

        private void DrawSectionFilter()
        {
            var options = new string[IDRPG3DRouteChineseGuide.Sections.Count + 1];
            options[0] = "全部分类";
            for (var i = 0; i < IDRPG3DRouteChineseGuide.Sections.Count; i++)
            {
                options[i + 1] = IDRPG3DRouteChineseGuide.Sections[i].Title;
            }

            _sectionIndex = EditorGUILayout.Popup("分类", _sectionIndex, options);
        }

        private void DrawSearch()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("搜索", GUILayout.Width(36f));
                _search = EditorGUILayout.TextField(_search);
                if (GUILayout.Button("清空", GUILayout.Width(52f)))
                {
                    _search = string.Empty;
                }
            }
        }

        private void DrawGuide()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            for (var i = 0; i < IDRPG3DRouteChineseGuide.Sections.Count; i++)
            {
                var section = IDRPG3DRouteChineseGuide.Sections[i];
                if (_sectionIndex > 0 && _sectionIndex != i + 1)
                {
                    continue;
                }

                if (!SectionMatchesSearch(section))
                {
                    continue;
                }

                EditorGUILayout.LabelField(section.Title, EditorStyles.boldLabel);
                foreach (var entry in section.Entries)
                {
                    if (!EntryMatchesSearch(entry))
                    {
                        continue;
                    }

                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.LabelField($"{entry.English} -> {entry.Chinese}", EditorStyles.boldLabel);
                        EditorGUILayout.LabelField(entry.Description, EditorStyles.wordWrappedLabel);
                    }
                }

                EditorGUILayout.Space();
            }

            EditorGUILayout.EndScrollView();
        }

        private bool SectionMatchesSearch(IDRPG3DRouteChineseGuideSection section)
        {
            if (string.IsNullOrWhiteSpace(_search))
            {
                return true;
            }

            foreach (var entry in section.Entries)
            {
                if (EntryMatchesSearch(entry))
                {
                    return true;
                }
            }

            return section.Title.IndexOf(_search, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool EntryMatchesSearch(IDRPG3DRouteChineseGuideEntry entry)
        {
            if (string.IsNullOrWhiteSpace(_search))
            {
                return true;
            }

            return entry.English.IndexOf(_search, System.StringComparison.OrdinalIgnoreCase) >= 0
                   || entry.Chinese.IndexOf(_search, System.StringComparison.OrdinalIgnoreCase) >= 0
                   || entry.Description.IndexOf(_search, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static SplineComputer GetSelectedSpline()
        {
            return Selection.activeGameObject == null ? null : Selection.activeGameObject.GetComponent<SplineComputer>();
        }

        private static string TranslateSplineType(Spline.Type type)
        {
            switch (type)
            {
                case Spline.Type.CatmullRom:
                    return "经过控制点的平滑路线，推荐用于手工规划地形路线";
                case Spline.Type.Bezier:
                    return "贝塞尔曲线，适合精修弯道和演出路线";
                case Spline.Type.BSpline:
                    return "B 样条，平滑但不一定经过控制点";
                case Spline.Type.Linear:
                    return "直线折线，适合调试或硬边路线";
                default:
                    return "未知类型";
            }
        }

        private static void ToggleClosed(SplineComputer spline)
        {
            Undo.RecordObject(spline, spline.isClosed ? "断开路线循环" : "闭合路线循环");
            if (spline.isClosed)
            {
                spline.Break();
            }
            else
            {
                if (!IDRPG3DRouteSurfaceSnapUtility.CanCloseLoop(spline))
                {
                    Debug.LogWarning($"路线 {spline.name} 至少需要 3 个控制点才能闭合。");
                    return;
                }

                spline.Close();
            }

            EditorUtility.SetDirty(spline);
            SceneView.RepaintAll();
        }
    }
}
