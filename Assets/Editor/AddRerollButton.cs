using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Battle;

public class RerollButtonEditorAdder
{
    [MenuItem("Tools/Add Reroll Button To Scene")]
    public static void ImplementRerollButton()
    {
        // Find existing EndTurnButton anywhere in the open scene
        var endTurnButton = GameObject.FindObjectOfType<EndTurnButton>(true);
        if (endTurnButton == null)
        {
            Debug.LogError("Could not find EndTurnButton in the open scene!");
            return;
        }

        // Duplicate it
        var rerollObj = GameObject.Instantiate(endTurnButton.gameObject, endTurnButton.transform.parent);
        rerollObj.name = "RerollBtn";

        // Remove the EndTurnButton component, so it doesn't do "End Turn" related behavior
        var oldScript = rerollObj.GetComponent<EndTurnButton>();
        if (oldScript != null)
        {
            GameObject.DestroyImmediate(oldScript);
        }

        // Clear Persistent Listeners on Button created in the Inspector (which is what triggered the enemy attack!)
        var btn = rerollObj.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick = new Button.ButtonClickedEvent();
        }

        // Attach our new logic script
        rerollObj.AddComponent<RerollButton>();

        // Update Text
        var txt = rerollObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (txt != null)
        {
            txt.text = "Reroll";
        }

        // Move position slightly above the End Turn button
        var rect = rerollObj.GetComponent<RectTransform>();
        rect.anchoredPosition = endTurnButton.GetComponent<RectTransform>().anchoredPosition + new Vector2(0, 100);

        // Mark scene dirty so user can save it
        EditorSceneManager.MarkSceneDirty(endTurnButton.gameObject.scene);

        Debug.Log("Successfully added RerollBtn above EndTurnBtn!");
    }
}
