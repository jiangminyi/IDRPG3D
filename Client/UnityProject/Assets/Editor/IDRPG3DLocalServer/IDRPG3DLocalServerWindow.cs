using System.Linq;
using UnityEditor;
using UnityEngine;

namespace IDRPG3D.EditorTools
{
    public sealed class IDRPG3DLocalServerWindow : EditorWindow
    {
        private readonly IDRPG3DLocalServerLogBuffer logBuffer = new IDRPG3DLocalServerLogBuffer();
        private readonly IDRPG3DLocalServerLogScrollController logScroll = new IDRPG3DLocalServerLogScrollController();
        private readonly IDRPG3DLocalServerLogViewCache logViewCache = new IDRPG3DLocalServerLogViewCache();
        private IDRPG3DLocalServerProcess gameServer;
        private IDRPG3DLocalServerProcess mongoExpress;

        [MenuItem("IDRPG3D/Local Test/Server Console", priority = 10)]
        public static void Open()
        {
            var window = GetWindow<IDRPG3DLocalServerWindow>();
            window.titleContent = new GUIContent("IDRPG3D Server");
            window.minSize = new Vector2(720f, 420f);
            window.Show();
            window.Focus();
        }

        private void OnEnable()
        {
            gameServer ??= new IDRPG3DLocalServerProcess(
                "GameServer",
                logBuffer,
                new[] { 20000, 11001 },
                new[] { "IDRPG3D.GameServer.Main" });
            mongoExpress ??= new IDRPG3DLocalServerProcess("MongoExpress", logBuffer);
            EditorApplication.update += Repaint;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Repaint;
        }

        private void OnDestroy()
        {
            gameServer?.Dispose();
            mongoExpress?.Dispose();
        }

        private void OnGUI()
        {
            DrawHeader();
            EditorGUILayout.Space(8f);
            DrawServerControls();
            EditorGUILayout.Space(8f);
            DrawLogToolbar();
            DrawLogConsole();
        }

        private void DrawHeader()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Local Development Services", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Repository", IDRPG3DLocalProjectPaths.RepositoryRoot);
                EditorGUILayout.LabelField("MongoDB", "Windows service: mongodb://127.0.0.1:27017");
            }
        }

        private void DrawServerControls()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawServicePanel(
                    "GameServer",
                    gameServer.State,
                    gameServer.ProcessId,
                    () => gameServer.Start(IDRPG3DLocalServerProcess.CreateGameServerCommand(IDRPG3DLocalProjectPaths.RepositoryRoot)),
                    () => gameServer.Stop());

                DrawServicePanel(
                    "Mongo Express",
                    mongoExpress.State,
                    mongoExpress.ProcessId,
                    () => mongoExpress.Start(IDRPG3DLocalServerProcess.CreateMongoExpressCommand(IDRPG3DLocalProjectPaths.RepositoryRoot)),
                    () => mongoExpress.Stop());
            }
        }

        private static void DrawServicePanel(string title, IDRPG3DLocalProcessState state, int? processId, System.Action start, System.Action stop)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Width(350f)))
            {
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("State", processId.HasValue ? $"{state} (PID {processId.Value})" : state.ToString());

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUI.enabled = state != IDRPG3DLocalProcessState.Running && state != IDRPG3DLocalProcessState.Starting && state != IDRPG3DLocalProcessState.Ready;
                    if (GUILayout.Button("Start", GUILayout.Height(28f)))
                    {
                        start();
                    }

                    GUI.enabled = state == IDRPG3DLocalProcessState.Running || state == IDRPG3DLocalProcessState.Starting || state == IDRPG3DLocalProcessState.Ready;
                    if (GUILayout.Button("Stop", GUILayout.Height(28f)))
                    {
                        stop();
                    }

                    GUI.enabled = true;
                }
            }
        }

        private void DrawLogToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Console", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();

                var nextAutoScroll = GUILayout.Toggle(logScroll.AutoScroll, "Auto Scroll", EditorStyles.toolbarButton, GUILayout.Width(92f));
                if (nextAutoScroll != logScroll.AutoScroll)
                {
                    logScroll.SetAutoScroll(nextAutoScroll);
                }

                if (GUILayout.Button("Jump Latest", EditorStyles.toolbarButton, GUILayout.Width(88f)))
                {
                    logScroll.JumpToLatest();
                }

                if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(60f)))
                {
                    logBuffer.Clear();
                }
            }
        }

        private void DrawLogConsole()
        {
            logViewCache.UpdateForEvent(Event.current.type == EventType.Layout, logBuffer.Snapshot);

            if (logScroll.AutoScroll && Event.current.type == EventType.ScrollWheel)
            {
                logScroll.NotifyManualScroll();
            }

            logScroll.Position = EditorGUILayout.BeginScrollView(logScroll.Position, GUILayout.ExpandHeight(true));

            foreach (var line in logViewCache.Lines)
            {
                var style = GetLineStyle(line);
                EditorGUILayout.LabelField(line, style);
            }

            logScroll.MoveToLatestOnRepaint(Event.current.type == EventType.Repaint);

            EditorGUILayout.EndScrollView();
        }

        private static GUIStyle GetLineStyle(string line)
        {
            var style = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true
            };

            var lower = line.ToLowerInvariant();
            if (lower.Contains("error") || lower.Contains("failed") || lower.Contains("exception"))
            {
                style.normal.textColor = new Color(1f, 0.35f, 0.35f);
            }
            else if (lower.Contains("listening") || lower.Contains("connected") || lower.Contains("started"))
            {
                style.normal.textColor = new Color(0.45f, 0.85f, 0.45f);
            }

            return style;
        }
    }

    public sealed class IDRPG3DLocalServerLogViewCache
    {
        public string[] Lines { get; private set; } = new string[0];

        public void UpdateForEvent(bool isLayout, string[] latestLines)
        {
            if (isLayout)
            {
                Lines = latestLines ?? new string[0];
            }
        }
    }

    public sealed class IDRPG3DLocalServerLogScrollController
    {
        public Vector2 Position { get; set; }
        public bool AutoScroll { get; private set; } = true;

        public void SetAutoScroll(bool enabled)
        {
            AutoScroll = enabled;
            if (enabled)
            {
                JumpToLatest();
            }
        }

        public void NotifyManualScroll()
        {
            AutoScroll = false;
        }

        public void JumpToLatest()
        {
            AutoScroll = true;
            Position = new Vector2(Position.x, float.MaxValue);
        }

        public void MoveToLatestOnRepaint(bool isRepaint)
        {
            if (AutoScroll && isRepaint)
            {
                Position = new Vector2(Position.x, float.MaxValue);
            }
        }
    }
}
