using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public static class CopyHudTool
{
    [MenuItem("Tools/Copy Stat HUD to Battle Scene")]
    public static void Execute()
    {
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
        string prefabPath = "Assets/TempStatHud.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(statParent, prefabPath);

        // 2. Open Battle Scene
        Scene battleScene = EditorSceneManager.OpenScene("Assets/Scenes/Battle.unity", OpenSceneMode.Single);
        
        // We need to place it in the same hierarchy path if possible.
        // Let's find the main Canvas.
        Canvas mainCanvas = null;
        foreach (var c in Object.FindObjectsOfType<Canvas>())
        {
            if (c.name == "Canvas") { mainCanvas = c; break; }
        }

        if (mainCanvas == null) mainCanvas = Object.FindObjectOfType<Canvas>();

        if (mainCanvas != null)
        {
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            inst.transform.SetParent(mainCanvas.transform, false);
            
            // Unpack the prefab completely so it becomes independent
            PrefabUtility.UnpackPrefabInstance(inst, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            
            // Position it matching the original
            inst.GetComponent<RectTransform>().anchoredPosition = statParent.GetComponent<RectTransform>().anchoredPosition;

            EditorSceneManager.MarkSceneDirty(battleScene);
            EditorSceneManager.SaveScene(battleScene);
            Debug.Log("Successfully copied Stat HUD to Battle scene under Canvas!");
        }
        else
        {
            Debug.LogError("Could not find Canvas in Battle scene.");
        }

        AssetDatabase.DeleteAsset(prefabPath);
    }
}
