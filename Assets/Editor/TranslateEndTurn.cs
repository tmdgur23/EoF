using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class TranslateEndTurn
{
    static TranslateEndTurn()
    {
        EditorApplication.delayCall += DoTranslation;
    }

    static void DoTranslation()
    {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (SessionState.GetBool("TranslatedEndTurn_V1", false)) return;
        
        string prefabPath = "Assets/Battle/EndTurnBtn.prefab";
        if (System.IO.File.Exists(prefabPath))
        {
            using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
            {
                var texts = scope.prefabContentsRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
                bool prefabModified = false;
                foreach (var t in texts)
                {
                    if (t.text == "End Turn" || t.text.Contains("End Turn"))
                    {
                        t.text = "??종료";
                        prefabModified = true;
                    }
                }
            }
            Debug.Log("Translated End Turn button in prefab!");
        }

        Scene currentScene = EditorSceneManager.GetActiveScene();
        bool modified = false;
        var sceneTexts = Object.FindObjectsOfType<TextMeshProUGUI>(true);
        foreach (var t in sceneTexts)
        {
            if (t.text == "End Turn" || t.text.Contains("End Turn"))
            {
                t.text = "??종료";
                modified = true;
            }
        }
        
        if (modified)
        {
            EditorSceneManager.MarkSceneDirty(currentScene);
            EditorSceneManager.SaveScene(currentScene);
            Debug.Log("Translated End Turn button in scene!");
        }

        SessionState.SetBool("TranslatedEndTurn_V1", true);
    }
}
