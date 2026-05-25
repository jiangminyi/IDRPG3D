using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{
	public class GameSettingsEditor : EditorWindow
	{
		static int storageDatasFoundCount;
		static Storage loadedStorage;

		Vector2 dataEditorScrollPos;

		[MenuItem("RTS Starter Kit/Game Settings", priority = 0)]
		static void Init()
		{
			GameSettingsEditor window = (GameSettingsEditor)EditorWindow.GetWindow(typeof(GameSettingsEditor));
			window.titleContent = new GUIContent("RTS Game Settings");
			window.maxSize = new Vector2(768f, 600f);
			window.minSize = window.maxSize;
			window.Show();

			Load();
		}
		
		[MenuItem("RTS Starter Kit/Online Guide", priority = 10)]
		static void ShowGuide()
		{
			Process.Start("https://docs.google.com/document/d/1jM-qJoNewQ2HnpzaUbwVG6VSX94sXPAGT5YRK15mUDA/edit?usp=sharing");
		}

		void OnGUI()
		{
			GUILayout.BeginVertical(InsaneEditorStyles.paddedBoxStyle);
			GUILayout.Label("Game Settings editor", InsaneEditorStyles.headerTextStyle);

			if (storageDatasFoundCount > 0)
			{
				if (storageDatasFoundCount > 1)
					EditorGUILayout.HelpBox("Found several Storages, it can cause showing of wrong settings in this window. It is recommended to have only one game Storage.", MessageType.Warning);

				dataEditorScrollPos = EditorGUILayout.BeginScrollView(dataEditorScrollPos);
				var storageEditor = Editor.CreateEditor(loadedStorage);
				storageEditor.OnInspectorGUI();
				EditorGUILayout.EndScrollView();
			}
			else
			{
				EditorGUILayout.HelpBox("No Storage found. You should create at least one, otherwise game will not work.", MessageType.Warning);
			}

			GUILayout.EndVertical();
		}

		static void Load()
		{
			var storages = new List<Storage>();

			EditorExtensions.LoadAssetsToList(storages, "t:storage");

			if (storages.Count > 0)
				loadedStorage = storages[0];

			storageDatasFoundCount = storages.Count;
		}

		/// <summary>Loads Storage from game resources. This method is can be used only in the Unity Editor, not in the build.</summary>
		public static Storage GetStorage()
		{
			if (!loadedStorage)
				Load();

			return loadedStorage;
		}
	}
}