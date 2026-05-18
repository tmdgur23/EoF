using UnityEditor;
using UnityEngine;
using Battle;

[InitializeOnLoad]
public class AttachBattleStatHUD
{
    static AttachBattleStatHUD()
    {
        EditorApplication.delayCall += Apply;
    }

    [MenuItem("Tools/Attach Battle Stat HUD")]
    static void ApplyManual()
    {
        SessionState.SetBool("AttachBattleStatHUD_V1", false);
        Apply();
    }

    static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (SessionState.GetBool("AttachBattleStatHUD_V1", false)) return;

        string prefabPath = "Assets/Battle/Battle.prefab";
        using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
        {
            var root = scope.prefabContentsRoot;

            // 이미 붙어있으면 스킵
            if (root.GetComponentInChildren<BattleStatHUD>(true) != null)
            {
                Debug.Log("[AttachBattleStatHUD] BattleStatHUD already attached.");
                SessionState.SetBool("AttachBattleStatHUD_V1", true);
                return;
            }

            // Battle 루트 오브젝트에 직접 추가
            root.AddComponent<BattleStatHUD>();
            Debug.Log("[AttachBattleStatHUD] BattleStatHUD attached to Battle prefab root!");
        }

        AssetDatabase.SaveAssets();
        SessionState.SetBool("AttachBattleStatHUD_V1", true);
    }
}
