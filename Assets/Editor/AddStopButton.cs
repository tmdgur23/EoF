using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Battle;

public class StopButtonEditorAdder
{
    [MenuItem("Tools/Add Stop Button To Scene")]
    public static void ImplementStopButton()
    {
        // Find existing EndTurnButton anywhere in the open scene
        var endTurnButton = GameObject.FindObjectOfType<EndTurnButton>(true);
        if (endTurnButton == null)
        {
            Debug.LogError("Could not find EndTurnButton in the open scene!");
            return;
        }

        // Duplicate it
        var stopObj = GameObject.Instantiate(endTurnButton.gameObject, endTurnButton.transform.parent);
        stopObj.name = "StopBtn";

        // Remove the EndTurnButton component, so it doesn't do "End Turn" related behavior
        var oldScript = stopObj.GetComponent<EndTurnButton>();
        if (oldScript != null)
        {
            GameObject.DestroyImmediate(oldScript);
        }

        // Clear Persistent Listeners on Button created in the Inspector
        var btn = stopObj.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick = new Button.ButtonClickedEvent();
        }

        // Attach our new logic script
        stopObj.AddComponent<StopButton>();

        // Update Text
        var txt = stopObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (txt != null)
        {
            txt.text = "Stop";
        }

        // Move position slightly above the End Turn button, next to Reroll Button
        var rect = stopObj.GetComponent<RectTransform>();
        rect.anchoredPosition = endTurnButton.GetComponent<RectTransform>().anchoredPosition + new Vector2(0, 50);

        // Mark scene dirty so user can save it
        EditorSceneManager.MarkSceneDirty(endTurnButton.gameObject.scene);

        Debug.Log("Successfully added StopBtn above EndTurnBtn!");
    }
}
