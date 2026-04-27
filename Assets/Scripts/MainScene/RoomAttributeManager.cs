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
    public enum AttributeType
    {
        Strength,     // 근력
        Intelligence, // 지식
        Willpower     // 정신력
    }

    public class RoomAttributeManager : MonoBehaviour
    {
        public static RoomAttributeManager Instance { get; private set; }

        private Dictionary<int, AttributeType> roomAttributes = new Dictionary<int, AttributeType>();

        // 카드 매핑 (ID 기준)
        private Dictionary<AttributeType, int[]> attributeCardIds = new Dictionary<AttributeType, int[]>
        {
            { AttributeType.Strength, new int[] { 301, 302, 303, 304, 305, 306, 307, 308, 309 } },
            { AttributeType.Intelligence, new int[] { 310, 311, 312, 313, 314, 315, 316, 317, 318 } },
            { AttributeType.Willpower, new int[] { 319, 320, 321, 322, 323, 324, 325, 326, 327 } }
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
            AttributeType[] types = (AttributeType[])System.Enum.GetValues(typeof(AttributeType));
            for (int i = 1; i <= 20; i++)
            {
                AttributeType randomType = types[Random.Range(0, types.Length)];
                roomAttributes[i] = randomType;
                Debug.Log($"Room {i} assigned with: {randomType}");
            }
        }

        public void TriggerReward(int roomIndex)
        {
            Debug.Log($"[RoomAttributeManager] TriggerReward started for Room {roomIndex}");
            
            // Safety check: Don't open if already open
            if (MainSceneRewardUI.Instance != null && MainSceneRewardUI.Instance.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("[RoomAttributeManager] Reward UI is already active. Ignoring trigger.");
                return;
            }

            if (!roomAttributes.ContainsKey(roomIndex))
            {
                Debug.LogWarning($"[RoomAttributeManager] Room index {roomIndex} not found in roomAttributes dictionary!");
                return;
            }

            AttributeType type = roomAttributes[roomIndex];
            var cardIds = attributeCardIds[type];
            Debug.Log($"[RoomAttributeManager] Attributes for room {roomIndex} is {type}. Found {cardIds.Length} mapped card IDs.");
            
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
                rewardUI.OpenReward(randomSelection);
                Debug.Log($"[RoomAttributeManager] Successfully opened Reward UI on object: {rewardObj.name}");
            }
            else
            {
                Debug.LogError("[RoomAttributeManager] Could not find any GameObject named 'Reward' or 'RewardUI' in the scene! Please ensure the prefab is in the hierarchy.");
            }
        }
    }
}
