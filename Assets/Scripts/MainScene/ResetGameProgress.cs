using UnityEngine;
using OptionMenu;
using Utilities;

namespace MainScene
{
    public class ResetGameProgress : MonoBehaviour
    {
        private void OnGUI()
        {
            // Set up a larger, bold font for better visibility
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 24;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.normal.textColor = Color.yellow;

            // Retrieve current reward count
            int currentRewards = PlayerPrefs.GetInt("MainScene_Rewards", 0);

            // Draw the Card Count text at the top center
            GUI.Label(new Rect(Screen.width / 2 - 100, 20, 300, 40), $"Cards Collected: {currentRewards} / 5", labelStyle);

            // Draw a button in the top left corner (X: 20, Y: 20, Width: 180, Height: 40)
            if (GUI.Button(new Rect(20, 20, 180, 40), "RESET ALL PROGRESS"))
            {
                ResetProgress();
            }
        }


        private void ResetProgress()
        {
            // 1. Reset Battle Count (Monsters) to Hound (0)
            var config = Options.LoadConfigData();
            config.BattleCount = 0;
            Options.SaveConfigData(config);

            // 2. Reset Reward count in Main Scene
            PlayerPrefs.SetInt("MainScene_Rewards", 0);
            PlayerPrefs.Save();

            // 3. Reset Deck to Starter Deck using PersistentData
            var starterData = DeckUtility.LoadStarterDeckData();
            if (starterData != null)
            {
                PersistentData.Save(starterData, Battle.General.Constants.PlayerDeckIdentifier);
                Debug.Log("[ResetGameProgress] Deck reset to Starter Deck successfully.");
            }
            else
            {
                Debug.LogError("[ResetGameProgress] Could not load Starter Deck. Deck reset failed.");
            }

            Debug.Log("[ResetGameProgress] ALL PROGRESS HAS BEEN RESET! Next battle will be Hound, and your deck is reset.");
        }
    }
}
