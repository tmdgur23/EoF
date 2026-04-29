using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Deck;
using Utilities;
using Cards.General;

namespace MainScene
{
    [RequireComponent(typeof(Button))]
    public class MainSceneDeckViewer : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("메인 씬 캔버스에 올려둔 PileCardOverview 프리팹 안의 CardCollectionViewOpener를 연결하세요.")]
        public CardCollectionViewOpener cardViewer;
        
        [Tooltip("덱 아이콘 아래에 있는 숫자 텍스트(TextMeshProUGUI)를 연결하세요.")]
        public TextMeshProUGUI counterText;

        public static MainSceneDeckViewer Instance { get; private set; }

        private Button button;
        private List<CardPool> allPools;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnDeckClicked);
            }

            // 모든 카드풀(Resources/CardPools) 미리 로드
            allPools = new List<CardPool>(DeckUtility.LoadAllPools());

            // 처음에 한 번 덱 장수 갱신
            UpdateCounter();
        }

        /// <summary>
        /// 다른 UI(카드 보상 등)에서 덱에 변경이 생겼을 때 숫자를 갱신합니다.
        /// </summary>
        public void UpdateCounter()
        {
            var deckData = DeckUtility.LoadSavedDeckData();
            int totalCards = 0;

            if (deckData != null && deckData.Cards != null && allPools != null)
            {
                foreach (var savedCard in deckData.Cards)
                {
                    // 풀에 존재하는 유효한 카드인지 확인
                    bool isValidCard = false;
                    foreach (var pool in allPools)
                    {
                        if (pool.Cards.Find(c => c.Id == savedCard.CardId) != null)
                        {
                            isValidCard = true;
                            break;
                        }
                    }

                    // 유효한 카드일 경우에만 장수 추가
                    if (isValidCard)
                    {
                        totalCards += savedCard.Count;
                    }
                }
            }
            
            if (counterText != null)
            {
                counterText.text = totalCards.ToString();
            }
        }

        private void OnDeckClicked()
        {
            if (cardViewer == null)
            {
                Debug.LogError("[MainSceneDeckViewer] CardCollectionViewOpener (PileCardOverview) 가 연결되지 않았습니다!");
                return;
            }

            var deckData = DeckUtility.LoadSavedDeckData();
            if (deckData == null || deckData.Cards == null)
            {
                Debug.LogWarning("[MainSceneDeckViewer] 저장된 덱 데이터가 없습니다.");
                return;
            }

            List<CardInstance> deckInstances = new List<CardInstance>();

            // 저장된 DeckSaveData(카드 ID와 개수)를 실제 CardInstance 로 변환합니다.
            foreach (var savedCard in deckData.Cards)
            {
                CardData cardData = null;
                foreach (var pool in allPools)
                {
                    cardData = pool.Cards.Find(c => c.Id == savedCard.CardId);
                    if (cardData != null) break;
                }

                if (cardData != null)
                {
                    // Count만큼 인스턴스를 생성해 리스트에 넣습니다.
                    for (int i = 0; i < savedCard.Count; i++)
                    {
                        var inst = new CardInstance(cardData);
                        inst.CostReduction = savedCard.CostReduction;
                        deckInstances.Add(inst);
                    }
                }
            }

            // UI 열기
            cardViewer.SetHeader("My Deck");
            cardViewer.Open(deckInstances, null, true);

            // 카드를 보는 동안 3D 클릭 방지용으로 커서 가시화
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void OpenForRemoval(int count = 1)
        {
            if (cardViewer == null) return;

            var deckInstances = GetDeckInstances();
            if (deckInstances.Count == 0) return;

            cardViewer.SetHeader($"Select a card to REMOVE (Remaining: {count})");
            cardViewer.Open(deckInstances, (cardModel) => 
            {
                RemoveCardFromSaveData(cardModel.Instance);
                UpdateCounter();
                
                cardViewer.Close();
                
                if (count > 1)
                {
                    OpenForRemoval(count - 1);
                }
                else if (RoomExplorationManager.Instance != null && RoomExplorationManager.Instance.currentRoomInteractions >= 5)
                {
                    RoomExplorationManager.Instance.ExitRoomOrBattle();
                }
            }, true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void OpenForUpgrade(int count = 1)
        {
            if (cardViewer == null) return;

            var deckInstances = GetDeckInstances();
            if (deckInstances.Count == 0) return;

            cardViewer.SetHeader($"Select a card to UPGRADE (Cost -1) (Remaining: {count})");
            cardViewer.Open(deckInstances, (cardModel) => 
            {
                UpgradeCardInSaveData(cardModel.Instance);
                UpdateCounter();
                
                cardViewer.Close();
                
                if (count > 1)
                {
                    OpenForUpgrade(count - 1);
                }
                else if (RoomExplorationManager.Instance != null && RoomExplorationManager.Instance.currentRoomInteractions >= 5)
                {
                    RoomExplorationManager.Instance.ExitRoomOrBattle();
                }
            }, true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private List<CardInstance> GetDeckInstances()
        {
            List<CardInstance> deckInstances = new List<CardInstance>();
            var deckData = DeckUtility.LoadSavedDeckData();
            if (deckData == null || deckData.Cards == null) return deckInstances;

            foreach (var savedCard in deckData.Cards)
            {
                CardData cardData = null;
                foreach (var pool in allPools)
                {
                    cardData = pool.Cards.Find(c => c.Id == savedCard.CardId);
                    if (cardData != null) break;
                }

                if (cardData != null)
                {
                    for (int i = 0; i < savedCard.Count; i++)
                    {
                        var inst = new CardInstance(cardData);
                        inst.CostReduction = savedCard.CostReduction;
                        deckInstances.Add(inst);
                    }
                }
            }
            return deckInstances;
        }

        private void RemoveCardFromSaveData(CardInstance card)
        {
            var deckData = DeckUtility.LoadSavedDeckData();
            if (deckData == null) return;

            int index = deckData.Cards.FindIndex(x => x.CardId == card.CardData.Id && x.CostReduction == card.CostReduction);
            if (index != -1)
            {
                deckData.Cards[index].Count--;
                if (deckData.Cards[index].Count <= 0)
                {
                    deckData.Cards.RemoveAt(index);
                }
                PersistentData.Save(deckData, Battle.General.Constants.PlayerDeckIdentifier);
                Debug.Log($"[DeckViewer] Removed card {card.CardData.Name} (CostReduction: {card.CostReduction}) from deck.");
            }
        }

        private void UpgradeCardInSaveData(CardInstance card)
        {
            var deckData = DeckUtility.LoadSavedDeckData();
            if (deckData == null) return;

            int index = deckData.Cards.FindIndex(x => x.CardId == card.CardData.Id && x.CostReduction == card.CostReduction);
            if (index != -1)
            {
                // Decrease original count
                deckData.Cards[index].Count--;
                if (deckData.Cards[index].Count <= 0)
                {
                    deckData.Cards.RemoveAt(index);
                }

                // Add upgraded version
                int newCostReduction = card.CostReduction + 1;
                int upgradedIndex = deckData.Cards.FindIndex(x => x.CardId == card.CardData.Id && x.CostReduction == newCostReduction);
                
                if (upgradedIndex != -1)
                {
                    deckData.Cards[upgradedIndex].Count++;
                }
                else
                {
                    deckData.Cards.Add(new CardSaveData() { CardId = card.CardData.Id, Count = 1, CostReduction = newCostReduction });
                }

                PersistentData.Save(deckData, Battle.General.Constants.PlayerDeckIdentifier);
                Debug.Log($"[DeckViewer] Upgraded card {card.CardData.Name}. New CostReduction: {newCostReduction}");
            }
        }
    }
}
