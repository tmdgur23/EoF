using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class AutoCopyHudTool
{
    static AutoCopyHudTool()
    {
        EditorApplication.delayCall += Execute;
    }

    public static void Execute()
    {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (SessionState.GetBool("AutoCopyHudDone", false)) return;

        EditorSceneManager.SaveOpenScenes();

        // 1. Open Main Scene and save the parent of STR_Stat as a prefab
        Scene mainScene = EditorSceneManager.OpenScene("Assets/Scenes/Main.unity", OpenSceneMode.Single);
        GameObject strStat = GameObject.Find("STR_Stat");
        if (strStat == null)
        {
            Debug.LogError("Could not find STR_Stat in Main scene.");
            return;
        }

        GameObject statParent = strStat.transform.parent.gameObject;
        string parentName = statParent.name;
        
        string prefabPath = "Assets/TempStatHud.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(statParent, prefabPath);

        // 2. Open Battle Scene
        Scene battleScene = EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity", OpenSceneMode.Single);
        
        // Find matching parent by name if possible (e.g. "PlayerHUD" or whatever it was)
        GameObject targetParent = GameObject.Find(parentName);
        
        // If not found, just use Canvas
        if (targetParent == null)
        {
            foreach (var c in Object.FindObjectsOfType<Canvas>())
            {
                if (c.name == "Canvas") { targetParent = c.gameObject; break; }
            }
            if (targetParent == null) 
            {
                var canvasObj = Object.FindObjectOfType<Canvas>();
                if (canvasObj != null) targetParent = canvasObj.gameObject;
            }
        }

        if (targetParent != null)
        {
            // First, destroy existing one in Battle scene if it has the same name
            Transform existing = targetParent.transform.Find(statParent.name);
            if (existing != null)
            {
                GameObject.DestroyImmediate(existing.gameObject);
            }

            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            inst.transform.SetParent(targetParent.transform, false);
            
            // Unpack the prefab completely
            PrefabUtility.UnpackPrefabInstance(inst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            
            inst.GetComponent<RectTransform>().anchoredPosition = statParent.GetComponent<RectTransform>().anchoredPosition;

            EditorSceneManager.MarkSceneDirty(battleScene);
            EditorSceneManager.SaveScene(battleScene);
            Debug.Log($"Successfully copied Stat HUD to Battle scene under {targetParent.name}!");
            SessionState.SetBool("AutoCopyHudDone", true);
        }
        else
        {
            Debug.LogError("Could not find Canvas or matching parent in Battle scene.");
            SessionState.SetBool("AutoCopyHudDone", true);
        }

        AssetDatabase.DeleteAsset(prefabPath);
        
        // Return to Battle scene since user is there
        EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity", OpenSceneMode.Single);
    }
}
