using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.IO;

[InitializeOnLoad]
public class StatHudSetup
{
    static StatHudSetup()
    {
        EditorApplication.delayCall += ApplyStatHudFix;
    }

    static void ApplyStatHudFix()
    {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (SessionState.GetBool("StatHudSetupDone_V4", false))
            return;

        Debug.Log("StatHudSetup: Applying V4 layout changes (even smaller size and spacing)...");

        string[] paths = {
            "Assets/MainScene/UI/MarbleRed.png",
            "Assets/MainScene/UI/MarbleBlue.png",
            "Assets/MainScene/UI/MarblePurple.png"
        };

        Scene scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "Main")
        {
            Debug.LogWarning("StatHudSetup: Please open the 'Main' scene to apply the Stat HUD layout.");
        }

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

            // Adjust spacing
            var layoutGroup = strObj.transform.parent.GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup != null)
            {
                layoutGroup.spacing = 8f; // Decrease spacing
            }
            else
            {
                // Fallback if there is no layout group
                var posR = strObj.GetComponent<RectTransform>().anchoredPosition;
                var posB = intObj.GetComponent<RectTransform>().anchoredPosition;
                var posP = menObj.GetComponent<RectTransform>().anchoredPosition;
                
                posB.x = posR.x + 36f;
                posP.x = posB.x + 36f;
                
                intObj.GetComponent<RectTransform>().anchoredPosition = posB;
                menObj.GetComponent<RectTransform>().anchoredPosition = posP;
            }

            EditorSceneManager.MarkSceneDirty(strObj.scene);
            EditorSceneManager.SaveScene(strObj.scene);
            Debug.Log("StatHudSetup: Successfully applied layout changes!");
            SessionState.SetBool("StatHudSetupDone_V4", true);
        }
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

        // Decrease size to 28x28
        var parentRect = parentObj.GetComponent<RectTransform>();
        parentRect.sizeDelta = new Vector2(28, 28);

        // Find child
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
        
        // Scale proportionally for the mask crop
        childRect.sizeDelta = new Vector2(48, 48); 
    }
}
