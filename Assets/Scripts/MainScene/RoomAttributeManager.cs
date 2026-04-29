using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cards.General;
using Battle.RewardMenu;
using Deck;
using Utilities;
using Zenject;

namespace MainScene
{
    public enum RoomConceptType
    {
        Strength_Base, Strength_Bleed, Strength_Combo,
        Intelligence_Analyze, Intelligence_Amplify, Intelligence_Debuff,
        Willpower_Fate, Willpower_Madness, Willpower_Convert
    }

    public class RoomAttributeManager : MonoBehaviour
    {
        public static RoomAttributeManager Instance { get; private set; }

        private Dictionary<int, RoomConceptType> roomConcepts = new Dictionary<int, RoomConceptType>();

        // 카드 매핑 (ID 기준)
        private Dictionary<RoomConceptType, int[]> conceptCardIds = new Dictionary<RoomConceptType, int[]>
        {
            { RoomConceptType.Strength_Base, new int[] { 301, 302, 303 } },
            { RoomConceptType.Strength_Bleed, new int[] { 304, 305, 306 } },
            { RoomConceptType.Strength_Combo, new int[] { 307, 308, 309 } },
            { RoomConceptType.Intelligence_Analyze, new int[] { 310, 311, 312 } },
            { RoomConceptType.Intelligence_Amplify, new int[] { 313, 314, 315 } },
            { RoomConceptType.Intelligence_Debuff, new int[] { 316, 317, 318 } },
            { RoomConceptType.Willpower_Fate, new int[] { 319, 320, 321 } },
            { RoomConceptType.Willpower_Madness, new int[] { 322, 323, 324 } },
            { RoomConceptType.Willpower_Convert, new int[] { 325, 326, 327 } }
        };

        [Header("Reward Configuration")]
        [SerializeField] private GameObject cardPrefab;


        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            AssignRandomAttributes();
        }

        private void AssignRandomAttributes()
        {
            RoomConceptType[] types = (RoomConceptType[])System.Enum.GetValues(typeof(RoomConceptType));
            for (int i = 1; i <= 20; i++)
            {
                RoomConceptType randomType = types[Random.Range(0, types.Length)];
                roomConcepts[i] = randomType;
                Debug.Log($"Room {i} assigned with concept: {randomType}");
            }
        }

        public RoomConceptType GetRoomConcept(int roomIndex)
        {
            if (roomConcepts.TryGetValue(roomIndex, out RoomConceptType type))
            {
                return type;
            }
            return RoomConceptType.Strength_Base; // 기본값
        }

        public void TriggerReward(int roomIndex, int count = 1)
        {
            Debug.Log($"[RoomAttributeManager] TriggerReward started for Room {roomIndex}. Count: {count}");
            
            // UI가 켜져 있어도 카드를 다시 띄울 수 있도록 Safety check 제거 (연속 3번 획득 구현)

            if (!roomConcepts.ContainsKey(roomIndex))
            {
                Debug.LogWarning($"[RoomAttributeManager] Room index {roomIndex} not found in roomConcepts dictionary!");
                return;
            }

            RoomConceptType type = roomConcepts[roomIndex];
            var cardIds = conceptCardIds[type];
            Debug.Log($"[RoomAttributeManager] Concept for room {roomIndex} is {type}. Found {cardIds.Length} mapped card IDs.");
            
            var pool = DeckUtility.LoadPool("CardPools/NewConceptPool"); 
            if (pool == null)
            {
                Debug.LogError("[RoomAttributeManager] Failed to load CardPool 'CardPools/NewConceptPool'!");
                return;
            }

            var filteredCards = pool.Cards.Where(c => cardIds.Contains(c.Id)).ToList();
            Debug.Log($"[RoomAttributeManager] Filtered {filteredCards.Count} cards from pool.");
            
            if (filteredCards.Count == 0)
            {
                Debug.LogWarning("[RoomAttributeManager] No cards matched the filtered criteria. Check card IDs!");
                return;
            }

            var randomSelection = filteredCards.OrderBy(x => Random.value).Take(3).Select(data => new CardInstance(data)).ToList();
            Debug.Log($"[RoomAttributeManager] Selected {randomSelection.Count} random cards for reward.");

            // 1. Precise search for the Reward UI component (even if inactive)
            MainSceneRewardUI rewardUI = Object.FindAnyObjectByType<MainSceneRewardUI>(FindObjectsInactive.Include);
            GameObject rewardObj = null;

            if (rewardUI != null)
            {
                rewardObj = rewardUI.gameObject;
                
                // Cleanup any other accidental duplicates in the scene
                var allRewardUIs = Object.FindObjectsByType<MainSceneRewardUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var ui in allRewardUIs)
                {
                    // If it's on a different GameObject, destroy the GameObject
                    if (ui.gameObject != rewardObj) 
                    {
                        Destroy(ui.gameObject);
                    }
                    else if (ui != rewardUI)
                    {
                        // If it's a second component on the SAME GameObject, just destroy the component
                        Destroy(ui);
                    }
                }
            }
            else
            {
                // Fallback to name-based search if component not found
                var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (var obj in allObjects)
                {
                    if (obj.scene.name != null && obj.hideFlags == HideFlags.None && (obj.name == "Reward" || obj.name == "RewardUI" || obj.name == "RewardMenu"))
                    {
                        if (rewardObj == null) rewardObj = obj;
                        else Destroy(obj);
                    }
                }
            }

            if (rewardObj != null)
            {
                // 2. Ensure MainSceneRewardUI is present
                rewardUI = rewardObj.GetComponent<MainSceneRewardUI>() ?? rewardObj.AddComponent<MainSceneRewardUI>();
                
                // 3. Sync references
                rewardUI.SetCardPrefab(cardPrefab);
                
                // 4. Open it!
                rewardUI.OpenReward(randomSelection, roomIndex, count);
                Debug.Log($"[RoomAttributeManager] Successfully opened Reward UI on object: {rewardObj.name}");
            }
            else
            {
                Debug.LogError("[RoomAttributeManager] Could not find any GameObject named 'Reward' or 'RewardUI' in the scene! Please ensure the prefab is in the hierarchy.");
            }
        }
    }
}
