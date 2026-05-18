using UnityEngine;
using UnityEditor;
using System.IO;

public class ForceTranslateV2
{
    [MenuItem("Tools/Force Translate Tooltips V2")]
    public static void Execute()
    {
        string[] searchPaths = new string[]
        {
            "Assets/Battle/RewardMenu/Reward.prefab"
        };

        foreach (var path in searchPaths)
        {
            if (File.Exists(path))
            {
                string text = File.ReadAllText(path);
                bool modified = false;

                if (text.Contains(">Choice<")) { text = text.Replace(">Choice<", ">선택<"); modified = true; }
                if (text.Contains("Pick a card to add to your collection")) { text = text.Replace("Pick a card to add to your collection", "덱에 추가할 카드를 선택하세요"); modified = true; }

                if (modified)
                {
                    File.WriteAllText(path, text);
                    Debug.Log($"Translated directly in file: {path}");
                }
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("Forced translation V2 complete!");
    }
}
