using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{
	public class SoundEditor : EditorWindow
	{
		static int storageDatasFoundCount;
		static Storage loadedStorage;
		static SoundLibrary loadedSoundLibrary;

		Vector2 dataEditorScrollPos;

		[MenuItem("RTS Starter Kit/Sound Editor", priority = 0)]
		static void Init()
		{
			SoundEditor window = (SoundEditor)EditorWindow.GetWindow(typeof(SoundEditor));
			window.titleContent = new GUIContent("RTS Sound Editor");
			window.maxSize = new Vector2(768f, 512f);
			window.minSize = window.maxSize;
			window.Show();

			Load();
		}

		private void OnGUI()
		{
			GUILayout.BeginVertical(InsaneEditorStyles.paddedBoxStyle);
			GUILayout.Label("Sound Editor", InsaneEditorStyles.headerTextStyle);

			if (storageDatasFoundCount > 0)
			{
				if (storageDatasFoundCount > 1)
					EditorGUILayout.HelpBox("Found several Storages, it can cause showing of wrong settings in this window. It is recommended to have only one game Storage.", MessageType.Warning);
			}
			else
			{
				EditorGUILayout.HelpBox("No Storage found. You should create at least one, otherwise game will not work.", MessageType.Warning);
			}

			if (!loadedSoundLibrary)
				EditorGUILayout.HelpBox("No Sound Library setted up in Storage parameters. You should create at least one and add it to the Storage settings.", MessageType.Warning);

			EditorGUILayout.HelpBox("This is preview version of Sound Editor. Our team is working on improving visual representation of Sound Library parameters. In next updates we hope to make it look better.", MessageType.Info);

			if (loadedSoundLibrary)
			{
				dataEditorScrollPos = EditorGUILayout.BeginScrollView(dataEditorScrollPos);
				var soundEditor = Editor.CreateEditor(loadedSoundLibrary);
				soundEditor.OnInspectorGUI();
				EditorGUILayout.EndScrollView();
			}

			GUILayout.EndVertical();
		}

		static void Load()
		{
			var storages = new List<Storage>();

			EditorExtensions.LoadAssetsToList(storages, "t:storage");

			if (storages.Count > 0)
			{
				loadedStorage = storages[0];
		
				loadedSoundLibrary = loadedStorage.soundLibrary;
			}

			storageDatasFoundCount = storages.Count;
		}
	}
}