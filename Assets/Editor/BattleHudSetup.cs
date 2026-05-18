using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.IO;

[InitializeOnLoad]
public class BattleHudSetup
{
    static BattleHudSetup()
    {
        EditorApplication.delayCall += ApplyStatHudToBattle;
    }

    static void ApplyStatHudToBattle()
    {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (SessionState.GetBool("BattleHudSetupDone_V1", false))
            return;

        Debug.Log("BattleHudSetup: Applying HUD changes to Battle Scene...");

        string currentScenePath = EditorSceneManager.GetActiveScene().path;
        string battleScenePath = "Assets/Scenes/Battle.unity";

        if (File.Exists(battleScenePath))
        {
            EditorSceneManager.SaveOpenScenes();
            Scene battleScene = EditorSceneManager.OpenScene(battleScenePath, OpenSceneMode.Single);

            GameObject strObj = GameObject.Find("STR_Stat");
            GameObject intObj = GameObject.Find("INT_Stat");
            GameObject menObj = GameObject.Find("MEN_Stat");

            if (strObj != null && intObj != null && menObj != null)
            {
                Sprite redSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/MainScene/UI/MarbleRed.png");
                Sprite blueSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/MainScene/UI/MarbleBlue.png");
                Sprite purpleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/MainScene/UI/MarblePurple.png");
                Sprite circleMask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

                SetupOrb(strObj, redSprite, circleMask);
                SetupOrb(intObj, blueSprite, circleMask);
                SetupOrb(menObj, purpleSprite, circleMask);

                var layoutGroup = strObj.transform.parent.GetComponent<HorizontalLayoutGroup>();
                if (layoutGroup != null)
                {
                    layoutGroup.spacing = 8f;
                }
                else
                {
                    var posR = strObj.GetComponent<RectTransform>().anchoredPosition;
                    var posB = intObj.GetComponent<RectTransform>().anchoredPosition;
                    var posP = menObj.GetComponent<RectTransform>().anchoredPosition;
                    
                    posB.x = posR.x + 36f;
                    posP.x = posB.x + 36f;
                    
                    intObj.GetComponent<RectTransform>().anchoredPosition = posB;
                    menObj.GetComponent<RectTransform>().anchoredPosition = posP;
                }

                EditorSceneManager.MarkSceneDirty(battleScene);
                EditorSceneManager.SaveScene(battleScene);
                Debug.Log("BattleHudSetup: Successfully applied layout to Battle scene!");
            }
            else
            {
                Debug.LogWarning("BattleHudSetup: Could not find STR_Stat, INT_Stat, or MEN_Stat in Battle scene.");
            }

            if (!string.IsNullOrEmpty(currentScenePath) && currentScenePath != battleScenePath)
            {
                EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
            }
        }

        SessionState.SetBool("BattleHudSetupDone_V1", true);
    }

    static void SetupOrb(GameObject parentObj, Sprite marbleSprite, Sprite maskSprite)
    {
        var parentImg = parentObj.GetComponent<Image>();
        if (parentImg == null) parentImg = parentObj.AddComponent<Image>();
        parentImg.sprite = maskSprite;
        parentImg.color = Color.white;

        var mask = parentObj.GetComponent<Mask>();
        if (mask == null) mask = parentObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        var parentRect = parentObj.GetComponent<RectTransform>();
        parentRect.sizeDelta = new Vector2(28, 28);

        Transform childTransform = parentObj.transform.Find("MarbleImage");
        GameObject childObj;
        
        if (childTransform != null)
        {
            childObj = childTransform.gameObject;
        }
        else
        {
            childObj = new GameObject("MarbleImage");
            childObj.transform.SetParent(parentObj.transform, false);
        }

        var childImg = childObj.GetComponent<Image>();
        if (childImg == null) childImg = childObj.AddComponent<Image>();
        childImg.sprite = marbleSprite;
        childImg.color = Color.white;

        var childRect = childObj.GetComponent<RectTransform>();
        childRect.anchorMin = new Vector2(0.5f, 0.5f);
        childRect.anchorMax = new Vector2(0.5f, 0.5f);
        childRect.pivot = new Vector2(0.5f, 0.5f);
        childRect.anchoredPosition = Vector2.zero;
        childRect.sizeDelta = new Vector2(48, 48); 
    }
}
