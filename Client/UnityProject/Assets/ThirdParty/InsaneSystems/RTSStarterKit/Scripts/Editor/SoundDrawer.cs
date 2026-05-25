using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InsaneSystems.RTSStarterKit
{
	[CustomPropertyDrawer(typeof(SoundAttribute))]
	public class SoundDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var storage = GameSettingsEditor.GetStorage();

			if (!storage)
			{
				EditorGUI.PropertyField(position, property, label, true);
				return;
			}

			var soundLibrary = storage.soundLibrary;

			if (!soundLibrary)
			{
				EditorGUI.PropertyField(position, property, label, true);
				return;
			}

			var soundPathes = soundLibrary.GetSoundsPathes(soundLibrary.soundsCategories);
			var currentIndex = GetIndexOfCurrentSound(property.objectReferenceValue as AudioClip, soundPathes);
			
			if (currentIndex < 0)
				currentIndex = 0;

			// here we drawing an popup with list of all sounds, divided by categories. We need to little reduce width to add second field near this popup
			var popupPosition = position;
			popupPosition.width -= 96;

			currentIndex = EditorGUI.Popup(popupPosition, property.displayName, currentIndex, soundPathes.ToArray());
			property.objectReferenceValue = GetSoundByIndex(currentIndex, soundPathes, soundLibrary);

			// we also showing default object field property in right side of our popup
			var propertyPosition = position;
			propertyPosition.x = popupPosition.width + 15;
			propertyPosition.width = 96;

			EditorGUI.PropertyField(propertyPosition, property, new GUIContent(""), true);
		}

		/// <summary>
		/// This method returns index in popup (list of sound pathes) of currently selected AudioClip.
		/// It always will return right index because AudioClip doesn't changes in field itself, only by user or from popup.
		/// It means that if audioclip category changed in Sound Library, it automatically select right item in popup if sound already selected.
		/// </summary>
		int GetIndexOfCurrentSound(AudioClip audioClip, List<string> soundPathes)
		{
			if (!audioClip)
				return -1;

			for (int i = 0; i < soundPathes.Count; i++)
			{
				string[] pathSplitted = soundPathes[i].Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);

				if (pathSplitted[pathSplitted.Length - 1] == audioClip.name)
					return i;
			}

			return -1;
		}

		///<summary> Retrives AudioClip of sound by its index in sound pathes list.</summary>
		AudioClip GetSoundByIndex(int index, List<string> soundPathes, SoundLibrary soundLibrary)
		{
			return soundLibrary.GetSoundByPath(soundPathes[index]);
		}
	}
}