using UnityEngine;
using OptionMenu;
using Utilities;
using Deck;
using Units.Player.General;

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

            // Draw the Route count text at the top center
            int routeCount = 1;
            if (RoomExplorationManager.Instance != null)
            {
                routeCount = RoomExplorationManager.Instance.currentLoopCount + 1;
                if (routeCount > 3) routeCount = 3;
            }

            GUI.Label(new Rect(Screen.width / 2 - 100, 20, 300, 40), $"Route: {routeCount} / 3", labelStyle);

            // Draw a button in the top right corner
            if (GUI.Button(new Rect(Screen.width - 200, 20, 180, 40), "RESET ALL PROGRESS"))
            {
                ResetProgress();
            }
        }


        private void ResetProgress()
        {
            // 1. Reset Battle Count (Monsters) to Hound (0)
            var config = Options.LoadConfigData();
            config.BattleCount = 0;
            
            // 체력과 소울 초기화
            config.Health = new Utilities.Range(40, 40); // 최소(현재) 40, 최대 40
            config.Soul = 0;
            
            Options.SaveConfigData(config);

            // UI 즉시 업데이트 (체력바, 소울바 새로고침)
            if (MainSceneHUD.Instance != null)
            {
                MainSceneHUD.Instance.UpdateUI();
            }

            // 2. Reset Loop Count in Main Scene
            if (RoomExplorationManager.Instance != null)
            {
                RoomExplorationManager.Instance.currentLoopCount = 0;
            }

            // 3. Reset Deck to Starter Deck using PersistentData
            var starterData = DeckUtility.LoadStarterDeckData();
            if (starterData != null)
            {
                PersistentData.Save(starterData, Battle.General.Constants.PlayerDeckIdentifier);
                Debug.Log("[ResetGameProgress] Deck reset to Starter Deck successfully.");

                // 4. Update the active Player instance's deck in memory!
                var player = Object.FindObjectOfType<Units.Player.General.Player>();
                if (player != null && player.CardDeck != null)
                {
                    player.CardDeck.Clear();
                    
                    // Re-build deck from starter data
                    var rebuiltDeck = DeckFactory.Build(starterData, player);
                    foreach (var card in rebuiltDeck.GetAll())
                    {
                        player.CardDeck.Add(card);
                    }
                    Debug.Log("[ResetGameProgress] Active Player's memory deck has been reset.");
                }
            }
            else
            {
                Debug.LogError("[ResetGameProgress] Could not load Starter Deck. Deck reset failed.");
            }

            // 5. 메인 씬 카드 덱 뷰어 카운터 갱신
            if (MainSceneDeckViewer.Instance != null)
            {
                MainSceneDeckViewer.Instance.UpdateCounter();
            }

            Debug.Log("[ResetGameProgress] ALL PROGRESS HAS BEEN RESET! Next battle will be Hound, and your deck is reset.");
        }
    }
}
