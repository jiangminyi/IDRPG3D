using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{
	public class GlobalEditor : EditorWindow
	{
		static UnitViewer unitViewerWindow;

		//[MenuItem("RTS Starter Kit/Global Settings Editor")]
		static void Init()
		{
			GlobalEditor window = (GlobalEditor)EditorWindow.GetWindow(typeof(GlobalEditor));
			window.titleContent = new GUIContent("RTS Global Settings Editor");
			window.maxSize = new Vector2(768f, 600f);
			window.minSize = window.maxSize;
			window.Show();

			unitViewerWindow = (UnitViewer)EditorWindow.GetWindow(typeof(UnitViewer));

			//CreateStyles();
			//LoadUnitDatas();
		}

		void OnGUI()
		{
			unitViewerWindow.OnGUI(); // so, it is possible, so we'll able to make global editor
		}
	}
}