using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.IO;

[InitializeOnLoad]
public class FontSetup
{
    static FontSetup()
    {
        EditorApplication.delayCall += ApplyFontToBattleSceneAndPrefabs;
    }

    static void ApplyFontToBattleSceneAndPrefabs()
    {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (SessionState.GetBool("BattleFontApplied_V1", false))
            return;

        string fontPath = "Assets/Fonts/BlackAndWhitePicture-Regular SDF.asset";
        TMP_FontAsset koreanFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);

        if (koreanFont == null)
        {
            Debug.LogWarning($"FontSetup: Could not find font at {fontPath}");
            return;
        }

        Debug.Log("FontSetup: Starting to apply Korean font to Battle scene and prefabs...");

        // 1. Update Prefabs in Assets folder
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            using (var editScope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                var prefabRoot = editScope.prefabContentsRoot;
                bool modified = false;

                var textsUI = prefabRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in textsUI)
                {
                    if (t.font != koreanFont)
                    {
                        t.font = koreanFont;
                        modified = true;
                    }
                }

                var texts3D = prefabRoot.GetComponentsInChildren<TextMeshPro>(true);
                foreach (var t in texts3D)
                {
                    if (t.font != koreanFont)
                    {
                        t.font = koreanFont;
                        modified = true;
                    }
                }
            }
        }

        // 2. Update Battle Scene
        string currentScenePath = EditorSceneManager.GetActiveScene().path;
        string battleScenePath = "Assets/Scenes/Battle.unity";

        if (File.Exists(battleScenePath))
        {
            // Save current scene first
            EditorSceneManager.SaveOpenScenes();

            Scene battleScene = EditorSceneManager.OpenScene(battleScenePath, OpenSceneMode.Single);

            bool sceneModified = false;
            var textsUI = Object.FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var t in textsUI)
            {
                if (t.font != koreanFont)
                {
                    t.font = koreanFont;
                    sceneModified = true;
                }
            }

            var texts3D = Object.FindObjectsOfType<TextMeshPro>(true);
            foreach (var t in texts3D)
            {
                if (t.font != koreanFont)
                {
                    t.font = koreanFont;
                    sceneModified = true;
                }
            }

            if (sceneModified)
            {
                EditorSceneManager.MarkSceneDirty(battleScene);
                EditorSceneManager.SaveScene(battleScene);
            }

            // Return to previous scene
            if (!string.IsNullOrEmpty(currentScenePath) && currentScenePath != battleScenePath)
            {
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
            }
        }

        Debug.Log("FontSetup: Successfully applied Korean font everywhere!");
        SessionState.SetBool("BattleFontApplied_V1", true);
    }
}
