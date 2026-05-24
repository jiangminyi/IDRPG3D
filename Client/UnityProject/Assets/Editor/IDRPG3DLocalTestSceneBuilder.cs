using System.IO;
using IDRPG3D.LocalTest;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IDRPG3D.EditorTools
{
    public static class IDRPG3DLocalTestSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/IDRPG3D_LocalTest.unity";

        [MenuItem("IDRPG3D/Local Test/Rebuild Scene", priority = 1)]
        public static void RebuildScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "IDRPG3D_LocalTest";

            var bootstrapObject = new GameObject("IDRPG3D_LocalTest_Bootstrap");
            bootstrapObject.AddComponent<IDRPG3DLocalTestBootstrap>();

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(bootstrapObject);

            Debug.Log($"IDRPG3D local test scene rebuilt: {ScenePath}");
        }

        [MenuItem("IDRPG3D/Local Test/Open Scene", priority = 2)]
        public static void OpenScene()
        {
            if (!File.Exists(ScenePath))
            {
                RebuildScene();
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        [MenuItem("IDRPG3D/Local Test/Add Scene To Build Settings", priority = 3)]
        public static void AddSceneToBuildSettings()
        {
            if (!File.Exists(ScenePath))
            {
                RebuildScene();
            }

            var scenes = EditorBuildSettings.scenes;
            foreach (var scene in scenes)
            {
                if (scene.path == ScenePath)
                {
                    Debug.Log($"IDRPG3D local test scene already in build settings: {ScenePath}");
                    return;
                }
            }

            var next = new EditorBuildSettingsScene[scenes.Length + 1];
            scenes.CopyTo(next, 0);
            next[next.Length - 1] = new EditorBuildSettingsScene(ScenePath, true);
            EditorBuildSettings.scenes = next;
            Debug.Log($"IDRPG3D local test scene added to build settings: {ScenePath}");
        }
    }
}
