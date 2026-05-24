using System;
using System.Collections;
using System.Diagnostics;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace IDRPG3D.LocalTest
{
    public sealed class IDRPG3DLocalTestBootstrap : MonoBehaviour
    {
        private const string ServerHost = "127.0.0.1";
        private const int GameServerPort = 20000;
        private const int MongoPort = 27017;
        private const string GameServerProtocol = "KCP/UDP";

        private InputField accountInput;
        private Text statusText;
        private Text endpointText;
        private Button startServerButton;
        private Button checkPortsButton;
        private Button loginButton;
        private Button enterWorldButton;
        private Button startIdleButton;
        private Button stopIdleButton;
        private Button createTeamButton;

        private string currentAccount = "local_player_001";
        private Process serverProcess;
        private Font defaultFont;

        private void Awake()
        {
            BuildSceneVisuals();
            BuildUI();
            AppendStatus("Local test scene ready.");
            StartCoroutine(CheckPortsRoutine());
        }

        private void OnDestroy()
        {
            if (serverProcess != null && !serverProcess.HasExited)
            {
                AppendStatus("Leaving server process running for debugging.");
            }
        }

        private void BuildSceneVisuals()
        {
            var cameraObject = GameObject.Find("Main Camera");
            if (cameraObject == null)
            {
                cameraObject = new GameObject("Main Camera");
                cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }

            cameraObject.transform.SetPositionAndRotation(new Vector3(0f, 4.8f, -8.5f), Quaternion.Euler(28f, 0f, 0f));
            cameraObject.GetComponent<Camera>().fieldOfView = 45f;
            cameraObject.GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;

            if (GameObject.Find("Directional Light") == null)
            {
                var lightObject = new GameObject("Directional Light");
                var light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.1f;
                lightObject.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            }

            if (GameObject.Find("Arena_Floor") == null)
            {
                var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
                floor.name = "Arena_Floor";
                floor.transform.localScale = new Vector3(5f, 1f, 5f);
            }

            if (GameObject.Find("Hero_DebugCapsule") == null)
            {
                var hero = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                hero.name = "Hero_DebugCapsule";
                hero.transform.position = new Vector3(-1.4f, 1f, 0f);
            }

            if (GameObject.Find("Monster_DebugCapsule") == null)
            {
                var monster = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                monster.name = "Monster_DebugCapsule";
                monster.transform.position = new Vector3(1.4f, 1f, 0f);
            }
        }

        private void BuildUI()
        {
            EnsureEventSystem();

            var canvas = CreateCanvas();
            ClearChildren(canvas.transform);

            var panel = CreatePanel(canvas.transform);

            CreateText(panel.transform, "IDRPG3D Local Flow", 22, TextAnchor.MiddleLeft, new Vector2(20f, -20f), new Vector2(520f, 34f));
            endpointText = CreateText(panel.transform, $"GameServer: {ServerHost}:{GameServerPort} {GameServerProtocol}    MongoDB: {ServerHost}:{MongoPort}", 14, TextAnchor.MiddleLeft, new Vector2(20f, -58f), new Vector2(560f, 26f));

            CreateText(panel.transform, "Account", 14, TextAnchor.MiddleLeft, new Vector2(20f, -98f), new Vector2(90f, 26f));
            accountInput = CreateInput(panel.transform, currentAccount, new Vector2(112f, -96f), new Vector2(238f, 30f));

            startServerButton = CreateButton(panel.transform, "Start Server", new Vector2(366f, -96f), new Vector2(120f, 30f), StartServer);
            checkPortsButton = CreateButton(panel.transform, "Check Ports", new Vector2(494f, -96f), new Vector2(120f, 30f), () => StartCoroutine(CheckPortsRoutine()));

            loginButton = CreateButton(panel.transform, "Login", new Vector2(20f, -142f), new Vector2(112f, 34f), () => PlaceholderNetworkAction("Login"));
            enterWorldButton = CreateButton(panel.transform, "Enter World", new Vector2(142f, -142f), new Vector2(112f, 34f), () => PlaceholderNetworkAction("Enter World"));
            startIdleButton = CreateButton(panel.transform, "Start Idle", new Vector2(264f, -142f), new Vector2(112f, 34f), () => PlaceholderNetworkAction("Start Idle"));
            stopIdleButton = CreateButton(panel.transform, "Stop Idle", new Vector2(386f, -142f), new Vector2(112f, 34f), () => PlaceholderNetworkAction("Stop Idle"));
            createTeamButton = CreateButton(panel.transform, "Create Team", new Vector2(508f, -142f), new Vector2(112f, 34f), () => PlaceholderNetworkAction("Create Team"));

            statusText = CreateText(panel.transform, string.Empty, 14, TextAnchor.UpperLeft, new Vector2(20f, -194f), new Vector2(590f, 210f));
        }

        private static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private Canvas CreateCanvas()
        {
            var canvasObject = GameObject.Find("LocalTestCanvas");
            if (canvasObject == null)
            {
                canvasObject = new GameObject("LocalTestCanvas");
            }

            var canvas = canvasObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = canvasObject.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvasObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            if (canvasObject.GetComponent<GraphicRaycaster>() == null)
            {
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            return canvas;
        }

        private static void ClearChildren(Transform parent)
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }

        private GameObject CreatePanel(Transform parent)
        {
            var panel = new GameObject("LocalTestPanel");
            panel.transform.SetParent(parent, false);

            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(28f, -28f);
            rect.sizeDelta = new Vector2(640f, 430f);

            var image = panel.AddComponent<Image>();
            image.color = new Color(0.07f, 0.08f, 0.10f, 0.92f);
            return panel;
        }

        private Text CreateText(Transform parent, string text, int fontSize, TextAnchor alignment, Vector2 position, Vector2 size)
        {
            var textObject = new GameObject(text.Replace(" ", "_"));
            textObject.transform.SetParent(parent, false);

            var rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var label = textObject.AddComponent<Text>();
            label.text = text;
            label.font = GetDefaultFont();
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = Color.white;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            return label;
        }

        private Font GetDefaultFont()
        {
            if (defaultFont != null)
            {
                return defaultFont;
            }

            defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null)
            {
                defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return defaultFont;
        }

        private InputField CreateInput(Transform parent, string text, Vector2 position, Vector2 size)
        {
            var inputObject = new GameObject("AccountInput");
            inputObject.transform.SetParent(parent, false);

            var rect = inputObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var image = inputObject.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.92f);

            var input = inputObject.AddComponent<InputField>();
            var textComponent = CreateText(inputObject.transform, text, 14, TextAnchor.MiddleLeft, new Vector2(8f, -2f), new Vector2(size.x - 16f, size.y - 4f));
            textComponent.color = new Color(0.08f, 0.09f, 0.11f, 1f);
            input.textComponent = textComponent;
            input.text = text;
            input.onEndEdit.AddListener(value => currentAccount = string.IsNullOrWhiteSpace(value) ? currentAccount : value.Trim());
            return input;
        }

        private Button CreateButton(Transform parent, string label, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            var buttonObject = new GameObject(label.Replace(" ", "_") + "Button");
            buttonObject.transform.SetParent(parent, false);

            var rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.18f, 0.33f, 0.58f, 1f);

            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);

            var text = CreateText(buttonObject.transform, label, 14, TextAnchor.MiddleCenter, Vector2.zero, size);
            text.color = Color.white;
            return button;
        }

        private void StartServer()
        {
            AppendStatus("Start server requested. Use PowerShell command from README for now.");
            AppendStatus("dotnet run --project GameServer/APP/Main/IDRPG3D.GameServer.Main.csproj -- -m Develop -g 1");
        }

        private void PlaceholderNetworkAction(string actionName)
        {
            currentAccount = string.IsNullOrWhiteSpace(accountInput.text) ? currentAccount : accountInput.text.Trim();
            AppendStatus($"{actionName}: UI is ready. Fantasy.Unity RPC binding is the next step. Account={currentAccount}");
        }

        private IEnumerator CheckPortsRoutine()
        {
            SetButtonsInteractable(false);
            yield return null;

            var mongoOpen = IsTcpOpen(ServerHost, MongoPort, 250);

            endpointText.text = $"GameServer: {ServerHost}:{GameServerPort} {GameServerProtocol}    MongoDB: {ServerHost}:{MongoPort} {(mongoOpen ? "OPEN" : "CLOSED")}";
            AppendStatus($"Local check -> GameServer:{GameServerProtocol} verify by Fantasy connect, MongoDB:{(mongoOpen ? "open" : "closed")}");
            SetButtonsInteractable(true);
        }

        private static bool IsTcpOpen(string host, int port, int timeoutMs)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    var connected = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(timeoutMs));
                    if (!connected)
                    {
                        return false;
                    }

                    client.EndConnect(result);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (startServerButton != null) startServerButton.interactable = interactable;
            if (checkPortsButton != null) checkPortsButton.interactable = interactable;
            if (loginButton != null) loginButton.interactable = interactable;
            if (enterWorldButton != null) enterWorldButton.interactable = interactable;
            if (startIdleButton != null) startIdleButton.interactable = interactable;
            if (stopIdleButton != null) stopIdleButton.interactable = interactable;
            if (createTeamButton != null) createTeamButton.interactable = interactable;
        }

        private void AppendStatus(string line)
        {
            Debug.Log($"[IDRPG3D LocalTest] {line}");
            if (statusText == null)
            {
                return;
            }

            var prefix = DateTime.Now.ToString("HH:mm:ss");
            statusText.text = $"{prefix}  {line}\n{statusText.text}";
        }
    }
}
